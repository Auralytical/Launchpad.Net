using System;

namespace Launchpad
{
    public abstract class RawOutputDevice  : IDisposable
    {
        public event Action Disconnecting, Disconnected;
        public event Action Connecting, Connected;

        public string Id { get; }
        public string Name { get; }
        
        public bool IsConnected { get; private set; }

        protected RawOutputDevice(OutputDeviceInfo info)
        {
            Id = info.Id;
            Name = info.Name;
        }
        public virtual void Dispose() => Disconnect(false);

        public bool Connect() => Connect(true);
        protected virtual bool Connect(bool isNormal)
        {
            if (IsConnected)
                Disconnect(isNormal);
            Connecting?.Invoke();
            if (ConnectInternal(isNormal))
            {
                IsConnected = true;
                Connected?.Invoke();
            }
            return true;
        }
        protected virtual bool ConnectInternal(bool isNormal) => true;

        public void Disconnect() => Disconnect(true);
        protected virtual void Disconnect(bool isNormal)
        {
            if (IsConnected)
                Disconnecting?.Invoke();
            DisconnectInternal(isNormal);
            if (IsConnected)
            {
                IsConnected = false;
                Disconnected?.Invoke();
            }
        }
        protected virtual void DisconnectInternal(bool isNormal) { }

        public virtual void Play(byte[] buffer, int count)
        {
            if (!IsConnected)
                return;
            PlayInternal(buffer, count);
        }
        protected virtual void PlayInternal(byte[] buffer, int count) { }
    }
}