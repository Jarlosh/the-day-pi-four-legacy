using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Editor
{
	internal class CustomToolbarSettingProvider: SettingsProvider
	{
		private class Styles
		{
			public static readonly GUIContent MinFPS = new GUIContent("Minimum FPS");
			public static readonly GUIContent MaxFPS = new GUIContent("Maximum FPS");
			public static readonly GUIContent LimitFPS = new GUIContent("Limit FPS");
		}
		
		private const string SETTING_PATH = "Assets/Editor/Setting/CustomToolbarSetting.asset";
		
		private SerializedObject _toolbarSetting;
		private CustomToolbarSetting _setting;

		private Vector2 _scrollPos;
		private ReorderableList _elementsList;

		public CustomToolbarSettingProvider(string path, SettingsScope scopes = SettingsScope.User): base(
			path, scopes)
		{

		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			// base.OnActivate(searchContext, rootElement);
			_toolbarSetting = CustomToolbarSetting.GetSerializedSetting();
			_setting = (_toolbarSetting.targetObject as CustomToolbarSetting);
		}

		public static bool IsSettingAvailable()
		{
#if UNITY_2020_3_OR_NEWER
			return ScriptableSingleton<CustomToolbarSetting>.instance != null;
#else
			CustomToolbarSetting.GetOrCreateSetting();
			return File.Exists(SETTING_PATH);;
#endif
		}

		public override void OnGUI(string searchContext)
		{
			base.OnGUI(searchContext);

			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

			_elementsList = _elementsList ?? CustomToolbarReordableList.Create(_setting.Elements, OnMenuItemAdd);
			_elementsList.DoLayoutList();

			EditorGUILayout.EndScrollView();

			_toolbarSetting.ApplyModifiedProperties();
			if (GUI.changed)
			{
				EditorUtility.SetDirty(_toolbarSetting.targetObject);
				ToolbarExtender.OnGUI();
#if UNITY_2020_3_OR_NEWER
				_setting.Save();
#endif
			}
		}

		private void OnMenuItemAdd(object target)
		{
			_setting.Elements.Add(target as BaseToolbarElement);
			_toolbarSetting.ApplyModifiedProperties();
#if UNITY_2020_3_OR_NEWER
			_setting.Save();
#endif
		}

		[SettingsProvider]
		public static SettingsProvider CreateCustomToolbarSettingProvider()
		{
			if (IsSettingAvailable())
			{
				CustomToolbarSettingProvider provider = new CustomToolbarSettingProvider("Project/Custom Toolbar", SettingsScope.Project)
				{
					keywords = GetSearchKeywordsFromGUIContentProperties<Styles>()
				};

				return provider;
			}

			return null;
		}
	}
}