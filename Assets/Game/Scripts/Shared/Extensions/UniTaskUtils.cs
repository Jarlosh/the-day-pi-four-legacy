using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Shared
{
	public static class UniTaskUtils
	{
		public static UniTask DelaySeconds(float seconds, CancellationToken cancellationToken = default (CancellationToken))
		{
			return UniTask.Delay(TimeSpan.FromSeconds((double) seconds), cancellationToken: cancellationToken);
		}
	}
}