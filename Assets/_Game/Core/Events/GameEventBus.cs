using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    public sealed class GameEventBus : IGameEventBus
    {
        private readonly Dictionary<Type, Delegate> _subscribers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            var eventType = typeof(TEvent);
            _subscribers.TryGetValue(eventType, out var current);
            _subscribers[eventType] = Delegate.Combine(current, handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            var eventType = typeof(TEvent);
            if (!_subscribers.TryGetValue(eventType, out var current))
            {
                return;
            }

            var updated = Delegate.Remove(current, handler);
            if (updated == null)
            {
                _subscribers.Remove(eventType);
                return;
            }

            _subscribers[eventType] = updated;
        }

        public void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
        {
            var eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var current) && current is Action<TEvent> callback)
            {
                callback.Invoke(gameEvent);
            }
        }
    }
}
