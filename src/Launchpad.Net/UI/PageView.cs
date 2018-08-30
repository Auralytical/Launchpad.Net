using System;
using System.Collections.Generic;

namespace Launchpad.UI
{
    public class PageView<T> : UIElement
    {
        private readonly List<ListItem<T>> _items;
        private int _pageIndex;

        public int Count => _items.Count;
        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                if (_pageIndex < 0 || _pageIndex >= Count)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _pageIndex = value;
            }
        }
        public int PageCount => (Count + 63) / 64;

        public PageView()
        {
            _items = new List<ListItem<T>>();
            _pageIndex = 0;
        }

        public T this[int index] => _items[index].Value;
        public void Add(T item, Light light)
        {
            _items.Add(new ListItem<T>(item, light));
            if (_pageIndex == Count - 1)
                Invalidate();
        }
        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            if (_pageIndex >= Count) // Last page no longer exists
            {
                _pageIndex--;
                Invalidate();
            }
            else if (_pageIndex >= index)
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