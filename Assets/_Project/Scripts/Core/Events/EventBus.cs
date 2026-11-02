using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    /// <summary>Global notice board for cross-system announcements such as "level won". One channel per notice type.</summary>
    public sealed class EventBus
    {
        // Values are typed as object because each channel has a different T; casts below are safe
        // since a channel is only ever stored under the type it was created for.
        private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

        // Publishing with no listeners is normal (tests, scene loads), so it does nothing.
        public void Publish<T>(T notice)
        {
            if (_channels.TryGetValue(typeof(T), out var channel))
                ((Channel<T>)channel).Publish(notice);
        }

        public void Subscribe<T>(Action<T> listener)
        {
            GetOrCreate<T>().Subscribe(listener);
        }

        public void Unsubscribe<T>(Action<T> listener)
        {
            if (_channels.TryGetValue(typeof(T), out var channel))
                ((Channel<T>)channel).Unsubscribe(listener);
        }

        private Channel<T> GetOrCreate<T>()
        {
            if (_channels.TryGetValue(typeof(T), out var existing))
                return (Channel<T>)existing;

            var created = new Channel<T>();
            _channels[typeof(T)] = created;
            return created;
        }
    }
}
