using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    /// <summary>A notice board for one type of notice: listeners sign up, publishers pin a notice.</summary>
    public sealed class Channel<T>
    {
        private readonly List<Action<T>> _listeners = new List<Action<T>>();

        public void Subscribe(Action<T> listener)
        {
            _listeners.Add(listener);
        }

        public void Unsubscribe(Action<T> listener)
        {
            _listeners.Remove(listener);
        }

        // A plain for loop keeps this allocation-free (a foreach over List<T> would also be fine).
        public void Publish(T notice)
        {
            for (var i = 0; i < _listeners.Count; i++)
            {
                _listeners[i](notice);
            }
        }
    }
}
