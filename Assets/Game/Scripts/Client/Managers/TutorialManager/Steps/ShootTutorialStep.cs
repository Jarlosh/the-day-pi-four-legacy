using UnityEngine;

namespace Game.Client
{
	public class ShootTutorialStep : TutorialStep
	{
		[Header("Shoot Tutorial")]
		[SerializeField] private DestructibleWall _destructibleWall;
		[SerializeField] private int _requiredShots = 1;
        
		private int _shotsCount;
        
		protected override void OnInitialize()
		{
			base.OnInitialize();
            
			_shotsCount = 0;
            
			EventBus.Instance.Subscribe<ShootEvent>(OnShoot);
            
			if (_destructibleWall != null)
			{
				_destructibleWall.gameObject.SetActive(true);
				_destructibleWall.OnDestroyed += OnWallDestroyed;
			}
		}
        
		protected override void OnComplete()
		{
			base.OnComplete();
            
			EventBus.Instance.Unsubscribe<ShootEvent>(OnShoot);
            
			if (_destructibleWall != null)
			{
				_destructibleWall.OnDestroyed -= OnWallDestroyed;
			}
		}
        
		private void OnShoot(ShootEvent _)
		{
			_shotsCount++;

			if (_shotsCount >= _requiredShots)
			{
				Complete();
			}
		}
        
		private void OnWallDestroyed()
		{
			if (_shotsCount >= _requiredShots)
			{
				Complete();
			}
		}
	}
}