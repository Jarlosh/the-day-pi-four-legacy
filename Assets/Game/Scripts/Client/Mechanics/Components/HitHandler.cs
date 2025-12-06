using UnityEngine;

namespace Game.Client
{
	public interface IHitHandler
	{
		void TakeDamage(float value);
	}
	
	public class HitHandler: MonoBehaviour, IHitHandler
	{
		[SerializeField] private Health _health;
		
		public void TakeDamage(float value)
		{
			_health.TakeDamage(value);
		}
	}
}