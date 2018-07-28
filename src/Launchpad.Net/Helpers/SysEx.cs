namespace Launchpad
{
    public static class SysEx
    {
        public const int MaxMessageLength = 7 + (3 * LaunchpadDevice.MaxLEDCount) + 1; //MK2 = 80, Pro = 97

        public static byte[] CreateBuffer(int length, MidiDeviceType type, byte mode)
        {
            var data = new byte[7 + length + 1];
            data[0] = 0xF0;
            data[1] = 0x00;
            data[2] = 0x20;
            data[3] = 0x29;
            data[4] = 0x02;
            data[5] = (byte)(type == MidiDeviceType.Mk2 ? 0x18 : 0x10);
            data[6] = mode;
            return data;
        }

        public static bool IsValid(byte[] buffer, int count, MidiDeviceType type)
        {
            return count >= 8 && 
                buffer[0] == 0xF0 &&
                buffer[1] == 0x00 &&
                buffer[2] == 0x20 &&
                buffer[3] == 0x29 &&
                buffer[4] == 0x02 &&
                ((type == MidiDeviceType.Mk2 && buffer[5] == 0x18) ||
                (type == MidiDeviceType.Pro && buffer[5] == 0x10)) &&
                buffer[count - 1] == 0xF7;
        }
    }
}