using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[Serializable]
	internal class ToolbarReserializeAll: BaseToolbarElement
	{
		private static GUIContent _reserializeAllBtn;

		public override string NameInList => "[Button] Reserialize all";
		public override int SortingGroup => 5;

		public override void Init()
		{
			_reserializeAllBtn = EditorGUIUtility.IconContent("P4_Updating");
			_reserializeAllBtn.tooltip = "Reserialize All Assets";
		}

		protected override void OnDrawInList(Rect position)
		{

		}

		protected override void OnDrawInToolbar()
		{
			if (GUILayout.Button(_reserializeAllBtn, ToolbarStyles.CommandButtonStyle))
			{
				ForceReserializeAssetsUtils.ForceReserializeAllAssets();
			}
		}
	}
}