using System;
using System.Collections.Generic;

namespace Launchpad.UI
{
    public class Screen
    {
        private readonly List<UIElement> _elements;

        public IReadOnlyList<UIElement> Elements => _elements;

        public Screen()
        {
            _elements = new List<UIElement>();
        }

        public void AddElement(UIElement element)
        {
            _elements.Add(element);
        }
        public void RemoveElement(UIElement element)
        {
            _elements.Remove(element);
        }

        public void Update(LaunchpadDevice device, IEnumerable<LaunchpadEvent> evnts)
        {
            for (int i = 0; i < _elements.Count; i++)
                _elements[i].Update(device, evnts);
        }
        protected void Draw(LaunchpadDevice device)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                if (_elements[i]._isInvalidated)
                {
                    _elements[i].Draw(device);
                    _elements[i]._isInvalidated = false;
                }
            }
        }
    }
}