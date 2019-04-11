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
        
        private readonly Light[] _lights, _oldLights;
        private readonly byte[] _indexToMidi, _midiToIndex;
        private readonly byte[] _clockMsg;
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
            // Mode selection, Standalone mode (default)
            var modeMsg = SysEx.CreateBuffer(_device.Type, new byte[] { 0x21, 0x01 });
            // Standalone Layout select, Programmer
            var layoutMsg = SysEx.CreateBuffer(_device.Type, new byte[] { 0x2C, 0x03 });
            // Set all LEDs, color = 0
            var clearMsg = SysEx.CreateBuffer(_device.Type, new byte[] { 0x0E, 0x0 });
            _device.Connected += () =>
            {
                SendBuffer(modeMsg);
                SendBuffer(layoutMsg);
                SendBuffer(clearMsg);
                _lightsInvalidated = true;
            };
            _device.Disconnecting += () =>
            {
                SendBuffer(clearMsg);
            };

            // Create buffers
            _lights = new Light[info.LightCount];
            _oldLights = new Light[info.LightCount];
            _clockMsg = Midi.CreateBuffer(MidiMessageType.MidiClock, 1); // MIDI Clock
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
                case LightMode.RGB: Set(midiId, light.R, light.G, light.B); break;
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
            SendBuffer(_clockMsg);
        }

        public void Render()
        {
            if (!_lightsInvalidated)
                return;
            
            List<byte> msgs_note_off = new List<byte>() { 0x0A};
            List<byte> msgs_note_normal = new List<byte>() { 0x0A };
            List<byte> msgs_note_rgb1 = new List<byte>() { 0x0B };
            List<byte> msgs_note_rgb2 = new List<byte>() { 0x0B };
            List<byte> msgs_note_pulse = new List<byte>() { 0x28 };
            List<byte> msgs_note_flash = new List<byte>() { 0x23 };

            for (int i = 0; i < _lights.Length; i++)
            {
                //if (_oldLights[i].Equals(_lights[i]))
                //    continue;
                byte midi = _indexToMidi[i];
                var light = _lights[i];
                switch (light.Mode)
                {
                    case LightMode.Off:
                        msgs_note_off.Add(midi);
                        msgs_note_off.Add(0);
                        break;
                    case LightMode.Normal:
                        msgs_note_normal.Add(midi);
                        msgs_note_normal.Add(light.Color);
                        break;
                    case LightMode.RGB:
                        // lpd pro: max rgb sysex cap 78. limit to 40 in a single sysex msg
                        if ((msgs_note_rgb1.Count - 1) < (4 * 40))
                        {
                            msgs_note_rgb1.Add(midi);
                            msgs_note_rgb1.Add(light.R);
                            msgs_note_rgb1.Add(light.G);
                            msgs_note_rgb1.Add(light.B);
                        }else
                        {
                            msgs_note_rgb2.Add(midi);
                            msgs_note_rgb2.Add(light.R);
                            msgs_note_rgb2.Add(light.G);
                            msgs_note_rgb2.Add(light.B);
                        }
                        break;
                    case LightMode.Pulse:
                        msgs_note_pulse.Add(midi);
                        msgs_note_pulse.Add(light.Color);
                        break;
                    case LightMode.Flash:
                        msgs_note_normal.Add(midi);
                        msgs_note_normal.Add(light.Color);
                        msgs_note_flash.Add(midi);
                        msgs_note_flash.Add(light.FlashColor);
                        break;
                }
            }
            
            SendSysExArray(msgs_note_off);
            SendSysExArray(msgs_note_normal);
            SendSysExArray(msgs_note_rgb1);
            SendSysExArray(msgs_note_rgb2);
            SendSysExArray(msgs_note_pulse);
            SendSysExArray(msgs_note_flash);


            for (int i = 0; i < _lights.Length; i++)
            {
                _oldLights[i] = _lights[i];
            }

            _lightsInvalidated = false;
        }

        private void SendSysExArray(List<byte> msgs)
        {
            if (msgs.Count > 1)
            {
                SendBuffer(SysEx.CreateBuffer(_device.Type, msgs.ToArray()));
            }
        }

        private void SendBuffer(byte[] buffer)
            => _device.Send(buffer);

        public void Set(byte midiId, byte red, byte green, byte blue)
        {
            byte index = _midiToIndex[midiId];
            if (index > byte.MaxValue)
                return;
            _lights[index] = new Light(LightMode.RGB, red, green, blue);
            _lightsInvalidated = true;
        }
    }
}