using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Launchpad
{    
    public interface IRenderer
    {
        void Clear();
   
        void Set(byte midiId, Light light);
        void Set(byte midiId, byte color);
        void Set(byte midiId, byte red, byte green, byte blue = 0xFF);  // color: 0 - 127
        void SetOff(byte midiId);
        void SetPulse(byte midiId, byte color);       
        void SetFlash(byte midiId, byte color1, byte color2);

        void ClockTick();
        void Render();
    }
}