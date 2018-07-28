using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Launchpad
{
    public class OutputDevice : IDisposable
    {
        public static IAudioEngine _engine;
        public static IReadOnlyList<OutputDeviceInfo> AvailableDevices { get; private set; }

        static OutputDevice()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                SetEngine(EngineType.Winmm);
            else //if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                SetEngine(EngineType.Alsa);
        }
        public static void SetEngine(EngineType type)
        {
            switch (type)
            {
                case EngineType.Winmm: _engine = Winmm.WinmmAudioEngine.Instance; break;
                case EngineType.Alsa: _engine = Alsa.AlsaAudioEngine.Instance; break;
                case EngineType.Emulator: _engine = Emulator.EmulatorAudioEngine.Instance; break;
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
            RefreshDevices();
        }

        public static void RefreshDevices() => AvailableDevices = _engine.ListOutputDevices();
        public static OutputDevice For(OutputDeviceInfo device) => new OutputDevice(_engine.GetOutputDevice(device));

        private readonly RawOutputDevice _device;

        public string DeviceId => _device.Id;
        public bool IsConnected => _device.IsConnected;

        private OutputDevice(RawOutputDevice device)
        {
            _device = device;
        }
        public void Dispose()
        {
            Stop();
        }

        public void Start(int tps, int skip = 0)
        {
            Stop();
            Connect();
        }
        public void Stop()
        { 
            Disconnect();
        }

        private bool Connect() => _device.Connect();
        private void Disconnect() => _device.Disconnect();

        public void Play(byte[] buffer) => Play(buffer, buffer.Length);
        public void Play(byte[] buffer, int count)
        {
            if (!IsConnected)
                return;
            _device.Play(buffer, count);
        }
    }
}