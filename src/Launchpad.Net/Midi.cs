using System;

namespace Launchpad
{
    public static class Midi
    {
        public const int MaxMessageLength = 7 + (3 * DeviceInfo.MaxLightCount) + 1; //MK2 = 80, Pro = 97

        public static byte[] CreateBuffer(MidiMessageType type, byte channel)
        {
            int length;
            switch (type)
            {
                case MidiMessageType.NoteOff: length = 3; break;
                case MidiMessageType.NoteOn: length = 3; break;
                case MidiMessageType.PolyphonicAftertouch: length = 3; break;
                case MidiMessageType.ControlModeChange: length = 3; break;
                case MidiMessageType.ProgramChange: length = 2; break;
                case MidiMessageType.ChannelAftertouch: length = 2; break;
                case MidiMessageType.PitchWheelRange: length = 3; break;
                default: throw new InvalidOperationException("Unknown MIDI message type");
            }
            byte[] data = new byte[length];
            data[0] = (byte)((byte)type | ((channel - 1) & 0x0F));
            return data;
        }
    }
}