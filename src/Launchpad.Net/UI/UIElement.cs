using System;
using System.Collections.Generic;

namespace Launchpad.UI
{
    public abstract class UIElement
    {
        internal bool _isInvalidated;

        protected UIElement()
        {
            _isInvalidated = true;
        }

        public void Invalidate()
        {
            _isInvalidated = true;
        }

        public virtual void Update(LaunchpadDevice device, IEnumerable<LaunchpadEvent> evnts) { }
        public virtual void Draw(LaunchpadDevice device) { }
    }
}