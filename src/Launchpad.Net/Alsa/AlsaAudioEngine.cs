using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Launchpad.Alsa
{
    internal class AlsaAudioEngine : IAudioEngine
    {
        public static readonly AlsaAudioEngine Instance = new AlsaAudioEngine();
        
        public IReadOnlyList<MidiDeviceInfo> ListLaunchpadDevices()
        {
            var devices = ImmutableList.CreateBuilder<MidiDeviceInfo>();
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
                                devices.Add(new MidiDeviceInfo(port, info.Name, MidiDeviceType.Mk2));
                            else if (info.Name.Contains(Midi.ProName) && info.Subname.Contains(Midi.ProSubName))
                                devices.Add(new MidiDeviceInfo(port, info.Name, MidiDeviceType.Pro));
                        }
                    }
                }
                finally { NativeMethods.snd_ctl_close(ctl); }
            }
            return devices.ToImmutable();
        }
        public IReadOnlyList<OutputDeviceInfo> ListOutputDevices() => Array.Empty<OutputDeviceInfo>();
        
        public RawMidiDevice GetMidiDevice(MidiDeviceInfo info) => new AlsaMidiDevice(info);
        public RawOutputDevice GetOutputDevice(OutputDeviceInfo info) => new AlsaOutputDevice(info);

        void IDisposable.Dispose() { }
    }
}