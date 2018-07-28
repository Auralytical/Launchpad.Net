using System;

namespace Launchpad
{
    public abstract class RawMidiDevice  : IDisposable
    {
        public event Action Disconnecting, Disconnected;
        public event Action Connecting, Connected;
        public event Action<byte> ButtonDown, ButtonUp;

        private readonly byte[] _indexToMidi;
        private readonly byte[] _midiToIndex;
        private readonly byte[,] _posToMidi;
        private readonly byte[,] _posToIndex;

        public string Id { get; }
        public string Name { get; }
        public MidiDeviceType Type { get; }
        public int LEDCount { get; }
        public int Width { get; }
        public int Height { get; }
        
        public bool IsConnected { get; private set; }

        protected RawMidiDevice(MidiDeviceInfo info)
        {
            Id = info.Id;
            Name = info.Name;
            Type = info.Type;

            if (Type == MidiDeviceType.Mk2) 
            {
                LEDCount = 80;
                Width = 9;
                Height = 9;
            }
            else //Pro
            {
                LEDCount = 96;
                Width = 10;
                Height = 10;
            }
            
            // Cache lookups
            var layout = Type == MidiDeviceType.Mk2 ? MidiDeviceInfo.Mk2MidiLayout : MidiDeviceInfo.ProMidiLayout;
            
            _midiToIndex = new byte[256];
            _indexToMidi = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                _midiToIndex[i] = 255;
                _indexToMidi[i] = 255;
            }
            _posToMidi = new byte[10,10];
            _posToIndex = new byte[10,10];

            for (int y = 0, i = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    byte val = layout[9 - y, x];
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
        internal byte GetIndex(byte x, byte y) => x < 10 && y < 10 ? _posToIndex[x, y] : byte.MaxValue;
        internal byte GetIndex(int x, int y) => GetIndex((byte)x, (byte)y);
        
        internal byte GetMidiId(byte index) => _midiToIndex[index];
        internal byte GetMidiId(int index) => index >= 0 && index < 256 ? _indexToMidi[index] : byte.MaxValue;
        internal byte GetMidiId(byte x, byte y) => x < 10 && y < 10 ?_posToMidi[x, y] : byte.MaxValue;
        internal byte GetMidiId(int x, int y) => GetMidiId((byte)x, (byte)y);

        internal void GetPos(byte midiId, out byte x, out byte y)
        {
            if (Type == MidiDeviceType.Mk2 && midiId >= 104)
                midiId -= 13;
            y = (byte)(midiId / 10U);
            x = (byte)(midiId % 10U);
        }

        protected void RaiseButtonDown(byte midiId) => ButtonDown?.Invoke(midiId);
        protected void RaiseButtonUp(byte midiId) => ButtonUp?.Invoke(midiId);

        public virtual void Update() { }
    }
}