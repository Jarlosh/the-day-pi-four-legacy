namespace Game.Core
{
	public class LayerManager
	{
		// Built-in Unity layers.
		private const int DefaultLayer = 0;
		private const int TransparentFXLayer = 1;
		private const int IgnoreRaycastLayer = 2;
		private const int WaterLayer = 4;
		private const int UILayer = 5;
		private const int TransparentWallLayer = 6;
		
		public static int Default => DefaultLayer;
		public static int TransparentFX => TransparentFXLayer;
		public static int IgnoreRaycast => IgnoreRaycastLayer;
		public static int Water => WaterLayer;
		public static int UI => UILayer;
		public static int TransparentWall => TransparentWallLayer;
	}
}