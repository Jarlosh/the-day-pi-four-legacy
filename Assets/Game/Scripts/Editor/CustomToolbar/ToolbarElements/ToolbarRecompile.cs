using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[Serializable]
	internal class ToolbarRecompile: BaseToolbarElement
	{
		private static GUIContent _recompileBtn;

		public override string NameInList => "[Button] Recompile";
		public override int SortingGroup => 5;

		public override void Init()
		{
			_recompileBtn = EditorGUIUtility.IconContent("WaitSpin05");
			_recompileBtn.tooltip = "Recompile";
		}

		protected override void OnDrawInList(Rect position)
		{

		}

		protected override void OnDrawInToolbar()
		{
			if (GUILayout.Button(_recompileBtn, ToolbarStyles.CommandButtonStyle))
			{
				UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
				Debug.Log("Recompile");
			}
		}
	}
}