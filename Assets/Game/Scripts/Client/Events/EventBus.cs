using System;
using System.Collections.Generic;
using System.Linq;
using Game.Shared;

namespace Game.Client
{
    // todo: make thread safe   
    public class EventBus: Singleton<EventBus>
    {
        private readonly Dictionary<Type, HashSet<Delegate>> _subscribers = new Dictionary<Type, HashSet<Delegate>>();
        
        public void Subscribe<T>(Action<T> subscriber)
        {
            Subscribe(typeof(T), subscriber);
        }

        private void Subscribe<T>(Type eventType, Action<T> callback)
        {
            if (!_subscribers.TryGetValue(eventType, out var subscribers))
            {
                _subscribers[eventType] = subscribers = new HashSet<Delegate>();
            }
            subscribers.Add(callback);
        }

        public void Unsubscribe<T>(Action<T> subscriber)
        {
            Unsubscribe(typeof(T), subscriber);
        }

        private void Unsubscribe<T>(Type subscriber, Action<T> action)
        {
            if (_subscribers.TryGetValue(subscriber, out var subscribers))
            {
                subscribers.Remove(action);
            }
        }
        
        public void Publish<T>(T evt)
        {
            if (!_subscribers.TryGetValue(typeof(T), out var subscribers))
            {
                return;
            }

            // todo: add callback's generic param check
            foreach (var callback in subscribers.ToList())
            {
                callback.DynamicInvoke(evt);
            }
        }
    }
}