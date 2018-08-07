using System.Runtime.InteropServices;

namespace Launchpad.Engines.Alsa
{
    internal struct SndRawMIDIInfo
    {
        public int Device;
        public int SubDevice;
        public int Stream;
        public int Card;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Id;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string Name;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Subname;
        public int SubDevicesCount;
        public int SubDevicesAvail;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Reserved;
    }
}