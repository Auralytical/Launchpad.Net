using System;
using System.Collections.Generic;
using System.Linq;

namespace Launchpad
{
    public partial class DeviceInfo
    {
        public static DeviceInfo LaunchpadPro { get; } = new DeviceInfo(
            DeviceType.LaunchpadPro,
            "Launchpad Pro",
            "MIDI 2",
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
            }, 
            new byte[,] // Indices don't really matter, base on S/Mini devices
            {
                { 255, 72, 73, 74, 75, 76, 77, 78, 79, 255 },
                { 80, 00, 01, 02, 03, 04, 05, 06, 07, 64 },
                { 81, 08, 09, 10, 11, 12, 13, 14, 15, 65 },
                { 82, 16, 17, 18, 19, 20, 21, 22, 23, 66 },
                { 83, 24, 25, 26, 27, 28, 29, 30, 31, 67 },
                { 84, 32, 33, 34, 35, 36, 37, 38, 39, 68 },
                { 85, 40, 41, 42, 43, 44, 45, 46, 47, 69 },
                { 86, 48, 49, 50, 51, 52, 53, 54, 55, 70 },
                { 87, 56, 57, 58, 59, 60, 61, 62, 63, 71 },
                { 255, 01, 02, 03, 04, 05, 06, 07, 08, 255 },
                { 255, 255, 255, 255, 88, 255, 255, 255, 255, 255}
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
            new Dictionary<byte, Color>
            {
                // TODO: Impl
            },
            d => new LaunchpadRGBRenderer(d));
    }
}