using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[Serializable]
	internal class ToolbarReserializeSelected: BaseToolbarElement
	{
		private static GUIContent _reserializeSelectedBtn;

		public override string NameInList => "[Button] Reserialize selected";
		public override int SortingGroup => 5;

		public override void Init()
		{
			_reserializeSelectedBtn = EditorGUIUtility.IconContent("Refresh");
			_reserializeSelectedBtn.tooltip = "Reserialize Selected Assets";
		}

		protected override void OnDrawInList(Rect position)
		{

		}

		protected override void OnDrawInToolbar()
		{
			if (GUILayout.Button(_reserializeSelectedBtn, ToolbarStyles.CommandButtonStyle))
			{
				ForceReserializeAssetsUtils.ForceReserializeSelectedAssets();
			}
		}
	}
}