using System.Runtime.InteropServices;

namespace Launchpad.Engines.Winmm
{
    //https://msdn.microsoft.com/en-us/library/vs/alm/dd798467(v=vs.85).aspx
    [StructLayout(LayoutKind.Sequential)]
    internal struct MIDIOUTCAPS
    {
        public static readonly uint Size = (uint)Marshal.SizeOf<MIDIOUTCAPS>();

        public ushort wMid;
        public ushort wPid;
        public ushort vDriverVersionMajor;
        public ushort vDriverVersionMinor;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public ushort wTechnology;
        public ushort wVoices;
        public ushort wNotes;
        public ushort wChannelMask;
        public uint dwSupport;
    }
}
