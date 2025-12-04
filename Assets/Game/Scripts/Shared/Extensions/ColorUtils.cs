using UnityEngine;

namespace Game.Shared
{
	public static class ColorUtils
	{
		public static string ToHex(this Color color)
		{
			return $"#{ColorUtility.ToHtmlStringRGB(color)}";
		}
		
		public static Color WithAlpha(this Color color, float alpha)
		{
			color.a = alpha;
			return color;
		}

		public static Color RandomAlpha(this Color color)
		{
			color.a = UnityEngine.Random.Range(0.0f, 1f);
			return color;
		}
	}
}