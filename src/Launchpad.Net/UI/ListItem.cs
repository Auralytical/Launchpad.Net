namespace Launchpad.UI
{
    public struct ListItem<T>
    {
        public T Value { get; }
        public Light Light { get; }

        public ListItem(T value, Light light)
        {
            Value = value;
            Light = light;
        }
    }
}