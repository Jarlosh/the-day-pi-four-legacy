using UnityEngine;

namespace Game.Shared
{
	public static class MathUtils
	{
		/// <summary>
		/// Map a value from one number scale to another 
		/// </summary>
		public static float MapValue(float value, float min, float max, float newMin, float newMax)
		{
			return Mathf.Clamp(newMin + (value - min) * (newMax - newMin) / (max - min), newMin, newMax);
		}

		public static bool InLayerMask(int layer, int layerMask)
		{
			return ((1 << layer) & layerMask) == (1 << layer);
		}

		public static bool GreaterOrEquals(this float lhs, float rhs)
		{
			return (double) lhs > (double) rhs || Mathf.Approximately(lhs, rhs);
		}

		public static bool InRangeInclusive(this float value, float min, float max)
		{
			return value.GreaterOrEquals(min) && value.LessOrEquals(max);
		}
		
		public static bool LessOrEquals(this float lhs, float rhs)
		{
			return (double) lhs < (double) rhs || Mathf.Approximately(lhs, rhs);
		}
		
		public static float MinToSec(float min) => min * 60f;

		public static float SecToMin(float sec) => sec / 60f;
	}
}