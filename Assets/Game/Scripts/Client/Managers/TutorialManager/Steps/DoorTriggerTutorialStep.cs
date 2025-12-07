using Cysharp.Threading.Tasks;
using DG.Tweening;
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
		[SerializeField] private float _doorCloseTime = 1f; // Скорость закрытия двери
		[SerializeField] private LayerMask _playerLayer; // Слой игрока
		
		private bool _doorClosed = false;
		
		protected override void OnInitialize()
		{
			base.OnInitialize();
			
			if (_doorObject != null)
			{
				_doorObject.localPosition = _doorOpenPosition;
			}
			
			if (_triggerZone != null)
			{
				_triggerZone.isTrigger = true;
			}
		}

		protected override void OnComplete()
		{
			CloseDoor().Forget();
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

			_doorObject.transform.DOLocalMove(_doorClosedPosition, _doorCloseTime);
			await UniTask.WaitForSeconds(_doorCloseTime);
			_doorObject.localPosition = _doorClosedPosition;
		}
		
		public void OnPlayerEnteredTrigger()
		{
			_triggerZone.gameObject.SetActive(false);
			CloseDoor().Forget();
		}
	}
}