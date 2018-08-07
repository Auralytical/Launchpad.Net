using System;
using System.Collections.Generic;

namespace Launchpad.Engines.Emulator
{
    public class EmulatorMidiDevices
    {
        public static IReadOnlyList<LaunchpadMidiDevice> GetLaunchpads()
        {
            var devices = new List<EmulatorLaunchpadMidiDevice>();
            devices.Add(new EmulatorLaunchpadMidiDevice("Mk2", "Mk2", DeviceType.Mk2));
            devices.Add(new EmulatorLaunchpadMidiDevice("Pro", "Pro", DeviceType.Pro));
            return devices;
        }
    }
}