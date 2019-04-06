using System;

namespace Launchpad
{
    public static class SysEx
    {
        public const int MaxMessageLength = 7 + (3 * DeviceInfo.MaxLightCount) + 1; //MK2 = 80, Pro = 97

        public static byte[] CreateBuffer(DeviceType type, byte[] msg, int msglen = -1)
        {
            int blocklen = msglen < msg.Length ? msg.Length : msglen;

            switch (type)
            {
                case DeviceType.LaunchpadMk2:
                    {
                        var data = new byte[6 + blocklen + 1];
                        data[0] = 0xF0;
                        data[1] = 0x00;
                        data[2] = 0x20;
                        data[3] = 0x29;
                        data[4] = 0x02;
                        data[5] = 0x18;
                        Buffer.BlockCopy(msg, 0, data, 6, msg.Length);
                        data[data.Length - 1] = 0xf7;
                        return data;
                    }
                case DeviceType.LaunchpadPro:
                    {
                        var data = new byte[6 + blocklen + 1];
                        data[0] = 0xF0;
                        data[1] = 0x00;
                        data[2] = 0x20;
                        data[3] = 0x29;
                        data[4] = 0x02;
                        data[5] = 0x10;
                        Buffer.BlockCopy(msg, 0, data, 6, msg.Length);
                        data[data.Length - 1] = 0xf7;
                        return data;
                    }
                default:
                    throw new InvalidOperationException("This device does not support SysEx");
            }
        }
        

        public static int GetHeaderLength(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.LaunchpadMk2: return 7;
                case DeviceType.LaunchpadPro: return 7;
                case DeviceType.LaunchpadMini: return 2;
            }
            return 0;
        }
    }
}