using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GitCommands
{
	public static class ReaderWriterLockSlimExtensions
	{
		public static IDisposable EnterReadLockOrFail(this ReaderWriterLockSlim readerWriterLockSlim, int millisecondsTimeout, string currentComponent)
		{
			if (!readerWriterLockSlim.TryEnterReadLock(millisecondsTimeout))
				throw new Exception("Could not acquire read lock for: " + currentComponent);

			return new ReaderWriterLockSlimExtensionsScope(readerWriterLockSlim, false, false);
		}

		public static IDisposable EnterUpgradeableReadLockOrFail(this ReaderWriterLockSlim readerWriterLockSlim, int millisecondsTimeout, string currentComponent)
		{
			if (!readerWriterLockSlim.TryEnterUpgradeableReadLock(millisecondsTimeout))
				throw new Exception("Could not acquire upgradeable read lock for: " + currentComponent);

			return new ReaderWriterLockSlimExtensionsScope(readerWriterLockSlim, false, true);
		}

		public static IDisposable EnterWriteLockOrFail(this ReaderWriterLockSlim readerWriterLockSlim, int millisecondsTimeout, string currentComponent)
		{
			if (!readerWriterLockSlim.TryEnterWriteLock(millisecondsTimeout))
				throw new Exception("Could not acquire write lock for: " + currentComponent);

			return new ReaderWriterLockSlimExtensionsScope(readerWriterLockSlim, true, false);
		}
	}

	public class ReaderWriterLockSlimExtensionsScope : IDisposable
	{
		private ReaderWriterLockSlim _readerWriterLockSlim;
		private bool _write;
		private bool _upgradable;

		internal ReaderWriterLockSlimExtensionsScope(ReaderWriterLockSlim readerWriterLockSlim, bool write, bool upgradable)
		{
			this._readerWriterLockSlim = readerWriterLockSlim;
			this._write = write;
			this._upgradable = upgradable;
		}

		public void Dispose()
		{
			if (_write)
				_readerWriterLockSlim.ExitWriteLock();
			else
				if (_upgradable)
					_readerWriterLockSlim.ExitUpgradeableReadLock();
				else
					_readerWriterLockSlim.ExitReadLock();

			
		}
	}
}
