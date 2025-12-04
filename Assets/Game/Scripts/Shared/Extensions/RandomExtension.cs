namespace Game.Shared.Extensions
{
	public static class RandomExtension
	{
		public static float NextFloat(this System.Random random, float minValue, float maxValue)
		{
			double range = (double) maxValue - (double) minValue;
			double mantissa = (random.NextDouble() * 2.0) - 1.0;
			double scaled = (mantissa * range) + minValue;
			
			return (float)scaled;
		}
	}
}