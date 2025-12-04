using System;
using System.Collections.Generic;

namespace Game.Shared
{
	public static class NoAllocLinqUtils
	{
		public static void ForEach<T>(this T[] enumerable, Action<T> func)
		{
			foreach (var item in enumerable)
			{
				func(item);
			}
		}
		
		public static bool AnyNoAlloc<TSource>(this List<TSource> source)
		{
			return source.Count > 0;
		}
		
		public static void AddRangeWhereNoAlloc<TSource, TRange>(this List<TSource> source, List<TRange> range, Predicate<TRange> where) where TRange: TSource
		{
			int count = range.Count;
			for (var i = 0; i < count; i++)
			{
				if (where(range[i]))
				{
					source.Add(range[i]);
				}
			}
		}
		
		public static bool AnyNoAlloc<TSource>(this List<TSource> source, Predicate<TSource> predicate)
		{
			foreach (var element in source)
			{
				if (predicate(element))
				{
					return true;
				}
			}
			
			return false;
		}
		
		public static float MaxNoAlloc<TSource>(this List<TSource> source, Func<TSource, float> selector)
		{
			var max = float.MinValue;
			var count = 0;
			foreach (var element in source)
			{
				var value = selector(element);
				if (value > max)
				{
					max = value;
					count++;
				}
			}

			ValidateSequenceContainsElements(count);
			return max;
		}

		public static int MaxNoAlloc<TSource>(this List<TSource> source, Func<TSource, int> selector)
		{
			int max = int.MinValue;
			var count = 0;
			foreach (var element in source)
			{
				var value = selector(element);
				if (value > max)
				{
					max = value;
					count++;
				}
			}

			ValidateSequenceContainsElements(count);
			return max;
		}

		public static float MinNoAlloc<TSource>(this TSource[] source, Func<TSource, float> selector)
		{
			var min = float.MaxValue;
			var count = 0;
			foreach (var element in source)
			{
				var value = selector(element);
				if (value < min)
				{
					min = value;
					count++;
				}
			}

			ValidateSequenceContainsElements(count);
			return min;
		}

		public static float MinNoAlloc<TSource>(this List<TSource> source, Func<TSource, float> selector)
		{
			var min = float.MaxValue;
			var count = 0;
			foreach (var element in source)
			{
				var value = selector(element);
				if (value < min)
				{
					min = value;
					count++;
				}
			}

			ValidateSequenceContainsElements(count);
			return min;
		}
		
		private static void ValidateSequenceContainsElements(int count)
		{
			if (count == 0)
			{
				throw new InvalidOperationException("Sequence contains no elements");
			}
		}
	}
}