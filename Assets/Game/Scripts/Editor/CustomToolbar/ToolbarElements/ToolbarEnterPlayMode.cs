using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[Serializable]
	internal class ToolbarEnterPlayMode: BaseToolbarElement
	{
#if UNITY_2019_3_OR_NEWER
		private int _selectedEnterPlayMode;
		private string[] _enterPlayModeOption;
#endif

		public override string NameInList => "[Dropdown] Enter play mode option";
		public override int SortingGroup => 2;

		public override void Init()
		{
			_enterPlayModeOption = new[]
			{
				"Disabled",
				"Reload All",
				"Reload Scene",
				"Reload Domain",
				"FastMode",
			};
		}

		protected override void OnDrawInList(Rect position)
		{
		}

		protected override void OnDrawInToolbar()
		{
#if UNITY_2019_3_OR_NEWER
			if (EditorSettings.enterPlayModeOptionsEnabled)
			{
				EnterPlayModeOptions option = EditorSettings.enterPlayModeOptions;
				_selectedEnterPlayMode = (int) option + 1;
			}

			_selectedEnterPlayMode = EditorGUILayout.Popup(_selectedEnterPlayMode, _enterPlayModeOption, GUILayout.Width(WidthInToolbar));

			if (GUI.changed && 0 <= _selectedEnterPlayMode && _selectedEnterPlayMode < _enterPlayModeOption.Length)
			{
				EditorSettings.enterPlayModeOptionsEnabled = _selectedEnterPlayMode != 0;
				EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions) (_selectedEnterPlayMode - 1);
			}
#endif
		}
	}
}