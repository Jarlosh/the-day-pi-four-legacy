using System;
using UnityEngine;

namespace Game.Client
{
	public class DestructibleWall: MonoBehaviour
	{
		[Header("Settings")]
		[SerializeField] private float _health = 100f;
		[SerializeField] private float _damagePerHit = 50f;
		[SerializeField] private Fracture _fracture;
		[SerializeField] private UnfreezeFragment[] _fragments;

		private bool _destroyed = false;

		[ContextMenu("Collect")]
		private void Collect()
		{
			_fragments = GetComponentsInChildren<UnfreezeFragment>();
		}
		
		public event Action OnDestroyed;

		private void Awake()
		{
			foreach (var fragment in _fragments)
			{
				fragment.onFractureCompleted.AddListener(DestroyWall);
			}
		}

		private void OnDestroy()
		{
			foreach (var fragment in _fragments)
			{
				fragment.onFractureCompleted.RemoveListener(DestroyWall);
			}
		}

		private void DestroyWall()
		{
			if (_destroyed)
			{
				return;
			}
			_destroyed = true;
			OnDestroyed?.Invoke();
			Debug.Log("Destroyed wall");
		}
	}
}