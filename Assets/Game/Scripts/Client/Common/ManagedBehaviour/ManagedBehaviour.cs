using Game.Client.App;
using Game.Core;

namespace Game.Client
{
	public class ManagedBehaviour: ManagedBehaviourBase
	{
		private ITimeService _timeService;

		public virtual bool UpdateWhenPaused => false;

		protected virtual void ManagedInitialize()
		{
		}

		public virtual void ManagedUpdate()
		{
		}

		public virtual void ManagedFixedUpdate()
		{
		}

		public virtual void ManagedLateUpdate()
		{
		}

		private bool CanUpdate()
		{
			return UpdateWhenPaused || !_timeService.IsPaused;
		}

		protected sealed override void Awake()
		{
			_timeService = ServiceLocator.Get<ITimeService>();

			ManagedInitialize();
		}

		public sealed override void Update()
		{
			if (CanUpdate())
			{
				ManagedUpdate();
			}
		}

		public sealed override void FixedUpdate()
		{
			if (CanUpdate())
			{
				ManagedFixedUpdate();
			}
		}

		public sealed override void LateUpdate()
		{
			if (CanUpdate())
			{
				ManagedLateUpdate();
			}
		}
	}
}