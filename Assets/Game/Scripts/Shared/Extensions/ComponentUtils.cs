using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Shared
{
	public static class ComponentUtils
	{
		public static T AddOrGet<T>(this MonoBehaviour behaviour) where T: Component
		{
			return behaviour.GetComponent<T>() ?? behaviour.gameObject.AddComponent<T>();
		}

		public static T AddOrGet<T>(this GameObject gameObject) where T: Component
		{
			return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
		}

		public static T FindComponent<T>(this Scene scene, bool includeInactive = true)
		{
			foreach (GameObject rootObject in scene.GetRootGameObjects())
			{
				T component = rootObject.GetComponentInChildren<T>(includeInactive);
				if (component != null)
				{
					return component;
				}
			}

			return default;
		}

		public static List<T> FindComponents<T>(this Scene scene, bool includeInactive = true)
		{
			List<T> components = new List<T>();
			foreach (GameObject rootObject in scene.GetRootGameObjects())
			{
				components.AddRange(rootObject.GetComponentsInChildren<T>(includeInactive));
			}

			return components;
		}
	}
}