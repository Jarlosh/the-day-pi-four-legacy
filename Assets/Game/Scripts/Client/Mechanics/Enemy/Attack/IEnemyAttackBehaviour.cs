using UnityEngine;

namespace Game.Client
{
	public interface IEnemyAttackBehaviour
	{
		void Initialize(Enemy enemy, Transform target);
		void StartAttack();
		void StopAttack();
		void UpdateAttack();
	}
	
}