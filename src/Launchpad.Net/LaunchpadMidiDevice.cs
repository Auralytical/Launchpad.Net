using System;

namespace Launchpad
{
    public abstract class LaunchpadMidiDevice  : IDisposable
    {
        public const int MaxLEDCount = 97;
        
        private static readonly byte[,] ProWithPowerMidiLayout = new byte[,]
        {
            { 255, 91, 92, 93, 94, 95, 96, 97, 98, 255 },
            { 80, 81, 82, 83, 84, 85, 86, 87, 88, 89 },
            { 70, 71, 72, 73, 74, 75, 76, 77, 78, 79 },
            { 60, 61, 62, 63, 64, 65, 66, 67, 68, 69 },
            { 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 },
            { 40, 41, 42, 43, 44, 45, 46, 47, 48, 49 },
            { 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 },
            { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 },
            { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 },
            { 255, 01, 02, 03, 04, 05, 06, 07, 08, 255 },
            { 255, 255, 255, 255, 99, 255, 255, 255, 255, 255}
        };
        private static readonly byte[,] ProMidiLayout = new byte[,]
        {
            { 255, 91, 92, 93, 94, 95, 96, 97, 98, 255 },
            { 80, 81, 82, 83, 84, 85, 86, 87, 88, 89 },
            { 70, 71, 72, 73, 74, 75, 76, 77, 78, 79 },
            { 60, 61, 62, 63, 64, 65, 66, 67, 68, 69 },
            { 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 },
            { 40, 41, 42, 43, 44, 45, 46, 47, 48, 49 },
            { 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 },
            { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 },
            { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 },
            { 255, 01, 02, 03, 04, 05, 06, 07, 08, 255 }
        };
        private static readonly byte[,] Mk2MidiLayout = new byte[,]
        {
            { 255, 104, 105, 106, 107, 108, 109, 110, 111, 255 },
            { 255, 81, 82, 83, 84, 85, 86, 87, 88, 89 },
            { 255, 71, 72, 73, 74, 75, 76, 77, 78, 79 },
            { 255, 61, 62, 63, 64, 65, 66, 67, 68, 69 },
            { 255, 51, 52, 53, 54, 55, 56, 57, 58, 59 },
            { 255, 41, 42, 43, 44, 45, 46, 47, 48, 49 },
            { 255, 31, 32, 33, 34, 35, 36, 37, 38, 39 },
            { 255, 21, 22, 23, 24, 25, 26, 27, 28, 29 },
            { 255, 11, 12, 13, 14, 15, 16, 17, 18, 19 },
            { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }
        };

        public event Action Disconnecting, Disconnected;
        public event Action Connecting, Connected;
        public event Action<byte> ButtonDown, ButtonUp;

        private readonly byte[] _indexToMidi;
        private readonly byte[] _midiToIndex;
        private readonly byte[,] _posToMidi;
        private readonly byte[,] _posToIndex;

        public string Id { get; }
        public string Name { get; }
        public DeviceType Type { get; }
        public int LEDCount { get; }
        public int Width { get; }
        public int Height { get; }
        
        public bool IsConnected { get; private set; }

        protected LaunchpadMidiDevice(string id, string name, DeviceType type)
        {
            Id = id;
            Name = name;
            Type = type;

            byte[,] layout;
            switch (Type)
            {
                case DeviceType.Mk2:
                    LEDCount = 80;
                    Width = 9;
                    Height = 9;
                    layout = Mk2MidiLayout;
                    break;
                case DeviceType.Pro:
                default:
                    LEDCount = 97;
                    Width = 10;
                    Height = 10;
                    layout = ProMidiLayout;
                    break;
                case DeviceType.ProWithPower:
                    LEDCount = 97;
                    Width = 10;
                    Height = 11;
                    layout = ProWithPowerMidiLayout;
                    break;
            }
            
            _midiToIndex = new byte[256];
            _indexToMidi = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                _midiToIndex[i] = 255;
                _indexToMidi[i] = 255;
            }
            _posToMidi = new byte[Width, Height];
            _posToIndex = new byte[Width, Height];

            for (int y = 0, i = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    byte val = layout[Height - y - 1, x];
                    if (val != 255)
                    {
                        _indexToMidi[i] = val;
                        _posToMidi[x, y] = val;
                        _midiToIndex[val] = (byte)i;
                        _posToIndex[x, y] = (byte)i;
                        i++;
                    }
                    else
                    {
                        _indexToMidi[i] = 255;
                        _posToMidi[x, y] = 255;
                        _posToIndex[x, y] = 255;
                    }
                }
            }
        }
        public virtual void Dispose() => Disconnect();

        public bool Connect() => Connect(true);
        protected virtual bool Connect(bool isNormal)
        {
            if (IsConnected)
                Disconnect(isNormal);
            Connecting?.Invoke();
            if (ConnectInternal(isNormal))
            {
                IsConnected = true;
                Connected?.Invoke();
            }
            return true;
        }
        protected virtual bool ConnectInternal(bool isNormal) => true;

        public void Disconnect() => Disconnect(true);
        protected virtual void Disconnect(bool isNormal)
        {
            if (IsConnected)
                Disconnecting?.Invoke();
            DisconnectInternal(isNormal);
            if (IsConnected)
            {
                IsConnected = false;
                Disconnected?.Invoke();
            }
        }        
        protected virtual void DisconnectInternal(bool isNormal) { }

        public virtual bool Send(byte[] buffer, int count)
        {
            if (!IsConnected)
                return false;
            return SendInternal(buffer, count);
        }
        protected virtual bool SendInternal(byte[] buffer, int count) => false;

        internal byte GetIndex(byte midiId) => _midiToIndex[midiId];
        internal byte GetIndex(int midiId) => GetIndex((byte)midiId);
        internal byte GetIndex(byte x, byte y) => x < Width && y < Height ? _posToIndex[x, y] : byte.MaxValue;
        internal byte GetIndex(int x, int y) => GetIndex((byte)x, (byte)y);
        
        internal byte GetMidiId(byte index) => _midiToIndex[index];
        internal byte GetMidiId(int index) => index >= 0 && index < 256 ? _indexToMidi[index] : byte.MaxValue;
        internal byte GetMidiId(byte x, byte y) => x < Width && y < Height ?_posToMidi[x, y] : byte.MaxValue;
        internal byte GetMidiId(int x, int y) => GetMidiId((byte)x, (byte)y);

        internal void GetPos(byte midiId, out byte x, out byte y)
        {
            if (Type == DeviceType.Mk2 && midiId >= 104)
                midiId -= 13;
            y = (byte)(midiId / 10U);
            x = (byte)(midiId % 10U);
        }

        protected void RaiseButtonDown(byte midiId) => ButtonDown?.Invoke(midiId);
        protected void RaiseButtonUp(byte midiId) => ButtonUp?.Invoke(midiId);

        public virtual void Update() { }
    }
}