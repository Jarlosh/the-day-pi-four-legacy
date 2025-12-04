using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Game.Editor
{
	[InitializeOnLoad]
	public static class CustomToolbarInitializer
	{
		static CustomToolbarInitializer()
		{
#if UNITY_2020_3_OR_NEWER
			CustomToolbarSetting setting = ScriptableSingleton<CustomToolbarSetting>.instance;
#else
            CustomToolbarSetting setting = CustomToolbarSetting.GetOrCreateSetting();
#endif

			setting.Elements.ForEach(element => element.Init());

			List<BaseToolbarElement> leftTools = setting.Elements.TakeWhile(element => !(element is ToolbarSides)).ToList();
			List<BaseToolbarElement> rightTools = setting.Elements.Except(leftTools).ToList();
			IEnumerable<Action> leftToolsDrawActions = leftTools.Select(TakeDrawAction);
			IEnumerable<Action> rightToolsDrawActions = rightTools.Select(TakeDrawAction);

			ToolbarExtender.LeftToolbarGUI.AddRange(leftToolsDrawActions);
			ToolbarExtender.RightToolbarGUI.AddRange(rightToolsDrawActions);
		}

		private static Action TakeDrawAction(BaseToolbarElement element)
		{
			Action action = element.DrawInToolbar;
			return action;
		}
	}
}