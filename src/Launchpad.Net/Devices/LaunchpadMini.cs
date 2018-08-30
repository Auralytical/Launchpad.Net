using System;
using System.Collections.Generic;
using System.Linq;

namespace Launchpad
{
    public partial class DeviceInfo
    {
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
            new Dictionary<byte, Color>
            {
                // TODO: Impl
            },
            d => new LaunchpadSRenderer(d));
    }
}