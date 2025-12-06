using UnityEngine;

namespace Game.Shared.Singletons
{
	[DefaultExecutionOrder(-1000)]
	[DisallowMultipleComponent]
	/// Init when scene that contain derived SceneSingletonBehaviour is Awake, before this Instance is null, after scene destoed is null
	public abstract class SceneSingletonBehaviour<T>: MonoBehaviour where T: MonoBehaviour
	{
		public static T Instance { get; private set; }

		protected abstract void OnInit();
		protected abstract void OnRelease();

		protected void Awake()
		{
			// can be initialized only once in scene
			if (Instance == null)
			{
				Instance = GetComponent<T>();
				OnInit();
			}
			else if (Instance != this)
			{
				Destroy(gameObject);
			}
		}

		protected void OnDestroy()
		{
			if (Instance == this)
			{
				OnRelease();
				Instance = null;
			}
		}
	}
}