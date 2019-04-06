using System;
using System.Collections.Generic;

namespace Launchpad.Engines.Winmm
{
    public static class WinmmMidiDevices
    {
        private static MidiDevice _bestChoice = null;
        public static MidiDevice bestChoice
        {
            get
            {
                if (_bestChoice == null) { GetLaunchpads(); }
                return _bestChoice;
            }
        }

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
                    if (caps.szPname.Contains(deviceType.MidiName))
                    {
                        var device = new WinmmMidiDevice(caps.szPname, caps.szPname, deviceType.Type);
                        devices.Add(device);
                        if (caps.szPname.Contains(deviceType.MidiSubName.Replace("MIDI ", "MIDIIN")))
                        {
                            _bestChoice = device;
                        }
                    }
                }
            }
            return devices;
        }
    }
}
