using System;
using System.Collections.Generic;

namespace Launchpad.Engines.Winmm
{
    public static class WinmmMidiDevices
    {
        public static IReadOnlyList<MidiDevice> GetLaunchpads()
        {
            var devices = new List<WinmmMidiDevice>();
            int inDeviceCount = NativeMethods.midiInGetNumDevs();
            for (uint i = 0; i < inDeviceCount; i++)
            {
                var caps = new MIDIINCAPS();
                if (NativeMethods.midiInGetDevCaps(i, ref caps, MIDIINCAPS.Size) != 0)
                    continue;

                foreach (var deviceType in DeviceInfo.SupportedDevices)
                {
                    if (caps.szPname.Contains(deviceType.MidiName) && caps.szPname.Contains(deviceType.MidiSubName.Replace("MIDI ", "MIDIIN")))
                        devices.Add(new WinmmMidiDevice(caps.szPname, caps.szPname, deviceType.Type));
                }
            }
            return devices;
        }
    }
}
