using System;

namespace Launchpad.Engines.Alsa
{
    internal class AlsaMidiDevice : MidiDevice
    {
        private IntPtr _inDeviceHandle, _outDeviceHandle;
        private byte[] _readBuffer;

        internal AlsaMidiDevice(string id, string name, DeviceType type)
            : base(id, name, type) { }

        protected override bool ConnectInternal(bool isNormal)
        {
            IntPtr input, output;
            if (NativeMethods.snd_rawmidi_open(out input, out output, Id, 0x04) < 0) //1 = APPEND, 2 = NONBLOCK, 4 = SYNC
                return false;
            if (NativeMethods.snd_rawmidi_nonblock(input, 1) < 0)
                return false;
            NativeMethods.snd_rawmidi_read(input, null, 0);

            _inDeviceHandle = input;
            _outDeviceHandle = output;
            _readBuffer = new byte[1024];
            return true;
        }

        protected override void DisconnectInternal(bool isNormal)
        {
            // If the device was disconnected, snd_ctl_close will throw an uncatchable exception
            if (!isNormal)
                return;
                
            if (_inDeviceHandle != IntPtr.Zero)
            {
                NativeMethods.snd_ctl_close(_inDeviceHandle);
                _inDeviceHandle = IntPtr.Zero;
            }
            if (_outDeviceHandle != IntPtr.Zero)
            {
                NativeMethods.snd_ctl_close(_outDeviceHandle);
                _outDeviceHandle = IntPtr.Zero;
            }
        }

        protected override bool SendInternal(byte[] buffer, int count)
        {
            int result = NativeMethods.snd_rawmidi_write(_outDeviceHandle, buffer, count);
            return result >= 0;
        }

        public override void Update()
        {
            int bytes = NativeMethods.snd_rawmidi_read(_inDeviceHandle, _readBuffer, _readBuffer.Length);
            if (bytes == -19) //ENODEV
            {
                if (!Connect(false))
                    return;
                bytes = NativeMethods.snd_rawmidi_read(_inDeviceHandle, _readBuffer, _readBuffer.Length);
            }

            int pos = 0;
            while (pos < bytes)
            {
                byte msgType = _readBuffer[pos++];
                switch (msgType)
                {
                    case (byte)MidiMessageType.NoteOn:
                    case (byte)MidiMessageType.ControlModeChange:
                        byte midiId = _readBuffer[pos++];
                        byte velocity = _readBuffer[pos++];
                        if (velocity != 0)
                            RaiseButtonDown((MidiMessageType)msgType, midiId);
                        else
                            RaiseButtonUp((MidiMessageType)msgType, midiId);
                        break;
                }
            }
        }
    }
}