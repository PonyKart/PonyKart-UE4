using System;

namespace Ponykart {
	/// <summary>
	/// A little helper for the IDisposable stuff.
	/// http://msdn.microsoft.com/en-us/library/system.idisposable.aspx
	/// http://msdn.microsoft.com/en-us/library/fs2xkftw.aspx
	/// 
	/// Note that the Dispose() method on LThings are called *after* we clean up stuff from the scene manager
	/// </summary>
	public abstract class LDisposable : IDisposable {
		/// <summary>
		/// Track whether Dispose has been called.
		/// </summary>
		public bool IsDisposed { get; protected set; }

		public LDisposable() {
			IsDisposed = false;
		}

		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method
		/// does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~LDisposable() {
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}

		/// <summary>
		/// Implement IDisposable.
		/// Do not make this method virtual.
		/// A derived class should not be able to override this method.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose(bool disposing) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// </summary>
		/// <param name="disposing">
		/// If disposing equals false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference
		/// other objects. Only unmanaged resources can be disposed.
		/// </param>
		protected virtual void Dispose(bool disposing) {
			IsDisposed = true;
		}
	}
}
