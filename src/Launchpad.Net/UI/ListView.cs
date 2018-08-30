using System;
using System.Collections.Generic;

namespace Launchpad.UI
{
    public class ListView<T> : UIElement
    {
        private readonly List<ListItem<T>> _items;

        public int Count => _items.Count;

        public ListView()
        {
            _items = new List<ListItem<T>>();
        }

        public T this[int index] => _items[index].Value;
        public void Add(T item, Light light)
        {
            if (Count < 64)
                Invalidate();
            _items.Add(new ListItem<T>(item, light));
        }
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            if (Count < 64)
                Invalidate();
        }

        public override void Draw(LaunchpadDevice device)
        {
            for (int i = 0, y = 8, x = 0; i < Math.Min(_items.Count, 64); i++)
            {
                if (i < _items.Count)
                    device.Set(x, y, _items[i].Light);
                else
                    device.SetOff(x, y);
                if (++x >= 8)
                {
                    if (--y < 0)
                        break;
                }
            }
        }
    }
}