using System;
using System.Collections.Generic;
using System.Linq;

namespace Launchpad
{
    public abstract class MidiDevice  : IDisposable
    {
        public event Action Disconnecting, Disconnected;
        public event Action Connecting, Connected;
        public event Action<MidiMessageType, byte> ButtonDown, ButtonUp;

        public string Id { get; }
        public string Name { get; }
        public DeviceType Type { get; }
        
        public bool IsConnected { get; private set; }

        protected MidiDevice(string id, string name, DeviceType type)
        {
            Id = id;
            Name = name;
            Type = type;
        }
        public virtual void Dispose() => Disconnect();

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

        public virtual bool Send(byte[] buffer, int count)
        {
            if (!IsConnected)
                return false;
            return SendInternal(buffer, count);
        }
        protected virtual bool SendInternal(byte[] buffer, int count) => false;

        protected void RaiseButtonDown(MidiMessageType type, byte midiId) => ButtonDown?.Invoke(type, midiId);
        protected void RaiseButtonUp(MidiMessageType type, byte midiId) => ButtonUp?.Invoke(type, midiId);

        public virtual void Update() { }
    }
}