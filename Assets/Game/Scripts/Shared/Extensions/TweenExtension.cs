using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;

namespace Game.Shared
{
	public static class TweenExtension
	{
		public static async UniTask AsyncWaitForCompletion(this Tween tween, CancellationToken token)
		{
			if (!tween.active)
			{
				if (Debugger.logPriority > 0)
				{
					Debugger.LogInvalidTween(tween);
				}

				return;
			}

			while (tween.active && !tween.IsComplete())
			{
				await UniTask.Yield(token);
			}
		}

		public static void Cancel(this Tween tween, bool complete = false)
		{
			if (tween is {active: true})
			{
				tween.Kill(complete);
			}
		}
	}
}