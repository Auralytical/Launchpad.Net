using System;
using System.Collections.Generic;

namespace Launchpad
{
    public interface IAudioEngine  : IDisposable
    {
        IReadOnlyList<MidiDeviceInfo> ListLaunchpadDevices();
        IReadOnlyList<OutputDeviceInfo> ListOutputDevices();
        
        RawMidiDevice GetMidiDevice(MidiDeviceInfo info);
        RawOutputDevice GetOutputDevice(OutputDeviceInfo info);
    }
}