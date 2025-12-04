using System;

namespace Game.Client.App
{
	// Number is priority. More number -> More priority.
	// If you pause game with priority 2, then pausing with priority 1 -> PauseType = 2.
	public enum PauseType
	{
		Unknown = 0,
		Meta = 1,
		Upgrade = 2,
		Menu = 3
	}

	public interface ITimeService
	{
		event Action<bool> EventPaused;

		float DeltaTime { get; }
		float TimeScaleCurrent { get; }
		float TimeScaleTarget { get; }
		float TimeScalePaused { get; }
		float TimeScaleActive { get; }
		bool IsPaused { get; }
		PauseType PauseType { get; }

		void Pause(object source, PauseType pauseType = PauseType.Unknown);

		void Resume(object source);

		void SetTimeScale(float timeScale);

		void SubscribePause(Action<bool> pauseAction);

		void UnsubscribePause(Action<bool> pauseAction);
	}
}