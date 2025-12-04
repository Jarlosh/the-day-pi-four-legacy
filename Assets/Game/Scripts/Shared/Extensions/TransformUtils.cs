using System.Collections.Generic;
using UnityEngine;

namespace Game.Shared
{
	public static class TransformUtils
	{
		public static void DestroyChildren(this Transform transform)
		{
			foreach (Transform child in transform)
			{
				Object.Destroy(child.gameObject);
			}
		}

		public static void DestroyImmediateChildren(this Transform transform)
		{
			foreach (Transform child in transform)
			{
				Object.DestroyImmediate(child.gameObject);
			}
		}

		public static IEnumerable<Transform> GetChildren(this Transform transform)
		{
			foreach (Transform child in transform)
			{
				yield return child;
			}
		}
	}
}