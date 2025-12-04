using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Editor
{
	[Serializable]
	internal class ToolbarReloadScene: BaseToolbarElement
	{
		private static GUIContent _reloadSceneBtn;

		public override string NameInList => "[Button] Reload scene";
		public override int SortingGroup => 3;

		public override void Init()
		{
			_reloadSceneBtn = new GUIContent(
				(Texture2D) AssetDatabase.LoadAssetAtPath($"{GetPackageRootPath}/Editor/CustomToolbar/Icons/LookDevResetEnv@2x.png", typeof(Texture2D)),
				"Reload scene");
		}

		protected override void OnDrawInList(Rect position)
		{

		}

		protected override void OnDrawInToolbar()
		{
			EditorGUIUtility.SetIconSize(new Vector2(17, 17));
			if (GUILayout.Button(_reloadSceneBtn, ToolbarStyles.CommandButtonStyle))
			{
				if (EditorApplication.isPlaying)
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().name);
				}
			}
		}
	}
}