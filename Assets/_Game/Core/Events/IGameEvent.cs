using System;

namespace Game.Core.Events
{
    public interface IGameEvent { }

    public interface IGameEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent;
        void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent;
    }
}
