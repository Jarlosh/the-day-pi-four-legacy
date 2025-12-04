using UnityEngine;

namespace Game.Shared
{
	public static class Vector2Utils
	{
		public static Vector2 Rotate(Vector2 v, float degrees)
		{
			var rad = degrees * Mathf.Deg2Rad;
			var sin = Mathf.Sin(rad);
			var cos = Mathf.Cos(rad);
			return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
		}
	}
}