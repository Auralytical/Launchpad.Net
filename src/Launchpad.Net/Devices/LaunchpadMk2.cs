using System;
using System.Collections.Generic;
using System.Linq;

namespace Launchpad
{
    public partial class DeviceInfo
    {
        public static DeviceInfo LaunchpadMk2 { get; } = new DeviceInfo(
            DeviceType.LaunchpadMk2,
            "Launchpad MK2",
            "MIDI 2",
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
            }, 
            new byte[,] // Indices don't really matter, base on S/Mini devices
            {
                { 72, 73, 74, 75, 76, 77, 78, 79, 255 },
                { 00, 01, 02, 03, 04, 05, 06, 07, 64 },
                { 08, 09, 10, 11, 12, 13, 14, 15, 65 },
                { 16, 17, 18, 19, 20, 21, 22, 23, 66 },
                { 24, 25, 26, 27, 28, 29, 30, 31, 67 },
                { 32, 33, 34, 35, 36, 37, 38, 39, 68 },
                { 40, 41, 42, 43, 44, 45, 46, 47, 69 },
                { 48, 49, 50, 51, 52, 53, 54, 55, 70 },
                { 56, 57, 58, 59, 60, 61, 62, 63, 71 },
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
            new Dictionary<byte, Color>
            {
                // TODO: Impl
            },
            d => new LaunchpadRGBRenderer(d));
    }
}