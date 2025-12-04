using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Editor
{
	[Serializable]
	[InitializeOnLoad]
	internal class ToolbarSceneSelection: BaseToolbarElement
	{
		private enum SceneSelectionSource
		{
			Collection,
			AllScene
		}
		
		public override string NameInList => "[Dropdown] Scene selection";
		public override int SortingGroup => 2;

		[SerializeField] private bool _showSceneFolder = true;
		[SerializeField] private SceneSelectionSource _sceneSelection = SceneSelectionSource.Collection;

		private static SceneCollectionObject _cachedSceneCollection;
		private static GenericMenu _sceneCollectionMenu;
		private static string _lastSceneCollectionGuid;
		
		private SceneData[] _scenesPopupDisplay;
		private List<string> _scenesPath = new();
		private List<string> _scenesBuildPath = new();
		private int _selectedSceneIndex;

		private List<SceneData> _toDisplay = new List<SceneData>();
		private string[] _sceneGuids;
		private Scene _activeScene;
		private int _usedIds;
		private string _name;
		private GUIContent _content;
		private bool _isPlaceSeparator;

		public override void Init()
		{
			SubscribeToEvents();
			RefreshScenesList();
			EditorSceneManager.sceneOpened -= HandleSceneOpened;
			EditorSceneManager.sceneOpened += HandleSceneOpened;
		}
		
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			EditorApplication.quitting += OnEditorQuitting;
			AssemblyReloadEvents.beforeAssemblyReload += UnsubscribeFromEvents;
		}
		
		private static void OnEditorQuitting()
		{
			UnsubscribeFromEvents();

			EditorApplication.quitting -= OnEditorQuitting;
			AssemblyReloadEvents.beforeAssemblyReload -= UnsubscribeFromEvents;
		}
		
		private static void SubscribeToEvents()
		{
			EditorApplication.projectChanged += InvalidateCache;
		}

		private static void UnsubscribeFromEvents()
		{
			EditorApplication.projectChanged -= InvalidateCache;
		}
		
		private static void InvalidateCache()
		{
			_cachedSceneCollection = null;
			_lastSceneCollectionGuid = null;
		}
		
		protected override void OnDrawInList(Rect position)
		{
			position.width = 200.0f;
			_showSceneFolder = EditorGUI.Toggle(position, "Group by folders", _showSceneFolder);
			
			position.x += position.width + FieldSizeSpace;
			position.width = 300.0f;
			_sceneSelection = (SceneSelectionSource) EditorGUI.EnumPopup(position, "Scene source", _sceneSelection);

			if (GUI.changed)
			{
				_toDisplay.Clear();
				RefreshScenesList();
			}
		}

		protected override void OnDrawInToolbar()
		{
			EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
			DrawSceneDropdown();
			EditorGUI.EndDisabledGroup();
		}

		private void DrawSceneDropdown()
		{
			_selectedSceneIndex = EditorGUILayout.Popup(_selectedSceneIndex, _scenesPopupDisplay.Select(e => e.PopupDisplay).ToArray(), GUILayout.Width(WidthInToolbar));

			if (GUI.changed && 0 <= _selectedSceneIndex && _selectedSceneIndex < _scenesPopupDisplay.Length)
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					foreach (var scenePath in _scenesPath)
					{
						if ((scenePath) == _scenesPopupDisplay[_selectedSceneIndex].Path)
						{
							EditorSceneManager.OpenScene(scenePath);
							break;
						}
					}
				}
			}
		}

		private void RefreshScenesList()
		{
			InitScenesData();
			
			switch (_sceneSelection)
			{
				case SceneSelectionSource.Collection:
				{
					AddedScenesFromCollection();
					break;
				}
				case SceneSelectionSource.AllScene:
				{
					AddedAllScenes();
					break;
				}
			}

			void AddedScenesFromCollection()
			{
				var sceneCollection = GetSceneCollection();
				if (sceneCollection == null || sceneCollection.Scenes == null || sceneCollection.Scenes.Count == 0)
				{
					Debug.LogWarning("Scene collection is empty. Add scenes to the collection first.");
					return;
				}

				for (var i = 0; i < sceneCollection.Scenes.Count; i++)
				{
					var sceneAsset = sceneCollection.Scenes[i];
					if (sceneAsset != null)
					{
						_scenesPath[i] = AssetDatabase.GetAssetPath(sceneAsset);
						PlaceSeperatorIfNeeded();
						AddScene(_scenesPath[i]);
					}
				}
			}

			void AddedAllScenes()
			{
				//Scenes in build settings
				for (var i = 0; i < _scenesBuildPath.Count; ++i)
				{
					AddScene(_scenesBuildPath[i]);
				}

				//Scenes on Assets/Scenes/
				_isPlaceSeparator = false;
				for (var i = 0; i < _scenesPath.Count; ++i)
				{
					if (_scenesPath[i].Contains("Assets/Scenes"))
					{
						PlaceSeperatorIfNeeded();
						AddScene(_scenesPath[i]);
					}
				}

				//Scenes on Plugins/Plugins/
				//Consider them as demo scenes from plugins
				_isPlaceSeparator = false;
				for (var i = 0; i < _scenesPath.Count; ++i)
				{
					if (_scenesPath[i].Contains("Assets/Plugins/"))
					{
						PlaceSeperatorIfNeeded();
						AddScene(_scenesPath[i], "Plugins demo");
					}
				}

				//All other scenes.
				_isPlaceSeparator = false;
				for (var i = 0; i < _scenesPath.Count; ++i)
				{
					PlaceSeperatorIfNeeded();
					AddScene(_scenesPath[i]);
				}
			}
			
			_scenesPopupDisplay = _toDisplay.ToArray();
		}

		private SceneCollectionObject GetSceneCollection()
		{
			string[] guids = AssetDatabase.FindAssets("t:SceneCollectionObject");
			if (guids.Length == 0)
			{
				Debug.LogWarning("No SceneCollectionObject found in the project. Create one first.");
				return null;
			}

			if (_cachedSceneCollection != null && _lastSceneCollectionGuid == guids[0])
			{
				return _cachedSceneCollection;
			}

			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			_cachedSceneCollection = AssetDatabase.LoadAssetAtPath<SceneCollectionObject>(path);
			_lastSceneCollectionGuid = guids[0];

			return _cachedSceneCollection;
		}

		private void AddScene(string path, string prefix = null, string overrideName = null)
		{
			if (!path.Contains(".unity"))
			{
				path += ".unity";
			}

			if (_toDisplay.Find(data => path == data.Path) != null)
			{
				return;
			}

			if (!string.IsNullOrEmpty(overrideName))
			{
				_name = overrideName;
			}
			else
			{
				if (_showSceneFolder)
				{
					var folderName = Path.GetFileName(Path.GetDirectoryName(path));
					_name = $"{folderName}/{GetSceneName(path)}";
				}
				else
				{
					_name = GetSceneName(path);
				}
			}

			if (!string.IsNullOrEmpty(prefix))
				_name = $"{prefix}/{_name}";

			if (_scenesBuildPath.Contains(path))
			{
				_content = new GUIContent(_name, EditorGUIUtility.Load("BuildSettings.Editor.Small") as Texture, "Open scene");
			}
			else
			{
				_content = new GUIContent(_name, "Open scene");
			}

			_toDisplay.Add(new SceneData()
			{
				Path = path,
				PopupDisplay = _content,
			});

			if (_selectedSceneIndex == -1 && GetSceneName(path) == _activeScene.name)
				_selectedSceneIndex = _usedIds;
			++_usedIds;
		}

		private void PlaceSeperatorIfNeeded()
		{
			if (!_isPlaceSeparator)
			{
				_isPlaceSeparator = true;
				PlaceSeperator();
			}
		}

		private void PlaceSeperator()
		{
			_toDisplay.Add(new SceneData()
			{
				Path = "\0",
				PopupDisplay = new GUIContent("\0"),
			});
			++_usedIds;
		}

		private void HandleSceneOpened(Scene scene, OpenSceneMode mode)
		{
			RefreshScenesList();
		}

		private string GetSceneName(string path)
		{
			path = path.Replace(".unity", "");
			return Path.GetFileName(path);
		}

		private void InitScenesData()
		{
			_toDisplay.Clear();
			_selectedSceneIndex = -1;
			_scenesBuildPath = EditorBuildSettings.scenes.Select(s => s.path).ToList();

			_sceneGuids = AssetDatabase.FindAssets("t:scene", new string[] { "Assets" });
			_scenesPath = _sceneGuids.ToList();
			for (var i = 0; i < _scenesPath.Count; ++i)
			{
				_scenesPath[i] = AssetDatabase.GUIDToAssetPath(_sceneGuids[i]);
			}

			_activeScene = SceneManager.GetActiveScene();
			_usedIds = 0;
		}

		private class SceneData
		{
			public string Path;
			public GUIContent PopupDisplay;
		}
	}
}