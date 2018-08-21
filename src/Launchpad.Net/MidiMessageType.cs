using System;

namespace Launchpad
{
    public enum MidiMessageType
    {
        NoteOff = 0x80,
        NoteOn = 0x90,
        PolyphonicAftertouch = 0xA0,
        ControlModeChange = 0xB0,
        ProgramChange = 0xC0,
        ChannelAftertouch = 0xD0,
        PitchWheelRange = 0xE0,
        SystemExclusive = 0xF0
    }
}