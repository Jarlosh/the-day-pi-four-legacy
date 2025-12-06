using UnityEngine;

namespace Game.Shared.Singletons
{
	[DefaultExecutionOrder(-1000)]
	[DisallowMultipleComponent]
	/// Init when a game is loaded, and will not destroyed anywhere
	/// To use this implementation need in derived create static function with attribute [RuntimeInitializeOnLoadMethod] and call InstantiateAutoSingleton from it
	public abstract class AutoSingletonBehaviour<T>: MonoBehaviour where T: MonoBehaviour
	{
		public static T Instance { get; private set; }

		protected abstract void OnInit();
		protected abstract void OnRelease();

		protected AutoSingletonBehaviour()
		{
		}

		protected static void InstantiateAutoSingleton()
		{
			if (Instance == null)
			{
				GameObject gameObject = Instantiate(new GameObject());
				gameObject.AddComponent<T>();
			}
		}
		
		protected static void InstantiateAutoSingleton(GameObject prefab)
		{
			if (Instance == null)
			{
				if (prefab != null)
				{
					Instantiate(prefab);
				}
				else
				{
					GameObject gameObject = Instantiate(new GameObject());
					gameObject.AddComponent<T>();
				}
			}
		}

		protected void Awake()
		{
			// can be initialized only once
			if (Instance == null)
			{
				Instance = gameObject.GetComponent<T>();
				gameObject.transform.parent = null;
				gameObject.name = GetType().Name;
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