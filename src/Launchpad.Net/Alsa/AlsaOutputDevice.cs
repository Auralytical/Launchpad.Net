namespace Launchpad.Alsa
{
    internal class AlsaOutputDevice : RawOutputDevice
    {
        public AlsaOutputDevice(OutputDeviceInfo info)
            : base(info) { }

        protected override bool ConnectInternal(bool isNormal)
        {
            return true;
        }

        protected override void DisconnectInternal(bool isNormal)
        {
        }
    }
}