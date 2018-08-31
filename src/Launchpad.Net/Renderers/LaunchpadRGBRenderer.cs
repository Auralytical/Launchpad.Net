using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Launchpad
{    
    public class LaunchpadRGBRenderer : IRenderer
    {        
        private readonly MidiDevice _device;

        private readonly Light[] _lights;
        private readonly byte[] _indexToMidi, _midiToIndex;
        private readonly byte[] _normalMsg, _pulseMsg, _flashMsg, _clockMsg;
        private bool _lightsInvalidated;

        public LaunchpadRGBRenderer(MidiDevice device)
        {
            _device = device;
            
            // Cache id lookups
            var info = DeviceInfo.FromType(device.Type);
            _indexToMidi = new byte[256];
            _midiToIndex = new byte[256];
            for (int i = 0; i < _indexToMidi.Length; i++)
                _indexToMidi[i] = 255;
            for (int i = 0; i < _midiToIndex.Length; i++)
                _midiToIndex[i] = 255;
            for (byte y = 0; y < info.Height; y++)
            {
                for (byte x = 0; x < info.Width; x++)
                {
                    byte midi = info.MidiLayout[info.Height - y - 1, x];
                    byte index = info.IndexLayout[info.Height - y - 1, x];
                    if (midi != 255 && index != 255)
                    {
                        _indexToMidi[index] = midi;
                        _midiToIndex[midi] = index;
                    }
                }
            }

            // Register events
            var modeMsg = SysEx.CreateBuffer(_device.Type, 0x21, 1);
            modeMsg[7] = 0x01; // Standalone mode
            var layoutMsg = SysEx.CreateBuffer(_device.Type, 0x2C, 1);
            layoutMsg[7] = 0x03; // Programmer layout
            var clearMsg = SysEx.CreateBuffer(_device.Type, 0x0E, 1);
            clearMsg[7] = 0x00; // Clear all lights
            _device.Connected += () =>
            {
                SendSysEx(modeMsg);
                SendSysEx(layoutMsg);
                SendSysEx(clearMsg);
                _lightsInvalidated = true;
            };
            _device.Disconnecting += () =>
            {
                SendSysEx(clearMsg);
            };

            // Create buffers
            _lights = new Light[info.LightCount];
            _normalMsg = SysEx.CreateBuffer(_device.Type, 0x0A, 2 * _lights.Length);
            _pulseMsg = SysEx.CreateBuffer(_device.Type, 0x28, 3 * _lights.Length);
            _flashMsg = SysEx.CreateBuffer(_device.Type, 0x23, 3 * _lights.Length);
            _clockMsg = Midi.CreateBuffer(MidiMessageType.MidiClock, 0); // MIDI Clock
        }

        public void Clear()
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                if (_lights[i].Mode != LightMode.Off)
                    _lightsInvalidated = true;
                _lights[i] = new Light(LightMode.Off);
            }
        }

        public void Set(byte midiId, Light light)
        {
            switch (light.Mode)
            {
                case LightMode.Off: SetOff(midiId); break;
                case LightMode.Normal: Set(midiId, light.Color); break;
                case LightMode.Pulse: SetPulse(midiId, light.Color); break;
                case LightMode.Flash: SetFlash(midiId, light.Color, light.FlashColor); break;
            }
        }
        public void Set(byte midiId, byte color)
        {
            byte index = _midiToIndex[midiId];
            if (index == byte.MaxValue)
                return;
            if (_lights[index].Mode == LightMode.Normal &&
                _lights[index].Color == color)
                return;

            _lights[index] = new Light(LightMode.Normal, color);
            _lightsInvalidated = true;
        }
        public void SetOff(byte midiId)
        {
            byte index = _midiToIndex[midiId];
            if (index == byte.MaxValue)
                return;
            if (_lights[index].Mode == LightMode.Off)
                return;

            _lights[index] = new Light(LightMode.Off);
            _lightsInvalidated = true;
        }
        public void SetPulse(byte midiId, byte color)
        {
            byte index = _midiToIndex[midiId];
            if (index == byte.MaxValue || color < 0 || color > 128)
                return;
            if (_lights[index].Mode == LightMode.Pulse &&
                _lights[index].Color == color)
                return;

            _lights[index] = new Light(LightMode.Pulse, color);
            _lightsInvalidated = true;
        }        
        public void SetFlash(byte midiId, byte color1, byte color2)
        {
            byte index = _midiToIndex[midiId];
            if (index == byte.MaxValue)
                return;
            if (_lights[index].Mode == LightMode.Flash &&
                _lights[index].Color == color1 &&
                _lights[index].FlashColor == color2)
                return;
                
            _lights[index] = new Light(LightMode.Flash, color1, color2);
            _lightsInvalidated = true;
        }

        public void ClockTick() 
        { 
            SendMidi(_clockMsg);
        }

        public void Render()
        {
            if (!_lightsInvalidated)
                return;

            int normalPos = 7;
            int pulsePos = 7;
            int flashPos = 7;
            for (int i = 0; i < _lights.Length; i++)
            {
                byte midi = _indexToMidi[i];
                var light = _lights[i];
                switch (light.Mode)
                {
                    case LightMode.Off:
                        _normalMsg[normalPos++] = midi;
                        _normalMsg[normalPos++] = 0;
                        break;
                    case LightMode.Normal:
                        _normalMsg[normalPos++] = midi;
                        _normalMsg[normalPos++] = light.Color;
                        break;
                    case LightMode.Pulse:
                        _pulseMsg[pulsePos++] = midi;
                        _pulseMsg[pulsePos++] = light.Color;
                        break;
                    case LightMode.Flash:
                        _normalMsg[normalPos++] = midi;
                        _normalMsg[normalPos++] = light.Color;
                        _flashMsg[flashPos++] = midi;
                        _flashMsg[flashPos++] = light.FlashColor;
                        break;
                }
            }
            SendSysEx(_normalMsg, normalPos);
            SendSysEx(_pulseMsg, pulsePos);
            SendSysEx(_flashMsg, flashPos);
            _lightsInvalidated = false;
        }

        private void SendSysEx(byte[] buffer)
            => SendSysEx(buffer, buffer.Length - 1);
        private void SendSysEx(byte[] buffer, int count)
        {
            if (count != 7) //Blank msg
            {
                buffer[count++] = 0xF7;
                _device.Send(buffer, count);
            }
        }
        private void SendMidi(byte[] buffer)
            => _device.Send(buffer, buffer.Length);
    }
}