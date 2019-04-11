namespace Launchpad
{
    public struct Light
    {
        public LightMode Mode { get; }
        public byte Color { get; }
        public byte FlashColor { get; }

        public readonly byte R, G, B;  // RGB only

        public Light(LightMode mode, byte color = 0, byte flashColor = 0)
        {
            Mode = mode;
            Color = color;
            FlashColor = flashColor;
            R = 0;
            G = 0;
            B = 0;
        }

        public Light(LightMode mode, byte red, byte green, byte blue = 0, byte flashColor = 0)
        {
            Mode = mode;
            Color = 0;
            FlashColor = flashColor;
            R = red;
            G = green;
            B = blue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Light)) return false;

            Light other = (Light)obj;
            if (Mode != other.Mode) return false;
            if (Color != other.Color) return false;
            if (FlashColor != other.FlashColor) return false;
            if (R != other.R) return false;
            if (G != other.G) return false;
            if (B != other.B) return false;
            return true;
        }
        public override int GetHashCode()
        {
            return ((byte)Mode << 3 * 8) | ( Color | FlashColor | ((R << 2 * 8) | (G << 1 * 8) | B));
        }
    }
}
