using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Launchpad
{    
    public class LaunchpadSRenderer : IRenderer
    {        
        private readonly MidiDevice _device;

        private readonly Light[] _lights;
        private readonly byte[] _indexToMidi, _midiToIndex;
        private readonly byte[] _normalMsg, _flashMsg, _bufferMsg;
        private bool _lightsInvalidated;
        private bool _flashState;

        public LaunchpadSRenderer(MidiDevice device)
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
            var layoutMsg = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            layoutMsg[1] = 0x00;
            layoutMsg[2] = 0x01; // X-Y layout
            var clearMsg = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            clearMsg[1] = 0x00;
            clearMsg[2] = 0x00; // Clear all lights
            _device.Connected += () =>
            {
                SendMidi(layoutMsg);
                SendMidi(clearMsg);
                _lightsInvalidated = true;
            };
            _device.Disconnecting += () =>
            {
                SendMidi(clearMsg);
            };

            // Create buffers
            _lights = new Light[info.LightCount];
            _normalMsg = Midi.CreateBuffer(MidiMessageType.NoteOn, 3, _lights.Length);
            _flashMsg = Midi.CreateBuffer(MidiMessageType.NoteOn, 3, _lights.Length);
            _bufferMsg = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            _bufferMsg[2] = 32; // Simple
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
            _flashState = !_flashState;
        }

        public void Render()
        {
            if (!_lightsInvalidated)
                return;

            var flashState = _flashState; // Cache because value is updated async
            for (int i = 0; i < _lights.Length; i++)
            {
                byte midi = _indexToMidi[i];
                var light = _lights[i];
                switch (light.Mode)
                {
                    case LightMode.Off:
                        _normalMsg[i + 1] = 0;
                        _flashMsg[i + 1] = 0;
                        break;
                    case LightMode.Normal:
                        _normalMsg[i + 1] = light.Color;
                        _flashMsg[i + 1] = light.Color;
                        break;
                    case LightMode.Pulse: // Not supported, treat as flash
                    case LightMode.Flash:
                        _normalMsg[i + 1] = light.Color;
                        _flashMsg[i + 1] = 0; // light.FlashColor
                        break;
                }
            }
            if (!flashState)
                SendMidi(_normalMsg);
            else
                SendMidi(_flashMsg);
            SendMidi(_bufferMsg); // Reset cursor position
        }

        private void SendMidi(byte[] buffer)
            => _device.Send(buffer, buffer.Length);
    }
}