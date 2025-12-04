using System;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
	[Serializable]
	internal class ToolbarTimeslider: BaseToolbarElement
	{
		[SerializeField] private float _minTime = 1;
		[SerializeField] private float _maxTime = 120;

		private bool _defaultSetOnce;
		private float _selectedTimeScale;

		public override string NameInList => "[Slider] Timescale";
		public override int SortingGroup => 1;

		public override void Init()
		{
			if (!_defaultSetOnce)
			{
				_defaultSetOnce = true;
				_selectedTimeScale = 1;
			}
		}

		public ToolbarTimeslider(float minTime = 0f, float maxTime = 10.0f): base(200)
		{
			_minTime = minTime;
			_maxTime = maxTime;
		}

		protected override void OnDrawInList(Rect position)
		{
			position.width = 70.0f;
			EditorGUI.LabelField(position, "Min Time");

			position.x += position.width + FieldSizeSpace;
			position.width = 50.0f;
			_minTime = EditorGUI.FloatField(position, "", _minTime);

			position.x += position.width + FieldSizeSpace;
			position.width = 70.0f;
			EditorGUI.LabelField(position, "Max Time");

			position.x += position.width + FieldSizeSpace;
			position.width = 50.0f;
			_maxTime = EditorGUI.FloatField(position, "", _maxTime);
		}

		protected override void OnDrawInToolbar()
		{
			EditorGUILayout.LabelField("Time", GUILayout.Width(30));
			_selectedTimeScale = EditorGUILayout.Slider("", _selectedTimeScale, _minTime, _maxTime, GUILayout.Width(WidthInToolbar - 30.0f));
			
			if (EditorApplication.isPlaying && _selectedTimeScale != Time.timeScale)
			{
				Time.timeScale = _selectedTimeScale;
			}
		}
	}
}