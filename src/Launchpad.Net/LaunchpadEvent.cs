namespace Launchpad
{
    public struct LaunchpadEvent
    {
        public EventType Type { get; }
        public Button Button { get;}

        public LaunchpadEvent(EventType type, Button button)
        {
            Type = type;
            Button = button;
        }
    }
}
