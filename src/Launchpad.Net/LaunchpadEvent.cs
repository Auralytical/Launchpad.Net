namespace Launchpad
{
    public struct LaunchpadEvent
    {
        public EventType Type { get; }
        public byte Midi { get; }
        public byte ButtonX { get; }
        public byte ButtonY { get; }
        public byte LightX { get; }
        public byte LightY { get; }
        public SystemButton? SystemButton { get; }

        public LaunchpadEvent(EventType type, MidiPosition pos)
        {
            Type = type;
            Midi = pos.Midi;
            ButtonX = pos.ButtonX;
            ButtonY = pos.ButtonY;
            LightX = pos.LightX;
            LightY = pos.LightY;
            SystemButton = pos.SystemButton;
        }
    }
}
