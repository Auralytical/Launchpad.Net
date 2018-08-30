using System;
using System.Collections.Generic;
using System.Linq;

namespace Launchpad
{
    public partial class DeviceInfo
    {
        public const int MaxLightCount = 97;

        public static DeviceInfo[] SupportedDevices => new[]
        {
            LaunchpadS,
            LaunchpadMini,
            LaunchpadMk2,
            LaunchpadPro
        };

        public static DeviceInfo FromType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.LaunchpadMini: return LaunchpadMini;
                case DeviceType.LaunchpadMk2: return LaunchpadMk2;
                case DeviceType.LaunchpadPro: return LaunchpadPro;
                case DeviceType.LaunchpadS: return LaunchpadS;
                default: throw new ArgumentOutOfRangeException("Unknown device type");
            }
        }

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
        public IReadOnlyDictionary<byte, Color> Colors { get; }
        public Func<MidiDevice, IRenderer> RendererFactory { get; }

        public DeviceInfo(DeviceType type, string name, string subName, byte[,] layout, int innerOffsetX, int innerOffsetY, 
            IReadOnlyDictionary<byte, SystemButton> systemButtons, IReadOnlyDictionary<byte, Color> colors,
            Func<MidiDevice, IRenderer> rendererFactory)
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
            Colors = colors;
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
    }
}