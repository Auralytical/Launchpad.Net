namespace Launchpad
{
    public struct OutputDeviceInfo
    {
        public readonly string Id, Name;

        public OutputDeviceInfo(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}