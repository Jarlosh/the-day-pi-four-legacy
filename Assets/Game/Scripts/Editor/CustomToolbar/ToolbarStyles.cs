using UnityEngine;

namespace Game.Editor
{
	public static class ToolbarStyles
	{
		public static readonly GUIStyle CommandButtonStyle;

		static ToolbarStyles()
		{
			CommandButtonStyle = new GUIStyle("Command")
			{
				fontSize = 12,
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageAbove,
				margin = new RectOffset(0, 0, 0, 0),
				overflow = new RectOffset(0, 0, 0, 0),
				fixedHeight = 20,
				fontStyle = FontStyle.Bold
			};
		}
	}
}