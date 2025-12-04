using System;
using System.Collections.Generic;

namespace Game.Core
{
	public static class ServiceLocator
	{
		private static readonly Dictionary<Type, object> _services = new();

		public static void Register<TContract, TImplementation>(TImplementation service) where TImplementation : TContract
		{
			_services[typeof(TContract)] = service;
		}
		
		public static void Register<T>(T service)
		{
			var type = typeof(T);
			_services[type] = service;
		}

		public static T Get<T>()
		{
			var type = typeof(T);
			
			if (_services.TryGetValue(type, out var service))
			{
				return (T)service;
			}
			
			throw new Exception($"Service {typeof(T).Name} is not registered");
		}

		public static void Clear()
		{
			_services.Clear();
		}
	}
}