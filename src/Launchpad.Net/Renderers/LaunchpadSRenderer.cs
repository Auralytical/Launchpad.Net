using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Launchpad
{    
    // TODO: Add running status and/or buffering (see: Double buffering in reference)
    public class LaunchpadSRenderer : IRenderer
    {        
        private readonly MidiDevice _device;

        private readonly Light[] _lights;
        private readonly byte[] _noteOn, _noteOff, _topNoteOn, _topNoteOff;
        private readonly byte[] _indexToMidi, _midiToIndex;
        private bool _lightsInvalidated;
        private int _flashTimer;
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
            for (byte y = 0, i = 0; y < info.Height; y++)
            {
                for (byte x = 0; x < info.Width; x++)
                {
                    byte midi = info.Layout[info.Height - y - 1, x];
                    if (midi != 255)
                    {
                        _indexToMidi[i] = midi;
                        _midiToIndex[midi] = i;
                        i++;
                    }
                }
            }

            // Register events
            var layoutMsg = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            layoutMsg[1] = 0x00;
            layoutMsg[2] = 0x01; // X-Y layout
            var brightnessMsg = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            //layoutMsg[1] = 0x1E; // Brightness
            //layoutMsg[2] = 0x00; // 1/3 (max "safe")
            layoutMsg[1] = 0x1F; // Brightness
            layoutMsg[2] = 0x00; // 9/3
            var clearMsg = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            clearMsg[1] = 0x00;
            clearMsg[2] = 0x00; // Clear all lights
            _device.Connected += () =>
            {
                SendMidi(layoutMsg);
                SendMidi(brightnessMsg);
                SendMidi(clearMsg);
                _lightsInvalidated = true;
            };
            _device.Disconnecting += () =>
            {
                SendMidi(clearMsg);
            };

            // Create buffers
            _lights = new Light[info.LightCount];
            _noteOn = Midi.CreateBuffer(MidiMessageType.NoteOn, 1);
            _noteOff = Midi.CreateBuffer(MidiMessageType.NoteOff, 1);
            _topNoteOn = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            _topNoteOff = Midi.CreateBuffer(MidiMessageType.ControlModeChange, 1);
            _topNoteOff[2] = 0x0C;
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
            if (++_flashTimer >= 24)
            {
                _flashState = !_flashState;
                _flashTimer = 0;
            }
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
                        if (midi >= 204 && midi <= 211) // Top Row (+100 to avoid overlapping codes)
                        {
                            _topNoteOff[1] = (byte)(midi - 100);
                            SendMidi(_topNoteOff);
                        }
                        else
                        {
                            _noteOff[1] = midi;
                            SendMidi(_noteOff);
                        }
                        break;
                    case LightMode.Normal:
                    case LightMode.Pulse: // Not supported
                        if (midi >= 204 && midi <= 211) // Top Row (+100 to avoid overlapping codes)
                        {
                            _topNoteOn[1] = (byte)(midi - 100);
                            _topNoteOn[2] = light.Color;
                            SendMidi(_topNoteOn);
                        }
                        else
                        {
                            _noteOn[1] = midi;
                            _noteOn[2] = light.Color;
                            SendMidi(_noteOn);
                        }
                        break;
                    case LightMode.Flash:
                        if (midi >= 204 && midi <= 211) // Top Row (+100 to avoid overlapping codes)
                        {
                            _topNoteOn[1] = (byte)(midi - 100);
                            _topNoteOn[2] = flashState ? light.FlashColor : light.Color;
                            SendMidi(_topNoteOn);
                        }
                        else
                        {
                            _noteOn[1] = midi;
                            _noteOn[2] = flashState ? light.FlashColor : light.Color;
                            SendMidi(_noteOn);
                        }
                        break;
                }
            }
        }

        private void SendMidi(byte[] buffer)
        {
            _device.Send(buffer, buffer.Length);
        }
    }
}