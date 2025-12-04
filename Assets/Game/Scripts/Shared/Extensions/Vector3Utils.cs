using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game.Shared
{
	public static class Vector3Utils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Abs(this Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 RotateAround(this Vector3 vector, Vector3 rotationOrigin, Quaternion rotation)
		{
			return rotation * (vector - rotationOrigin) + rotationOrigin;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
		{
			return new Vector3
			{
				x = x ?? vector.x,
				y = y ?? vector.y,
				z = z ?? vector.z
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MinComponent(this Vector3 vector)
		{
			return Mathf.Min(vector.x, Mathf.Min(vector.y, vector.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float MaxComponent(this Vector3 vector)
		{
			return Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ToXZ(this Vector3 vector) => new Vector2(vector.x, vector.z);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Multiply(this Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Divide(this Vector3 v1, Vector3 v2)
		{
			return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
		}
	}
}