using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Launchpad
{    
    public class LaunchpadDevice : IDisposable
    {        
        public event Action<IReadOnlyList<LaunchpadEvent>> Tick;

        private readonly MidiDevice _device;
        private readonly DeviceInfo _info;
        private readonly MidiPosition[,] _posMap;
        private readonly MidiPosition[] _midiMap, _systemButtonMap;
        private readonly IRenderer _renderer;
        
        private SemaphoreSlim _sendLock;    
        private ConcurrentQueue<LaunchpadEvent> _queuedEvents;
        private Task _task;
        private CancellationTokenSource _cancelToken;  

        public bool IsRunning { get; private set; }

        public string Id => _device.Id;
        public DeviceType Type => _device.Type;
        public bool IsConnected => _device.IsConnected;
        public int Width => _info.Width;
        public int Height => _info.Height;
        public int InnerOffsetX => _info.InnerOffsetX;
        public int InnerOffsetY => _info.InnerOffsetY;
        public int LightCount => _info.LightCount;

        public LaunchpadDevice(MidiDevice device)
        {
            _device = device;
            _sendLock = new SemaphoreSlim(1, 1);
            _info = DeviceInfo.FromType(device.Type);
            _renderer = _info.RendererFactory(_device);
            
            // Cache id lookups
            _posMap = new MidiPosition[Width,Height];
            _midiMap = new MidiPosition[256];
            _systemButtonMap = new MidiPosition[256];

            var empty = new MidiPosition(255, 255, 255, 255, 255, null);
            for (byte y = 0; y < Height; y++)
            {
                for (byte x = 0; x < Width; x++)
                    _posMap[x, y] = empty;
            }
            for (int i = 0; i < _midiMap.Length; i++)
                _midiMap[i] = empty;
            for (int i = 0; i < _systemButtonMap.Length; i++)
                _systemButtonMap[i] = empty;

            for (byte y = 0; y < Height; y++)
            {
                for (byte x = 0; x < Width; x++)
                {
                    byte midi = _info.MidiLayout[Height - y - 1, x];
                    if (midi != 255)
                    {
                        var systemButton = _info.SystemButtons.TryGetValue(midi, out var systemButtonVal) ? systemButtonVal : (SystemButton?)null;
                        byte buttonX = x >= _info.InnerOffsetX && x < _info.InnerOffsetX + 8 ? (byte)(x - _info.InnerOffsetX) : (byte)255;
                        byte buttonY = y >= _info.InnerOffsetY && y < _info.InnerOffsetY + 8 ? (byte)(y - _info.InnerOffsetY) : (byte)255;
                        var info = new MidiPosition(midi, buttonX, buttonY, x, y, systemButton);
                        _posMap[x, y] = info;
                        _midiMap[info.Midi] = info;
                        if (systemButton.HasValue)
                            _systemButtonMap[(byte)info.SystemButton.Value] = info;
                    }
                }
            }

            // Register events
            _device.ButtonDown += (type, midiId) =>
            {
                if (type == MidiMessageType.ControlModeChange && (Type == DeviceType.LaunchpadS || Type == DeviceType.LaunchpadMini))
                    midiId += 100; // Adjust for overlapping midi codes
                _queuedEvents.Enqueue(new LaunchpadEvent(EventType.ButtonDown, _midiMap[midiId]));
            };
            _device.ButtonUp += (type, midiId) =>
            {
                if (type == MidiMessageType.ControlModeChange && (Type == DeviceType.LaunchpadS || Type == DeviceType.LaunchpadMini))
                    midiId += 100; // Adjust for overlapping midi codes
                _queuedEvents.Enqueue(new LaunchpadEvent(EventType.ButtonUp, _midiMap[midiId]));
            };
        }
        public void Dispose()
        {
            Stop();
        }

        public void Start(int bpm, int tps)
        {
            Stop();
            _queuedEvents = new ConcurrentQueue<LaunchpadEvent>();
            _cancelToken = new CancellationTokenSource();
            _task = Task.Run(async () =>
            {
                IsRunning = true;
                var clockTask = RunClockTask(bpm, _cancelToken.Token);
                var logicTask = RunLogicTask(tps, _cancelToken.Token);
                await Task.WhenAll(clockTask, logicTask).ConfigureAwait(false);
                IsRunning = false;
            });
        }
        public void Stop()
        { 
            if (_task != null)
            {
                _cancelToken.Cancel();
                _task.GetAwaiter().GetResult();
                _task = null;
            }
            _device.Disconnect();
        }

        private Task RunClockTask(int bpm, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var tickLength = TimeSpan.FromSeconds((1.0 / bpm) * 24); // Sent at 24 ppqn (pulses per quarter note)
                var nextTick = DateTimeOffset.UtcNow + tickLength;
                while (!cancelToken.IsCancellationRequested)
                {
                    //Limit loop to bpm rate
                    var now = DateTimeOffset.UtcNow;
                    if (now < nextTick)
                    {
                        await Task.Delay(nextTick - now);
                        continue;
                    }
                    nextTick += tickLength;

                    //If not connected to the device, sleep
                    if (!IsConnected)
                        continue;

                    await _sendLock.WaitAsync(cancelToken).ConfigureAwait(false);
                    try { _renderer.ClockTick(); }
                    finally { _sendLock.Release(); }
                }
            });
        }
        private Task RunLogicTask(int tps, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                var events = new List<LaunchpadEvent>();

                var tickLength = TimeSpan.FromSeconds(1.0 / tps);
                var nextTick = DateTimeOffset.UtcNow + tickLength;
                while (!cancelToken.IsCancellationRequested)
                {
                    //Limit loop to tps rate
                    var now = DateTimeOffset.UtcNow;
                    if (now < nextTick)
                    {
                        await Task.Delay(nextTick - now);
                        continue;
                    }
                    nextTick += tickLength;

                    //If not connected to the device, reconnect
                    if (!IsConnected)
                    {
                        if (!_device.Connect())
                            continue;
                    }

                    _device.Update();

                    //Fetch all queued events for this tick
                    events.Clear();
                    while (_queuedEvents.TryDequeue(out var evnt))
                        events.Add(evnt);
                    Tick?.Invoke(events);

                    await _sendLock.WaitAsync(cancelToken).ConfigureAwait(false);
                    try { _renderer.Render(); }
                    finally { _sendLock.Release(); }
                }
            });
        }

        public void Clear()
            => _renderer.Clear();

        public void Set(int x, int y, Light light)
            => _renderer.Set(_posMap[x, y].Midi, light);
        public void Set(SystemButton button, Light light)
            => _renderer.Set(_systemButtonMap[(byte)button].Midi, light);
        public void Set(int x, int y, byte color)
            => _renderer.Set(_posMap[x, y].Midi, color);
        public void Set(SystemButton button, byte color)
            => _renderer.Set(_systemButtonMap[(byte)button].Midi, color);
        public void Set(int x, int y, byte red, byte green, byte blue)
            => _renderer.Set(_posMap[x, y].Midi, red, green, blue);
        public void Set(SystemButton button, byte red, byte green, byte blue)
            => _renderer.Set(_systemButtonMap[(byte)button].Midi, red, green, blue);
        public void SetOff(int x, int y)
            => _renderer.SetOff(_posMap[x, y].Midi);
        public void SetOff(SystemButton button)
            => _renderer.SetOff(_systemButtonMap[(byte)button].Midi);
        public void SetPulse(int x, int y, byte color)
            => _renderer.SetPulse(_posMap[x, y].Midi, color);  
        public void SetPulse(SystemButton button, byte color)
            => _renderer.SetPulse(_systemButtonMap[(byte)button].Midi, color);        
        public void SetFlash(int x, int y, byte color1, byte color2)
            => _renderer.SetFlash(_posMap[x, y].Midi, color1, color2);
        public void SetFlash(SystemButton button, byte color1, byte color2)
            => _renderer.SetFlash(_systemButtonMap[(byte)button].Midi, color1, color2);

        public void SetRow(int x, int y1, int y2, byte color)
        {
            if(y1 > y2)
            {
                int y3 = y1;
                y1 = y2;
                y2 = y1;
            }
            for(int y=y1; y < y2; y++)
            {
                Set(x, y, color);
            }
        }

        public void SetLine(int y, int x1, int x2, byte color)
        {
            if (x1 > x2)
            {
                int x3 = x1;
                x1 = x2;
                x2 = x1;
            }
            for (int x = x1; x < x2; x++)
            {
                Set(x, y, color);
            }
        }
        public void SetRow(int x, int y1, int y2, byte red, byte green, byte blue = 0)
        {
            if (y1 > y2)
            {
                int y3 = y1;
                y1 = y2;
                y2 = y1;
            }
            for (int y = y1; y <= y2; y++)
            {
                Set(x, y, red, green ,blue);
            }
        }

        public void SetLine(int y, int x1, int x2, byte red, byte green, byte blue = 0)
        {
            if (x1 > x2)
            {
                int x3 = x1;
                x1 = x2;
                x2 = x1;
            }
            for (int x = x1; x <= x2; x++)
            {
                Set(x, y, red, green, blue);
            }
        }

    }
}