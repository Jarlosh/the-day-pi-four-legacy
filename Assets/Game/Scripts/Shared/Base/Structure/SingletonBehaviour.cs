using UnityEngine;

namespace Game.Shared.Singletons
{
	[DefaultExecutionOrder(-1000)]
	[DisallowMultipleComponent]
	/// Init when scene that contain derived SingletonBehaviour is Awake, before this Instance is null
	public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
	{
		public static T Instance { get; private set; }

		protected abstract void OnInit();
		protected abstract void OnRelease();

		protected void Awake()
		{
			// can be initialized only once
			if (Instance == null)
			{
				Instance = GetComponent<T>();
				gameObject.transform.parent = null;
				DontDestroyOnLoad(gameObject);

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