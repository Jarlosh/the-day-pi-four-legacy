using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
	public class DoorTriggerTutorialStep : TutorialStep
	{
		[Header("Door Trigger Settings")]
		[SerializeField] private Collider _triggerZone; // Триггер, в который должен войти игрок
		[SerializeField] private Transform _doorObject; // Объект двери, который поднимается из-под земли
		[SerializeField] private Vector3 _doorClosedPosition; // Позиция, когда дверь закрыта
		[SerializeField] private Vector3 _doorOpenPosition; // Позиция, когда дверь открыта (под землей)
		[SerializeField] private float _doorCloseSpeed = 5f; // Скорость закрытия двери
		[SerializeField] private LayerMask _playerLayer; // Слой игрока
		
		private bool _playerInTrigger = false;
		private bool _doorClosed = false;
		
		protected override void OnInitialize()
		{
			base.OnInitialize();
			
			if (_doorObject != null)
			{
				_doorObject.position = _doorOpenPosition;
			}
			
			if (_triggerZone != null)
			{
				_triggerZone.isTrigger = true;
			}
		}
		
		private void Update()
		{
			if (!_isInitialized || _isCompleted)
				return;
			
			if (_doorClosed && !_isCompleted)
			{
				Complete();
			}
		}
		
		private async UniTaskVoid CloseDoor()
		{
			if (_doorObject == null || _doorClosed)
				return;
			
			_doorClosed = true;
			
			while (Vector3.Distance(_doorObject.position, _doorClosedPosition) > 0.1f)
			{
				_doorObject.position = Vector3.MoveTowards(
					_doorObject.position,
					_doorClosedPosition,
					_doorCloseSpeed * Time.deltaTime
				);
				
				await UniTask.Yield();
			}
			
			_doorObject.position = _doorClosedPosition;
		}
		
		public void OnPlayerEnteredTrigger()
		{
			_playerInTrigger = true;
			CloseDoor().Forget();
		}
	}
}