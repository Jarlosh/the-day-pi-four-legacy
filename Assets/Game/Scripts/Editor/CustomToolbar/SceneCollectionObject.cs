using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Editor
{
	[CreateAssetMenu(menuName = "Editor/Scene Collection")]
	public class SceneCollectionObject: ScriptableObject
	{
		public List<SceneAsset> Scenes;

		public void Open()
		{
			for (var i = 0; i < Scenes.Count; i++)
			{
				var scene = Scenes[i];
                
				var mode = (i == 0) ? OpenSceneMode.Single : OpenSceneMode.Additive;
				EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scene), mode);
			}
		}
	}
}