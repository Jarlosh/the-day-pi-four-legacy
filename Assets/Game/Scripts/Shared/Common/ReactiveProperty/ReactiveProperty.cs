using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Shared
{
	public interface IReadOnlyReactiveProperty<T> : IObservable<T>
	{
		T Value { get; }
		bool HasValue { get; }
	}

	public interface IReactiveProperty<T> : IReadOnlyReactiveProperty<T>
	{
		new T Value { get; set; }
	}

	public class ReactiveProperty<T> : IReactiveProperty<T>
	{
		private class Subscription : IDisposable
		{
			private ReactiveProperty<T> _property;
			private IObserver<T> _observer;

			public Subscription(ReactiveProperty<T> property, IObserver<T> observer)
			{
				_property = property;
				_observer = observer;
				
				observer.OnCompleted();
			}

			public void Dispose()
			{
				_property.Unsubscribe(_observer);
			}
		}

		private T _value;
		private List<IObserver<T>> _observers = new List<IObserver<T>>();
		private bool _isDisposed = false;
		
		public event Action<T> EventChanged;

		public T Value
		{
			get => _value;
			set
			{
				if (!Equals(_value, value))
				{
					_value = value;
					if (!_isDisposed)
					{
						NotifyObservers(_value);
						EventChanged?.Invoke(_value);
					}
				}
			}
		}

		public bool HasValue => true;

		public ReactiveProperty(T initialValue)
		{
			_value = initialValue;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			_observers.Add(observer);
			
			return new Subscription(this, observer);
		}

		private void Unsubscribe(IObserver<T> observer)
		{
			_observers.Remove(observer);
		}

		private void NotifyObservers(T value)
		{
			foreach (var observer in _observers)
			{
				observer.OnNext(value);
			}
		}

		public void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
			}

			_observers.Clear();
		}
	}
}