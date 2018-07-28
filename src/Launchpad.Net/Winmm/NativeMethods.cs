using System;
using System.Runtime.InteropServices;

namespace Launchpad.Winmm
{
    internal static class NativeMethods
    {
        public delegate void MidiInProc(IntPtr hMidiIn, uint wMsg, uint dwInstance, uint dwParam1, uint dwParam2);
        public delegate void MidiOutProc(IntPtr hmo, uint wMsg, uint dwInstance, uint dwParam1, uint dwParam2);

        [DllImport("winmm")]
        public static extern int midiInGetNumDevs();
        [DllImport("winmm")]
        public static extern int midiInGetDevCaps(uint uDeviceID, ref MIDIINCAPS lpMidiInCaps, uint cbMidiInCaps);
        [DllImport("winmm")]
        public static extern int midiInOpen(out IntPtr lphMidiIn, uint uDeviceID, MidiInProc dwCallback, uint dwInstance, uint dwFlags);
        [DllImport("winmm")]
        public static extern int midiInClose(IntPtr hMidiIn);
        [DllImport("winmm")]
        public static extern int midiInStart(IntPtr hMidiIn);
        [DllImport("winmm")]
        public static extern int midiInStop(IntPtr hMidiIn);

        [DllImport("winmm")]
        public static extern int midiOutGetNumDevs();
        [DllImport("winmm")]
        public static extern int midiOutGetDevCaps(uint uDeviceID, ref MIDIOUTCAPS lpMidiOutCaps, uint cbMidiOutCaps);
        [DllImport("winmm")]
        public static extern int midiOutOpen(out IntPtr lphMidiOut, uint uDeviceID, MidiOutProc dwCallback, uint dwInstance, uint dwFlags);
        [DllImport("winmm")]
        public static extern int midiOutClose(IntPtr hMidiOut);
        [DllImport("winmm")]
        public static extern int midiOutPrepareHeader(IntPtr hmo, IntPtr lpMidiInHdr, uint cbMidiOutHdr);
        [DllImport("winmm")]
        public static extern int midiOutUnprepareHeader(IntPtr hmo, IntPtr lpMidiInHdr, uint cbMidiOutHdr);
        [DllImport("winmm")]
        public static extern int midiOutLongMsg(IntPtr hmo, IntPtr lpMidiOutHdr, uint uSize);
    }

}
