using System;

namespace Game.Shared
{
	internal static class Stubs
	{
		public static readonly Action Nop = () => { };
		public static readonly Action<Exception> Throw = ex => { ex.Throw(); };
	}
	
	public static class Observer
	{
		internal static IObserver<T> CreateSubscribeObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted)
		{
			return new ActionObserver<T>(onNext, onError, onCompleted);
		}
	}

	public static class ObservableExtensions
	{
		public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
		{
			return source.Subscribe(Observer.CreateSubscribeObserver(onNext, Stubs.Throw, Stubs.Nop));
		}
		
		public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
		{
			return source.Subscribe(Observer.CreateSubscribeObserver(onNext, onError, Stubs.Nop));
		}

		public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
		{
			return source.Subscribe(Observer.CreateSubscribeObserver(onNext, Stubs.Throw, onCompleted));
		}

		public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
		{
			return source.Subscribe(Observer.CreateSubscribeObserver(onNext, onError, onCompleted));
		}
	}
}