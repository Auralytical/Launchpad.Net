namespace Launchpad
{
    public struct MidiPosition
    {
        public byte Midi { get; }
        public byte ButtonX { get; }
        public byte ButtonY { get; }
        public byte LightX { get; }
        public byte LightY { get; }
        public SystemButton? SystemButton { get; }

        public MidiPosition(byte midi, byte buttonX, byte buttonY, byte lightX, byte lightY, SystemButton? systemButton)
        {
            Midi = midi;
            ButtonX = buttonX;
            ButtonY = buttonY;
            LightX = lightX;
            LightY = lightY;
            SystemButton = systemButton;
        }
    }
}
