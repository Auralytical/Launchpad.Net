using System;
using System.Threading;

namespace Launchpad.Engines.Winmm
{
    internal class WinmmMidiDevice : MidiDevice
    {
        private uint _inDeviceId, _outDeviceId;
        private IntPtr _inDeviceHandle, _outDeviceHandle;
        private NativeMethods.MidiInProc _inputCallback;
        private NativeMethods.MidiOutProc _outputCallback;
        private ManualResetEventSlim _inputClosed, _outputClosed;
        private MidiBuffer _outBuffer;

        public WinmmMidiDevice(string id, string name, DeviceType type)
            : base(id, name, type)
        {
            _inputClosed = new ManualResetEventSlim(false);
            _outputClosed = new ManualResetEventSlim(false);
        }

        protected override bool Connect(bool isNormal)
        {
            if (base.Connect())
            {
                _inputClosed.Reset();
                _outputClosed.Reset();
                return true;
            }
            return false;
        }
        protected override bool ConnectInternal(bool isNormal)
        {
            int inDeviceCount = NativeMethods.midiInGetNumDevs();
            uint? inDeviceId = null;
            for (uint i = 0; i < inDeviceCount; i++)
            {
                var caps = new MIDIINCAPS();
                NativeMethods.midiInGetDevCaps(i, ref caps, MIDIINCAPS.Size);
                if (caps.szPname == Id)
                {
                    inDeviceId = i;
                    break;
                }
            }
            if (inDeviceId == null)
                return false;

            int outDeviceCount = NativeMethods.midiOutGetNumDevs();
            uint? outDeviceId = null;
            for (uint i = 0; i < outDeviceCount; i++)
            {
                var caps = new MIDIOUTCAPS();
                NativeMethods.midiOutGetDevCaps(i, ref caps, MIDIOUTCAPS.Size);
                if (caps.szPname == Id)
                {
                    outDeviceId = i;
                    break;
                }
            }
            if (outDeviceId == null)
                return false;

            if (NativeMethods.midiInOpen(out var inDeviceHandle, inDeviceId.Value, _inputCallback, 0, 0x00030000) != 0)
                return false;
            if (NativeMethods.midiOutOpen(out var outDeviceHandle, outDeviceId.Value, _outputCallback, 0, 0x00030000) != 0)
                return false;
            if (NativeMethods.midiInStart(inDeviceHandle) != 0)
                return false;

            _outBuffer = new MidiBuffer(outDeviceHandle, SysEx.MaxMessageLength);

            _inDeviceId = inDeviceId.Value;
            _outDeviceId = outDeviceId.Value;
            _inDeviceHandle = inDeviceHandle;
            _outDeviceHandle = outDeviceHandle;
            _inputCallback = InputEvent;
            _outputCallback = OutputEvent;
            return true;
        }

        protected override void DisconnectInternal(bool isNormal)
        {
            if (_outBuffer != null)
            {
                _outBuffer.Dispose();
                _outBuffer = null;
            }
            if (_inDeviceHandle != IntPtr.Zero)
            {
                if (IsConnected)
                    NativeMethods.midiInStop(_inDeviceHandle);
                NativeMethods.midiInClose(_inDeviceHandle);
                _inDeviceHandle = IntPtr.Zero;
                _inputClosed.Wait(2500);
            }
            if (_outDeviceHandle != IntPtr.Zero)
            {
                NativeMethods.midiOutClose(_outDeviceHandle);
                _outDeviceHandle = IntPtr.Zero;
                _outputClosed.Wait(2500);
            }
            _inputCallback = null;
            _outputCallback = null;
            _inDeviceId = 0;
            _outDeviceId = 0;
        }

        private void InputEvent(IntPtr hMidiIn, uint wMsg, uint dwInstance, uint dwParam1, uint dwParam2)
        {
            switch (wMsg)
            {
                case 0x3C1: //MM_MIM_OPEN (Connected)
                    break;
                case 0x3C2: //MM_MIM_CLOSE (Disconnected)
                    _inputClosed.Set();
                    break;
                case 0x3C3: //MM_MIM_DATA
                    byte msgType = (byte)(dwParam1 >> 0); // TODO: Test
                    byte midiId = (byte)(dwParam1 >> 8);
                    byte velocity = (byte)(dwParam1 >> 16);
                    switch (msgType)
                    {
                        case (byte)MidiMessageType.NoteOn:
                        case (byte)MidiMessageType.ControlModeChange:
                            if (velocity != 0)
                                RaiseButtonDown((MidiMessageType)msgType, midiId);
                            else
                                RaiseButtonUp((MidiMessageType)msgType, midiId);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        private void OutputEvent(IntPtr hMidiIn, uint wMsg, uint dwInstance, uint dwParam1, uint dwParam2)
        {
            switch (wMsg)
            {
                case 0x3C7: //MM_MOM_OPEN (Connected)
                    break;
                case 0x3C8: //MM_MOM_CLOSE (Disconnected)
                    _outputClosed.Set();
                    break;
                default:
                    break;
            }
        }

        protected override bool SendInternal(byte[] buffer, int count)
        {
            if (!IsConnected)
                return false;
            if (!_outBuffer.Prepare(buffer, buffer.Length))
                return false;
            try
            {
                if (NativeMethods.midiOutLongMsg(_outDeviceHandle, _outBuffer.Ptr, MIDIHDR.Size) >= 0)
                    return true;
            }
            finally { _outBuffer.Unprepare(); }
            return false;
        }
    }
}