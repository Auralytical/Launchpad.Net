using System;
using System.Collections.Generic;

namespace Launchpad.UI
{
    public class PowerLight : UIElement
    {
        private Light _light;
        public Light Light
        {
            get => _light;
            set
            {
                _light = value;
                Invalidate();
            }
        }

        public override void Draw(LaunchpadDevice device)
        {
            device.Set(SystemButton.PowerLight, _light);
        }
    }
}