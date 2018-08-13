using System;

namespace Launchpad.Engines.Emulator
{
    internal class EmulatorLaunchpadMidiDevice : LaunchpadMidiDevice
    {
        private readonly LED[] _leds;
        private bool _ledsInvalidated;

        internal EmulatorLaunchpadMidiDevice(string id, string name, DeviceType type)
            : base(id, name, type)
        {
            _leds = new LED[LaunchpadMidiDevice.MaxLEDCount];
        }

        protected override bool ConnectInternal(bool isNormal) => true;
        protected override void DisconnectInternal(bool isNormal) { }

        protected override bool SendInternal(byte[] buffer, int count)
        {
            if (!SysEx.IsValid(buffer, count, Type))
                return false;

            count--;
            int i = 6;
            switch (buffer[i++])
            {
                case 0x0A: // Set LEDS
                    while (i < count - 1)
                    {
                        byte led = buffer[i++];
                        byte color = buffer[i++];
                        byte index = GetIndex(led);
                        if (index != byte.MaxValue)
                        {
                            _leds[index].Color = color;
                            _leds[index].FlashColor = 0;
                            _leds[index].Mode = color != 0 ? LEDMode.Normal : LEDMode.Off;
                        }
                    }
                    _ledsInvalidated = true;
                    break;
                case 0x0B: // Set LEDS (RGB)
                    throw new NotSupportedException();
                case 0x0C: // Set LEDS (by col)
                    while (i < count - 1)
                    {
                        byte x = buffer[i++];
                        byte color = buffer[i++];
                        for (int y = 0; y < 9; y++)
                        {
                            byte index = GetIndex(x, y);
                            if (index != byte.MaxValue)
                            {
                                _leds[index].Color = color;
                                _leds[index].FlashColor = 0;
                                _leds[index].Mode = color != 0 ? LEDMode.Normal : LEDMode.Off;
                            }
                        }
                    }
                    _ledsInvalidated = true;
                    break;
                case 0x0D: // Set LEDS (by row)
                    while (i < count - 1)
                    {
                        byte y = buffer[i++];
                        byte color = buffer[i++];
                        for (int x = 0; x < 9; x++)
                        {
                            byte index = GetIndex(x, y);
                            if (index != byte.MaxValue)
                            {
                                _leds[index].Color = color;
                                _leds[index].FlashColor = 0;
                                _leds[index].Mode = color != 0 ? LEDMode.Normal : LEDMode.Off;
                            }
                        }
                    }
                    _ledsInvalidated = true;
                    break;
                case 0x0E: // Set LEDS (all)
                    {
                        byte color = buffer[i++];
                        for (int index = 0; i < _leds.Length; i++)
                        {
                            _leds[index].Color = color;
                            _leds[index].FlashColor = 0;
                            _leds[index].Mode = color != 0 ? LEDMode.Normal : LEDMode.Off;                        
                        }
                    }
                    _ledsInvalidated = true;
                    break;
                case 0x0F: // Set LEDS (grid)
                    throw new NotSupportedException();
                case 0x14: // Scroll text
                    throw new NotSupportedException();
                case 0x21: // Mode Select
                    if (buffer[7] == 1) // Standalone
                        break; //Ignore
                    throw new NotSupportedException();
                case 0x22: // Ableton Layout
                    throw new NotSupportedException();
                case 0x23: // Flash LED
                    throw new NotSupportedException();
                case 0x28: // Pulse LED
                    throw new NotSupportedException();
                case 0x2B: // Fader Setup
                    throw new NotSupportedException();
                case 0x2C: // Standalone Layout
                    if (buffer[7] == 3) // Programmer
                        break; //Ignore
                    throw new NotSupportedException();
                case 0x2D: // Mode Status
                    throw new NotSupportedException();
                case 0x2E: // Ableton Layout Status
                    throw new NotSupportedException();
                case 0x2F: // Standalone Layout Status
                    throw new NotSupportedException();
            }

            if (_ledsInvalidated)
                Render();
            return true;
        }

        private void Render()
        {
            Console.Clear();
            for (int y = 9; y >= 0; y--)
            {
                for (int x = 0; x <= 9; x++)
                {
                    byte index = GetIndex(x, y);
                    if (index == byte.MaxValue)
                        Console.Write(' ');
                    else
                    {
                        var led = _leds[index];
                        if (led.Mode == LEDMode.Off)
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                        else
                            Console.ForegroundColor = ConsoleColor.White;
                        Console.Write('█');
                    }
                }
                Console.WriteLine();
            }
            _ledsInvalidated = false;
        }
    }
}