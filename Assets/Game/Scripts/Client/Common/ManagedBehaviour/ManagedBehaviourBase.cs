using UnityEngine;

namespace Game.Client
{
	public class ManagedBehaviourBase: MonoBehaviour
	{
		protected virtual void Awake() { }
		public virtual void Update() { }
		public virtual void FixedUpdate() { }
		public virtual void LateUpdate() { }
	}
}