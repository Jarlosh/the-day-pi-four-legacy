using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[Serializable]
	internal class ToolbarFPSSlider: BaseToolbarElement
	{
		[SerializeField] private int _minFPS = 1;
		[SerializeField] private int _maxFPS = 999;

		private int _selectedFramerate;

		public override string NameInList => "[Slider] FPS";
		public override int SortingGroup => 1;

		public override void Init()
		{
			if (_selectedFramerate == 0)
			{
				_selectedFramerate = 999;
			}
		}

		public ToolbarFPSSlider(int minFPS = 1, int maxFPS = 999): base(200)
		{
			_minFPS = minFPS;
			_maxFPS = maxFPS;
		}

		protected override void OnDrawInList(Rect position)
		{
			position.width = 70.0f;
			EditorGUI.LabelField(position, "Min FPS");

			position.x += position.width + FieldSizeSpace;
			position.width = 50.0f;
			_minFPS = Mathf.RoundToInt(EditorGUI.IntField(position, "", _minFPS));

			position.x += position.width + FieldSizeSpace;
			position.width = 70.0f;
			EditorGUI.LabelField(position, "Max FPS");

			position.x += position.width + FieldSizeSpace;
			position.width = 50.0f;
			_maxFPS = Mathf.RoundToInt(EditorGUI.IntField(position, "", _maxFPS));
		}

		protected override void OnDrawInToolbar()
		{
			EditorGUILayout.LabelField("FPS", GUILayout.Width(30));
			_selectedFramerate = EditorGUILayout.IntSlider("", _selectedFramerate, _minFPS, _maxFPS, GUILayout.Width(WidthInToolbar - 30.0f));
			
			if (EditorApplication.isPlaying && _selectedFramerate != Application.targetFrameRate)
			{
				Application.targetFrameRate = _selectedFramerate;
			}
		}
	}
}