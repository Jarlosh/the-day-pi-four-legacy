using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Shared.Extensions
{
	public static class GameObjectExtensions
	{
		public static T[] GetComponentsInChildrenOnly<T>(this MonoBehaviour _this, bool includeInactive = false)
			where T: Component =>
			_this.GetComponentsInChildren<T>(includeInactive).Where(component => component != _this).ToArray();

		public static T[] GetComponentsInChildrenOnly<T>(this GameObject _this, bool includeInactive = false)
			where T: Component =>
			_this.GetComponentsInChildren<T>(includeInactive).Where(component => component.gameObject != _this)
				.ToArray();

		public static T GetComponentSafe<T>(this GameObject gameObject, string assetBundlePath) where T: Component
		{
			var component = gameObject.GetComponent<T>();
			Assert.IsNotNull(component, $"{nameof(component)} game object should have components of type {typeof(T)}");
			return component;
		}

		public static string GetPath(this GameObject go)
		{
			var path = new Stack<string>();
			path.Push(go.name);

			while (go.transform.parent != null)
			{
				go = go.transform.parent.gameObject;
				path.Push(go.name);
			}
			return $"{go.scene.name}.scene/{string.Join("/", path)}";
		}
	}
}