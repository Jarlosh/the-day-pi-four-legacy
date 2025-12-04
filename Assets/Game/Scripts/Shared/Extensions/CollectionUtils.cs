using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Shared
{
	public static class CollectionUtils
	{
		public static void AddRange<TKey>(this HashSet<TKey> set, IEnumerable<TKey> keys)
		{
			foreach (var key in keys)
			{
				set.Add(key);
			}
		}

		public static void RemoveRange<TKey>(this HashSet<TKey> set, IEnumerable<TKey> keys)
		{
			foreach (var key in keys)
			{
				set.Remove(key);
			}
		}

		public static void RemoveRange<TSource>(this List<TSource> set, IEnumerable<TSource> keys)
		{
			foreach (var key in keys)
			{
				set.Remove(key);
			}
		}

		public static TValue Find<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
		{
			dictionary.TryGetValue(key, out TValue value);
			return value;
		}

		public static TValue EnsureValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue: new()
		{
			if (!dictionary.TryGetValue(key, out TValue value))
			{
				dictionary[key] = value = new TValue();
			}

			return value;
		}

		public static TValue Get<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
		{
			return dictionary[key];
		}

		public static TValue EnsureValue<TKey, TValue>(
			this IDictionary<TKey, TValue> dictionary, 
			TKey key, 
			Func<TValue> fallback)
		{
			if (!dictionary.TryGetValue(key, out TValue value))
			{
				dictionary[key] = value = fallback();
			}
			return value;
		}

		public static TValue EnsureValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
		{
			if (!dictionary.TryGetValue(key, out TValue value))
			{
				dictionary[key] = value = defaultValue;
			}

			return value;
		}

		public static void AddOrRemove<TKey>(this ICollection<TKey> collection, TKey item, bool shouldAdd)
		{
			if (shouldAdd)
			{
				collection.Add(item);
			}
			else
			{
				collection.Remove(item);
			}
		}

		public static void Insert<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out List<TValue> list))
			{
				list.Add(value);
			}
			else
			{
				dictionary[key] = new List<TValue> { value };
			}
		}

		public static void Insert<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out HashSet<TValue> list))
			{
				list.Add(value);
			}
			else
			{
				dictionary[key] = new HashSet<TValue> { value };
			}
		}

		public static void Clear<TKey>(this List<TKey> list, bool withDispose) where TKey: IDisposable
		{
			if (withDispose)
			{
				for (int index = list.Count - 1; index >= 0; index--)
				{
					list[index].Dispose();
				}
			}

			list.Clear();
		}

		public static void Delete<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out HashSet<TValue> list))
			{
				list.Remove(value);
				if (list.Count == 0)
				{
					dictionary.Remove(key);
				}
			}
		}

		public static void DeleteButKeepHashSet<TKey, TValue>(this IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out HashSet<TValue> list))
			{
				list.Remove(value);
			}
		}

		public static void Delete<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out List<TValue> list))
			{
				list.Remove(value);
				if (list.Count == 0)
				{
					dictionary.Remove(key);
				}
			}
		}

		public static void Remove<T>(this Queue<T> queue, T itemToRemove) where T: class
		{
			var list = queue.ToList();
			queue.Clear();

			foreach (var item in list)
			{
				if (item == itemToRemove)
				{
					continue;
				}

				queue.Enqueue(item);
			}
		}

		public static void RemoveAt<T>(this Queue<T> queue, int itemIndex)
		{
			for (int i = 0; i < queue.Count; i++)
			{
				T item = queue.Dequeue();
				if (i == itemIndex)
				{
					continue;
				}

				queue.Enqueue(item);
			}
		}

		public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
		{
			ArrayList.Adapter((IList)list).Sort(Comparer<T>.Create(comparison));
		}

		// Sorts in IList<T> in place, when T is IComparable<T>
		public static void Sort<T>(this IList<T> list) where T: IComparable<T>
		{
			Sort(list, Comparison);
			
			int Comparison(T l, T r) => l.CompareTo(r);
		}
		
		public static int SiblingComparer<T>(T a, T b) where T: Component
		{
			return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
		}

		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			System.Random rnd = new System.Random();

			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rnd.Next(n + 1);
				(list[k], list[n]) = (list[n], list[k]);
			}
			return list;
		}
		
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
		{
			System.Random rnd = new System.Random();

			if (items is not IList<T> list)
			{
				list = items.ToList();
			}
			
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rnd.Next(n + 1);
				(list[k], list[n]) = (list[n], list[k]);
			}
			return list;
		}
		
		public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
		{
			return enumerable.Where(e => !predicate(e));
		} 
		
		public static IEnumerable<TR> WhereIs<TR>(this IEnumerable enumerable)
		{
			foreach (var item in enumerable)
			{
				if (item is TR result)
				{
					yield return result;
				}
			}
		} 
		
		public static IEnumerable<T> WhereNotDefault<T>(this IEnumerable<T> enumerable)
		{
			return enumerable.Where(e => !e.Equals(default));
		}

		public static bool TryFirst<T>(this IEnumerable<T> enumerable, out T result) where T: class
		{
			result = enumerable.FirstOrDefault();
			return (result is not null);
		}

		public static bool TryFirst<T>(this IEnumerable<T> enumerable, Predicate<T> predicate, out T result)
		{
			foreach (T item in enumerable)
			{
				if (predicate(item))
				{
					result = item;
					return true;
				}
			}
			result = default;
			return false;
		}

		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
		{
			foreach (var item in enumerable)
			{
				func(item);
			}
		}
		
		public static void ForEach<Tkey, Tvalue>(this Dictionary<Tkey, Tvalue> enumerable, Action<KeyValuePair<Tkey, Tvalue>> func)
		{
			foreach (var item in enumerable)
			{
				func(item);
			}
		}
		
		public static void ForEach<T, TR>(this IEnumerable<T> enumerable, Func<T, TR> func)
		{
			foreach (var item in enumerable)
			{
				func(item);
			}
		}

		public static bool AnyDifferencesWithoutElementOrder<T>(IEnumerable<T> listA, IEnumerable listB, List<T> aWithoutB)
		{
			if (!Equals(listA, aWithoutB))
			{
				aWithoutB.Clear();
				aWithoutB.AddRange(listA);
			}

			bool differences = false;
			foreach (T b in listB)
			{
				if (!aWithoutB.Remove(b))
				{
					differences = true;
				}
			}

			differences |= aWithoutB.Count > 0;
			return differences;
		}
	}
}