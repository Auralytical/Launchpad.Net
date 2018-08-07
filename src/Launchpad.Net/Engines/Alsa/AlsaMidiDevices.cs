using System;
using System.Collections.Generic;

namespace Launchpad.Engines.Alsa
{
    public static class AlsaMidiDevices
    {
        public static IReadOnlyList<LaunchpadMidiDevice> GetLaunchpads()
        {
            var devices = new List<AlsaLaunchpadMidiDevice>();
            int card = -1;
            while (NativeMethods.snd_card_next(ref card) >= 0 && card >= 0)
            {
                if (NativeMethods.snd_ctl_open(out var ctl, "hw:" + card, 0) < 0)
                    continue;
                try
                {
                    int device = -1;
                    while (NativeMethods.snd_ctl_rawmidi_next_device(ctl, ref device) >= 0 && device >= 0)
                    {
                        var info = new SndRawMIDIInfo { Device = device };

                        info.SubDevice = 0;
                        if (NativeMethods.snd_ctl_rawmidi_info(ctl, ref info) < 0)
                            continue;
                        for (int i = 0; i < info.SubDevicesCount; i++)
                        {
                            info.SubDevice = i;
                            info.Stream = 0;
                            if (NativeMethods.snd_ctl_rawmidi_info(ctl, ref info) < 0)
                                continue;
                            info.Stream = 1;
                            if (NativeMethods.snd_ctl_rawmidi_info(ctl, ref info) < 0)
                                continue;

                            string port = $"hw:{info.Card},{info.Device},{info.SubDevice}";
                            if (info.Name.Contains(Midi.Mk2Name) && info.Subname.Contains(Midi.Mk2SubName))
                                devices.Add(new AlsaLaunchpadMidiDevice(port, info.Name, DeviceType.Mk2));
                            else if (info.Name.Contains(Midi.ProName) && info.Subname.Contains(Midi.ProSubName))
                                devices.Add(new AlsaLaunchpadMidiDevice(port, info.Name, DeviceType.Pro));
                        }
                    }
                }
                finally { NativeMethods.snd_ctl_close(ctl); }
            }
            return devices;
        }
    }
}