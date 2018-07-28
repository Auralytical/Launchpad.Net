using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Launchpad.Emulator
{
    internal class EmulatorAudioEngine : IAudioEngine
    {
        public static readonly EmulatorAudioEngine Instance = new EmulatorAudioEngine();

        public IReadOnlyList<MidiDeviceInfo> ListLaunchpadDevices()
        {
            var devices = ImmutableList.CreateBuilder<MidiDeviceInfo>();
            devices.Add(new MidiDeviceInfo("Mk2", "Mk2", MidiDeviceType.Mk2));
            devices.Add(new MidiDeviceInfo("Pro", "Pro", MidiDeviceType.Pro));
            return devices.ToImmutable();
        }
        public IReadOnlyList<OutputDeviceInfo> ListOutputDevices() => Array.Empty<OutputDeviceInfo>();
        
        public RawMidiDevice GetMidiDevice(MidiDeviceInfo info) => new EmulatorMidiDevice(info);
        public RawOutputDevice GetOutputDevice(OutputDeviceInfo info) => throw new NotImplementedException();

        void IDisposable.Dispose() { }
    }
}