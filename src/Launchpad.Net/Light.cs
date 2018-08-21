namespace Launchpad
{
    public struct Light
    {
        public LightMode Mode { get; }
        public byte Color { get; }
        public byte FlashColor { get; }

        public Light(LightMode mode, byte color = 0, byte flashColor = 0)
        {
            Mode = mode;
            Color = color;
            FlashColor = flashColor;
        }
    }
}
