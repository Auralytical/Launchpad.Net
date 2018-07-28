using System;
using System.Runtime.InteropServices;

namespace Launchpad.Winmm
{
    internal class MidiBuffer : IDisposable
    {
        private readonly IntPtr _deviceHandle;
        private IntPtr _headerPtr, _dataPtr;
        private bool _isPrepared;
        private uint _size;

        public IntPtr Ptr => _headerPtr;

        public MidiBuffer(IntPtr device, uint size)
        {
            _deviceHandle = device;
            _size = size;

            //Allocate pointers
            _dataPtr = Marshal.AllocHGlobal((int)size);
            try
            {
                _headerPtr = Marshal.AllocHGlobal((int)MIDIHDR.Size);
            }
            catch
            {
                Marshal.FreeHGlobal(_dataPtr);
                throw;
            }
        }
        public void Dispose()
        {
            if (_isPrepared)
                Unprepare();
            if (_dataPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_dataPtr);
                _dataPtr = IntPtr.Zero;
            }
            if (_headerPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_headerPtr);
                _headerPtr = IntPtr.Zero;
            }
        }
        
        public bool Prepare(byte[] buffer, int count)
        {
            if (_isPrepared)
                Unprepare();

            Marshal.Copy(buffer, 0, _dataPtr, count);
            var header = new MIDIHDR
            {
                lpData = _dataPtr,
                dwBufferLength = (uint)count
            };
            Marshal.StructureToPtr(header, _headerPtr, false);

            _isPrepared = NativeMethods.midiOutPrepareHeader(_deviceHandle, _headerPtr, MIDIHDR.Size) == 0;
            return _isPrepared;
        }
        public void Unprepare()
        {
            if (!_isPrepared)
                return;
            NativeMethods.midiOutUnprepareHeader(_deviceHandle, _headerPtr, MIDIHDR.Size);
            _isPrepared = false;
        }
    }
}
