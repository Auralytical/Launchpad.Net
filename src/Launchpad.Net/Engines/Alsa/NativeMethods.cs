using System;
using System.Runtime.InteropServices;

namespace Launchpad.Engines.Alsa
{
    internal static class NativeMethods
    {
        [DllImport("asound")]
        public static extern int snd_card_next(ref int card);
        [DllImport("asound")]
        public static extern int snd_ctl_open(out IntPtr ctl, string name, int mode);
        [DllImport("asound")]
        public static extern int snd_ctl_close(IntPtr ctl);
        [DllImport("asound")]
        public static extern int snd_ctl_rawmidi_next_device(IntPtr ctl, ref int device);
        [DllImport("asound")]
        public static extern int snd_ctl_rawmidi_info(IntPtr ctl, ref SndRawMIDIInfo midiInfo);

        [DllImport("asound")]
        public static extern int snd_rawmidi_open(out IntPtr input, out IntPtr output, string name, int mode);
        [DllImport("asound")]
        public static extern int snd_rawmidi_nonblock(IntPtr output, int mode);
        [DllImport("asound")]
        public static extern int snd_rawmidi_close(IntPtr rmidi);

        [DllImport("asound")]
        public static extern int snd_rawmidi_read(IntPtr rmidi, byte[] buffer, int size);
        [DllImport("asound")]
        public static extern int snd_rawmidi_write(IntPtr rmidi, byte[] buffer, int size);
        [DllImport("asound")]
        public static extern int snd_rawmidi_drain(IntPtr rmidi);
    }
}
