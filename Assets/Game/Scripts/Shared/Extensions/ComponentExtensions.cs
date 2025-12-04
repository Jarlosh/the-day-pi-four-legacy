using UnityEngine;

namespace Game.Shared.Extensions
{
	public static class ComponentExtensions
	{
		/// <summary>
		/// Path + Type
		/// </summary>
		/// <param name="monoBehaviour"></param>
		/// <returns></returns>
		public static string GetFullName(this Component monoBehaviour)
		{
			if (monoBehaviour == null)
			{
				// ReSharper disable once PossibleNullReferenceException
				return $"Destroyed Behaviour!({monoBehaviour.GetType()})";
			}

			return monoBehaviour.GetPath() + " (" + monoBehaviour.GetType() + ")" + " ( scene = "
				+ monoBehaviour.gameObject.scene.name + ") " + " ( HashCode = " + monoBehaviour.GetHashCode() + ")";
		}

		public static string GetPath(this Component monoBehaviour)
		{
			if (monoBehaviour == null)
			{
				// ReSharper disable once PossibleNullReferenceException
				return $"Destroyed Behaviour!({monoBehaviour.GetType()})";
			}

			string fullName = "/" + monoBehaviour.name;
			Transform parent = monoBehaviour.gameObject.transform.parent;
			while (parent != null)
			{
				fullName = "/" + parent.name + fullName;
				parent = parent.parent;
			}
			return fullName + " (" + monoBehaviour.GetType() + ")";
		}
	}
}