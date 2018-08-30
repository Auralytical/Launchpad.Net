namespace Launchpad
{
    public struct Color
    {
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
        public Color(int r, int g, int b)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
        }
    }
}
