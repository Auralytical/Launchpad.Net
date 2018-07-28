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
        public const int MaxLEDCount = 96;
        public const int MaxWidth = 10;
        public const int MaxHeight = 10;
        public const int MaxInnerWidth = 8;
        public const int MaxInnerHeight = 8;

        private static IAudioEngine _engine;

        public static IReadOnlyList<MidiDeviceInfo> AvailableDevices { get; private set; }

        static LaunchpadDevice()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                SetEngine(EngineType.Winmm);
            else //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                SetEngine(EngineType.Alsa);
        }
        public static void SetEngine(EngineType type)
        {
            switch (type)
            {
                case EngineType.Winmm: _engine = Winmm.WinmmAudioEngine.Instance; break;
                case EngineType.Alsa: _engine = Alsa.AlsaAudioEngine.Instance; break;
                case EngineType.Emulator: _engine = Emulator.EmulatorAudioEngine.Instance; break;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
            RefreshDevices();
        }

        public static void RefreshDevices() => AvailableDevices = _engine.ListLaunchpadDevices();
        public static LaunchpadDevice For(MidiDeviceInfo device) 
        {
            if (device.Type == MidiDeviceType.None)
                return null;
            return new LaunchpadDevice(_engine.GetMidiDevice(device));
        } 

        public event Action<IReadOnlyList<LaunchpadEvent>> Tick;

        private readonly RawMidiDevice _device;

        private ConcurrentQueue<LaunchpadEvent> _queuedEvents;
        private Task _task;
        private CancellationTokenSource _cancelToken;

        private byte[] _normalMsg, _pulseMsg, _flashMsg;
        private LED[] _leds, _oldLeds;
        private bool _ledsInvalidated;

        public bool IsRunning { get; private set; }

        public string DeviceId => _device.Id;
        public MidiDeviceType DeviceType => _device.Type;
        public bool IsConnected => _device.IsConnected;
        public int LEDCount => _device.LEDCount;
        public int Width => _device.Width;
        public int Height => _device.Height;

        private LaunchpadDevice(RawMidiDevice device)
        {
            _device = device;

            _leds = Array.Empty<LED>();
            _oldLeds = Array.Empty<LED>();
            var modeMsg = SysEx.CreateBuffer(1, _device.Type, 0x21);
            modeMsg[7] = 1; //Standalone mode
            var layoutMsg = SysEx.CreateBuffer(1, _device.Type, 0x2C);
            layoutMsg[7] = 3; //Programmer layout
            var clearMsg = SysEx.CreateBuffer(1, _device.Type, 0x0E);
            clearMsg[7] = 0; //Clear all lights

            var connectMsgs = new[] { modeMsg, layoutMsg, clearMsg };
            var disconnectMsgs = new[] { clearMsg };

            _device.Connected += () =>
            {
                for (int i = 0; i < connectMsgs.Length; i++)
                    Send(connectMsgs[i], connectMsgs[i].Length - 1);
            };
            _device.Disconnecting += () =>
            {
                for (int i = 0; i < disconnectMsgs.Length; i++)
                    Send(disconnectMsgs[i], disconnectMsgs[i].Length - 1);
            };
            _device.ButtonDown += midiId => 
            {
                _device.GetPos(midiId, out var x, out var y);
                var btn = new Button(midiId, x, y);
                if (btn.IsSystem)
                    _queuedEvents.Enqueue(new LaunchpadEvent(EventType.SystemButtonDown, btn));
                else
                    _queuedEvents.Enqueue(new LaunchpadEvent(EventType.ButtonDown, btn));
            };
            _device.ButtonUp += midiId => 
            {
                _device.GetPos(midiId, out var x, out var y);
                var btn = new Button(midiId, x, y);
                if (btn.IsSystem)
                    _queuedEvents.Enqueue(new LaunchpadEvent(EventType.SystemButtonUp, btn));
                else
                    _queuedEvents.Enqueue(new LaunchpadEvent(EventType.ButtonUp, btn));
            };
        }
        public void Dispose()
        {
            Stop();
        }

        public void Start(int tps, int skip = 0)
        {
            Stop();
            _queuedEvents = new ConcurrentQueue<LaunchpadEvent>();
            _cancelToken = new CancellationTokenSource();
            _oldLeds = new LED[_device.LEDCount];
            _leds = new LED[_device.LEDCount];
            _normalMsg = SysEx.CreateBuffer(2 * _leds.Length, _device.Type, 0x0A);
            _pulseMsg = SysEx.CreateBuffer(3 * _leds.Length, _device.Type, 0x28);
            _flashMsg = SysEx.CreateBuffer(3 * _leds.Length, _device.Type, 0x23);
            _task = RunTask(tps, skip, _cancelToken.Token);
        }
        public void Stop()
        { 
            if (_task != null)
            {
                _cancelToken.Cancel();
                _task.GetAwaiter().GetResult();
                _task = null;
            }
            Disconnect();
        }

        private bool Connect()
        {
            if (_device.Connect())
            {
                _ledsInvalidated = true;
                return true;
            }
            return false;
        }
        private void Disconnect()
        {
            _device.Disconnect();
        }

        private Task RunTask(int tps, int skip, CancellationToken cancelToken)
        {
            return Task.Run(async () =>
            {
                IsRunning = true;
                var events = new List<LaunchpadEvent>();

                var tickLength = TimeSpan.FromSeconds((1.0 / tps * (skip + 1)));
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
                        if (!Connect())
                            continue;
                    }

                    _device.Update();

                    //Fetch all queued events for this tick
                    while (_queuedEvents.TryDequeue(out var evnt))
                        events.Add(evnt);

                    Tick?.Invoke(events);
                    Render();

                    events.Clear();
                }
                IsRunning = false;
            });
        }

        public void Clear()
        {
            for (int i = 0; i < _leds.Length; i++)
            {
                if (_leds[i].Mode != LEDMode.Off)
                    _ledsInvalidated = true;
                _leds[i].Mode = LEDMode.Off;
                _leds[i].Color = 0;
                _leds[i].FlashColor = 0;
            }
        }
        public void Set(LED[] leds)
        {
            if (leds.Length != 80)
                throw new InvalidOperationException("Array must be 80 elements");
            _leds = leds;
            _ledsInvalidated = true;
        }
        public void Set(int x, int y, int color)
        {
            int index = _device.GetIndex(x, y);
            if (index == byte.MaxValue)
                return;

            _leds[index].Mode = LEDMode.Normal;
            _leds[index].Color = (byte)color;
            _leds[index].FlashColor = 0;
            _ledsInvalidated = true;
        }
        public void SetOff(int x, int y)
        {
            int index = _device.GetIndex(x, y);
            if (index == byte.MaxValue)
                return;

            _leds[index].Mode = LEDMode.Off;
            _leds[index].Color = 0;
            _leds[index].FlashColor = 0;
            _ledsInvalidated = true;
        }
        public void SetPulse(int x, int y, int color)
        {
            int index = _device.GetIndex(x, y);
            if (index == byte.MaxValue || color < 0 || color > 128)
                return;

            _leds[index].Mode = LEDMode.Pulse;
            _leds[index].Color = (byte)color;
            _leds[index].FlashColor = 0;
            _ledsInvalidated = true;
        }
        public void SetFlash(int x, int y, int fromColor, int toColor)
        {
            int index = _device.GetIndex(x, y);
            if (index == byte.MaxValue)
                return;
                
            _leds[index].Mode = LEDMode.Flash;
            _leds[index].Color = (byte)fromColor;
            _leds[index].FlashColor = (byte)toColor;
            _ledsInvalidated = true;
        }
        private void Render()
        {
            if (!_ledsInvalidated)
                return;

            int normalPos = 7;
            int pulsePos = 7;
            int flashPos = 7;
            for (int i = 0; i < _leds.Length; i++)
            {
                byte id = _device.GetMidiId(i);
                var led = _leds[i];
                var oldLed = _oldLeds[i];
                if (led.Mode == oldLed.Mode && led.Color == oldLed.Color && led.FlashColor == oldLed.FlashColor)
                    continue;
                switch (led.Mode)
                {
                    case LEDMode.Off:
                        _normalMsg[normalPos++] = id;
                        _normalMsg[normalPos++] = 0;
                        break;
                    case LEDMode.Normal:
                        _normalMsg[normalPos++] = id;
                        _normalMsg[normalPos++] = led.Color;
                        break;
                    case LEDMode.Pulse:
                        _pulseMsg[pulsePos++] = id;
                        _pulseMsg[pulsePos++] = led.Color;
                        break;
                    case LEDMode.Flash:
                        _normalMsg[normalPos++] = id;
                        _normalMsg[normalPos++] = led.Color;
                        _flashMsg[flashPos++] = 0;
                        _flashMsg[flashPos++] = id;
                        _flashMsg[flashPos++] = led.FlashColor;
                        break;
                }
            }
            Send(_normalMsg, normalPos);
            Send(_pulseMsg, pulsePos);
            Send(_flashMsg, flashPos);

            Array.Copy(_leds, _oldLeds, _leds.Length);
            _ledsInvalidated = false;
        }

        private void Send(byte[] buffer, int count)
        {
            if (count != 7) //Blank msg
            {
                buffer[count++] = 0xF7;
                _device.Send(buffer, count);
            }
        }

        public byte GetLEDIndex(int x, int y) => _device.GetIndex(x, y);
    }
}