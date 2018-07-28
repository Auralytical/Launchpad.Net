namespace Launchpad
{
    public struct Button
    {
        public byte MidiId { get; }
        public byte X { get; }
        public byte Y { get; }

        public bool IsSystem => IsTopRow || IsLeftCol || IsRightCol || IsBottomRow;
        public bool IsTopRow => Y == 9;
        public bool IsBottomRow => Y == 0;
        public bool IsLeftCol => X == 0;
        public bool IsRightCol => X == 9;

        public Button(byte id, byte x, byte y)
        {
            MidiId = id;
            X = x;
            Y = y;
        }
    }
}