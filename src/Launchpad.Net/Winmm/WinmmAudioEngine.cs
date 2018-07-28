using System;
using System.Collections.Generic;

namespace Launchpad.Winmm
{
    internal class WinmmAudioEngine : IAudioEngine
    {
        public static readonly WinmmAudioEngine Instance = new WinmmAudioEngine();

        public IReadOnlyList<MidiDeviceInfo> ListLaunchpadDevices()
        {
            var devices = new List<MidiDeviceInfo>();
            int inDeviceCount = NativeMethods.midiInGetNumDevs();
            for (uint i = 0; i < inDeviceCount; i++)
            {
                var caps = new MIDIINCAPS();
                if (NativeMethods.midiInGetDevCaps(i, ref caps, MIDIINCAPS.Size) <= 0)
                    continue;

                if (caps.szPname.Contains(Midi.Mk2Name))
                    devices.Add(new MidiDeviceInfo(caps.szPname, caps.szPname, MidiDeviceType.Mk2));
                else if (caps.szPname.Contains(Midi.ProName))
                    devices.Add(new MidiDeviceInfo(caps.szPname, caps.szPname, MidiDeviceType.Pro));
            }
            return devices;
        }
        public IReadOnlyList<OutputDeviceInfo> ListOutputDevices() => Array.Empty<OutputDeviceInfo>();
        
        public RawMidiDevice GetMidiDevice(MidiDeviceInfo info) => new WinmmMidiDevice(info);
        public RawOutputDevice GetOutputDevice(OutputDeviceInfo info) => throw new NotImplementedException();

        void IDisposable.Dispose() { }
    }
}
