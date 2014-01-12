using Mogre;
using Ponykart.Core;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Level, LevelType.Race)]
	public class CompositorHandler : ILevelHandler {
		Viewport v;

		public CompositorHandler() {
			if (Options.ModelDetail == ModelDetailOption.High) {
				v = LKernel.GetG<Viewport>();
				CompositorManager.Singleton.AddCompositor(v, "Bloom");
				CompositorManager.Singleton.SetCompositorEnabled(v, "Bloom", true);

				// In order to get compositors to work correctly, we have to disable and then re-enable them with each camera switch.
				// This destroys all of the render targets and recreates them with them all using the new camera.
				// There was a patch submitted to ogre 1.7 that just updated the render targets automatically (without destroying them)
				// on a camera change, so mogre is probably just a bit out of date
				// http://www.ogre3d.org/forums/viewtopic.php?f=4&t=53330
				
				CameraManager.OnPreCameraSwitch += new CameraEvent(OnPreCameraSwitch);
				CameraManager.OnPostCameraSwitch += new CameraEvent(OnPostCameraSwitch);
			}
		}


		/// <summary>
		/// Before we switch cameras we need to disable all compositors
		/// </summary>
		void OnPreCameraSwitch(LCamera cam) {
			CompositorManager.Singleton.SetCompositorEnabled(v, "Bloom", false);
		}

		/// <summary>
		/// And afterwards we re-enable them again
		/// </summary>
		void OnPostCameraSwitch(LCamera cam) {
			CompositorManager.Singleton.SetCompositorEnabled(v, "Bloom", true);
		}



		public void Detach() {
			if (Options.ModelDetail == ModelDetailOption.High) {
				CompositorManager.Singleton.RemoveCompositor(v, "Bloom");

				CameraManager.OnPreCameraSwitch -= new CameraEvent(OnPreCameraSwitch);
				CameraManager.OnPostCameraSwitch -= new CameraEvent(OnPostCameraSwitch);
			}
		}
	}
}
