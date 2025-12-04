using System;

namespace Game.Shared
{
	public interface ICancelable : IDisposable
	{
		bool IsDisposed { get; }
	}
}