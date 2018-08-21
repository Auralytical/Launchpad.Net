using System;
using System.Collections.Generic;
using System.Linq;

namespace Launchpad
{
    public class DeviceInfo
    {
        public const int MaxLightCount = 97;

        public static DeviceInfo[] SupportedDevices => new[]
        {
            LaunchpadS,
            LaunchpadMini,
            LaunchpadMk2,
            LaunchpadPro
        };

        public static DeviceInfo LaunchpadS { get; } = new DeviceInfo( // Untested
            DeviceType.LaunchpadS,
            "Launchpad S",
            "Launchpad S MIDI 1",
            new byte[,]
            {
                { 204, 205, 206, 207, 208, 209, 210, 211, 255 },
                { 00, 01, 02, 03, 04, 05, 06, 07, 08 },
                { 16, 17, 18, 19, 20, 21, 22, 23, 24 },
                { 32, 33, 34, 35, 36, 37, 38, 39, 40 },
                { 48, 49, 50, 51, 52, 53, 54, 55, 56 },
                { 64, 65, 66, 67, 68, 69, 70, 71, 72 },
                { 80, 81, 82, 83, 84, 85, 86, 87, 88 },
                { 96, 97, 98, 99, 100, 101, 102, 103, 104 },
                { 112, 113, 114, 115, 116, 117, 118, 119, 120 },
            }, 0, 0,
            new Dictionary<byte, SystemButton>
            {
                [08] = SystemButton.Track1,
                [24] = SystemButton.Track2,
                [40] = SystemButton.Track3,
                [56] = SystemButton.Track4,
                [72] = SystemButton.Track5,
                [88] = SystemButton.Track6,
                [104] = SystemButton.Track7,
                [120] = SystemButton.Track8,
                [204] = SystemButton.Up,
                [205] = SystemButton.Down,
                [206] = SystemButton.Left,
                [207] = SystemButton.Right,
                [208] = SystemButton.Mode1,
                [209] = SystemButton.Mode2,
                [210] = SystemButton.Mode3,
                [211] = SystemButton.Mode4
            },
            d => new LaunchpadSRenderer(d));

        public static DeviceInfo LaunchpadMini { get; } = new DeviceInfo(
            DeviceType.LaunchpadMini,
            "Launchpad Mini",
            "Launchpad Mini MIDI 1",
            new byte[,]
            {
                { 204, 205, 206, 207, 208, 209, 210, 211, 255 },
                { 00, 01, 02, 03, 04, 05, 06, 07, 08 },
                { 16, 17, 18, 19, 20, 21, 22, 23, 24 },
                { 32, 33, 34, 35, 36, 37, 38, 39, 40 },
                { 48, 49, 50, 51, 52, 53, 54, 55, 56 },
                { 64, 65, 66, 67, 68, 69, 70, 71, 72 },
                { 80, 81, 82, 83, 84, 85, 86, 87, 88 },
                { 96, 97, 98, 99, 100, 101, 102, 103, 104 },
                { 112, 113, 114, 115, 116, 117, 118, 119, 120 },
            }, 0, 0,
            new Dictionary<byte, SystemButton>
            {
                [08] = SystemButton.Track1,
                [24] = SystemButton.Track2,
                [40] = SystemButton.Track3,
                [56] = SystemButton.Track4,
                [72] = SystemButton.Track5,
                [88] = SystemButton.Track6,
                [104] = SystemButton.Track7,
                [120] = SystemButton.Track8,
                [204] = SystemButton.Up,
                [205] = SystemButton.Down,
                [206] = SystemButton.Left,
                [207] = SystemButton.Right,
                [208] = SystemButton.Mode1,
                [209] = SystemButton.Mode2,
                [210] = SystemButton.Mode3,
                [211] = SystemButton.Mode4
            },
            d => new LaunchpadSRenderer(d));

        public static DeviceInfo LaunchpadMk2 { get; } = new DeviceInfo(
            DeviceType.LaunchpadMk2,
            "Launchpad MK2",
            "Launchpad MK2 MIDI 2",
            new byte[,]
            {
                { 104, 105, 106, 107, 108, 109, 110, 111, 255 },
                { 81, 82, 83, 84, 85, 86, 87, 88, 89 },
                { 71, 72, 73, 74, 75, 76, 77, 78, 79 },
                { 61, 62, 63, 64, 65, 66, 67, 68, 69 },
                { 51, 52, 53, 54, 55, 56, 57, 58, 59 },
                { 41, 42, 43, 44, 45, 46, 47, 48, 49 },
                { 31, 32, 33, 34, 35, 36, 37, 38, 39 },
                { 21, 22, 23, 24, 25, 26, 27, 28, 29 },
                { 11, 12, 13, 14, 15, 16, 17, 18, 19 }
            }, 0, 0,
            new Dictionary<byte, SystemButton>
            {
                [19] = SystemButton.Track8,
                [29] = SystemButton.Track7,
                [39] = SystemButton.Track6,
                [49] = SystemButton.Track5,
                [59] = SystemButton.Track4,
                [69] = SystemButton.Track3,
                [79] = SystemButton.Track2,
                [89] = SystemButton.Track1,
                [104] = SystemButton.Up,
                [105] = SystemButton.Down,
                [106] = SystemButton.Left,
                [107] = SystemButton.Right,
                [108] = SystemButton.Mode1,
                [109] = SystemButton.Mode2,
                [110] = SystemButton.Mode3,
                [111] = SystemButton.Mode4
            },
            d => new LaunchpadRGBRenderer(d));

        public static DeviceInfo LaunchpadPro { get; } = new DeviceInfo(
            DeviceType.LaunchpadPro,
            "Launchpad Pro",
            "Launchpad Pro MIDI 2",
            new byte[,]
            {
                { 255, 91, 92, 93, 94, 95, 96, 97, 98, 255 },
                { 80, 81, 82, 83, 84, 85, 86, 87, 88, 89 },
                { 70, 71, 72, 73, 74, 75, 76, 77, 78, 79 },
                { 60, 61, 62, 63, 64, 65, 66, 67, 68, 69 },
                { 50, 51, 52, 53, 54, 55, 56, 57, 58, 59 },
                { 40, 41, 42, 43, 44, 45, 46, 47, 48, 49 },
                { 30, 31, 32, 33, 34, 35, 36, 37, 38, 39 },
                { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 },
                { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 },
                { 255, 01, 02, 03, 04, 05, 06, 07, 08, 255 },
                { 255, 255, 255, 255, 99, 255, 255, 255, 255, 255}
            }, 1, 2, 
            new Dictionary<byte, SystemButton>
            {
                [01] = SystemButton.RecordArm,
                [02] = SystemButton.TrackSelect,
                [03] = SystemButton.Mute,
                [04] = SystemButton.Solo,
                [05] = SystemButton.Volume,
                [06] = SystemButton.Pan,
                [07] = SystemButton.Sends,
                [08] = SystemButton.StopClip,
                [19] = SystemButton.Track8,
                [29] = SystemButton.Track7,
                [39] = SystemButton.Track6,
                [49] = SystemButton.Track5,
                [59] = SystemButton.Track4,
                [69] = SystemButton.Track3,
                [79] = SystemButton.Track2,
                [89] = SystemButton.Track1,
                [91] = SystemButton.Up,
                [92] = SystemButton.Down,
                [93] = SystemButton.Left,
                [94] = SystemButton.Right,
                [95] = SystemButton.Mode1,
                [96] = SystemButton.Mode2,
                [97] = SystemButton.Mode3,
                [98] = SystemButton.Mode4,
                [80] = SystemButton.Shift,
                [70] = SystemButton.Click,
                [60] = SystemButton.Undo,
                [50] = SystemButton.Delete,
                [40] = SystemButton.Quantise,
                [30] = SystemButton.Duplicate,
                [20] = SystemButton.Double,
                [10] = SystemButton.Record
            },
            d => new LaunchpadRGBRenderer(d));

        public DeviceType Type { get; }
        public int Width { get; }
        public int Height { get; }
        public int InnerOffsetX { get; }
        public int InnerOffsetY { get; }
        public int LightCount { get; }
        public string MidiName { get; }
        public string MidiSubName { get; }
        public byte[,] Layout { get; }
        public IReadOnlyDictionary<byte, SystemButton> SystemButtons { get; }
        public Func<MidiDevice, IRenderer> RendererFactory { get; }

        public DeviceInfo(DeviceType type, string name, string subName, byte[,] layout, int innerOffsetX, int innerOffsetY, 
            IReadOnlyDictionary<byte, SystemButton> systemButtons, Func<MidiDevice, IRenderer> rendererFactory)
        {
            Type = type;
            MidiName = name;
            MidiSubName = subName;
            Layout = layout;
            Width = layout.GetLength(1);
            Height = layout.GetLength(0);
            InnerOffsetX = innerOffsetX;
            InnerOffsetY = innerOffsetY;
            SystemButtons = systemButtons;
            RendererFactory = rendererFactory;

            LightCount = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (layout[y, x] != 255)
                        LightCount++;
                }
            }
        }

        public static DeviceInfo FromType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.LaunchpadMini: return LaunchpadMini;
                case DeviceType.LaunchpadMk2: return LaunchpadMk2;
                case DeviceType.LaunchpadPro: return LaunchpadPro;
                default: throw new ArgumentOutOfRangeException("Unknown device type");
            }
        }
    }
}