using System;
using System.Collections.Generic;

namespace Launchpad.Engines.Winmm
{
    public static class WinmmMidiDevices
    {
        public static IReadOnlyList<LaunchpadMidiDevice> GetLaunchpads(bool mapPowerLED = false)
        {
            var devices = new List<WinmmLaunchpadMidiDevice>();
            int inDeviceCount = NativeMethods.midiInGetNumDevs();
            for (uint i = 0; i < inDeviceCount; i++)
            {
                var caps = new MIDIINCAPS();
                if (NativeMethods.midiInGetDevCaps(i, ref caps, MIDIINCAPS.Size) <= 0)
                    continue;

                if (caps.szPname.Contains(Midi.Mk2Name))
                    devices.Add(new WinmmLaunchpadMidiDevice(caps.szPname, caps.szPname, DeviceType.Mk2));
                else if (caps.szPname.Contains(Midi.ProName))
                    devices.Add(new WinmmLaunchpadMidiDevice(caps.szPname, caps.szPname, mapPowerLED ? DeviceType.ProWithPower : DeviceType.Pro));
            }
            return devices;
        }
    }
}
