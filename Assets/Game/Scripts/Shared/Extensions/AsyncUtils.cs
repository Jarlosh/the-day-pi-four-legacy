using System.Threading;

namespace Game.Shared
{
	public static class AsyncUtils
	{
		public static void CancelDisposeNull(ref CancellationTokenSource tokenSource)
		{
			tokenSource.Cancel();
			tokenSource.Dispose();
			tokenSource = null;
		}
		
		public static bool TryCancelDisposeNull(ref CancellationTokenSource tokenSource)
		{
			bool sourceNull = tokenSource == null;

			if (!sourceNull)
			{
				tokenSource.Cancel();
				tokenSource.Dispose();
				tokenSource = null;
			}

			return !sourceNull;
		}
	}
}