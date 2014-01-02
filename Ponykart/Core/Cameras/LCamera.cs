using Mogre;

namespace Ponykart.Core {
	/// <summary>
	/// All camera classes should subclass from this.
	/// </summary>
	public abstract class LCamera : LDisposable {
		/// <summary>
		/// The Mogre camera we're manipulating
		/// </summary>
		public Camera Camera { get; protected set; }
		/// <summary>
		/// The scene node the camera is attached to
		/// </summary>
		public SceneNode CameraNode { get; protected set; }
		/// <summary>
		/// The name of this camera
		/// </summary>
		public string Name { get; protected set; }
		/// <summary>
		/// Is this camera active or not?
		/// </summary>
		protected bool IsActive;

		/// <summary>
		/// Hook up to the frame started event
		/// </summary>
		public LCamera(string name) {
			Name = name;
		}

		/// <summary>
		/// shorthand
		/// </summary>
		public void Register() {
			LKernel.GetG<CameraManager>().RegisterCamera(this);
		}

		/// <summary>
		/// Make sure you register the camera before calling this!
		/// </summary>
		public void MakeActiveCamera() {
			LKernel.GetG<CameraManager>().SwitchCurrentCamera(this);
		}

		/// <summary>
		/// Update the camera every frame! 
		/// </summary>
		protected virtual bool UpdateCamera(FrameEvent evt) {
			return true;
		}

		/// <summary>
		/// Is ran when we switch cameras and this one becomes the active camera.
		/// </summary>
		public virtual void OnSwitchToActive(LCamera oldCamera) {
			IsActive = true;
			LKernel.GetG<Root>().FrameStarted += UpdateCamera;
		}

		/// <summary>
		/// Is ran when we switch cameras and this one was previously the active camera but isn't any more.
		/// </summary>
		public virtual void OnSwitchToInactive(LCamera newCamera) {
			IsActive = false;
			LKernel.GetG<Root>().FrameStarted -= UpdateCamera;
		}

		/// <summary>
		/// Unhook from the frame started event
		/// </summary>
		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			if (disposing)
				LKernel.GetG<Root>().FrameStarted -= UpdateCamera;

			base.Dispose(disposing);
		}
	}
}
