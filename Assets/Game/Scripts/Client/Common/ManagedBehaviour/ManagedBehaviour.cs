using System;
using Game.Client.App;
using Game.Core;

namespace Game.Client
{
	public class ManagedBehaviour: ManagedBehaviourBase
	{
		private ITimeService _timeService;

		private void Start()
		{
			_timeService = ServiceLocator.Get<ITimeService>();
		}

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
			if (_timeService == null)
			{
				_timeService = ServiceLocator.Get<ITimeService>();
			}

			return UpdateWhenPaused || _timeService is { IsPaused: false };
		}
		

		protected sealed override void Awake()
		{
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