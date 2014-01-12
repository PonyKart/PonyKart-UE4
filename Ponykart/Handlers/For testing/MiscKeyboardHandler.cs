using System.Diagnostics;
using System.IO;
using BulletSharp;
using Mogre;
using MOIS;
using Ponykart.Actors;
using Ponykart.Core;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;
using Ponykart.Properties;
using Ponykart.Sound;
using Ponykart.Stuff;
using Vector3 = Mogre.Vector3;

namespace Ponykart.Handlers {
	/// <summary>
	/// This class is hooked up to the keyboard events and does miscellaneous things depending on what keys are pressed.
	/// It's mostly for debugging functions
	/// </summary>
	[Handler(HandlerScope.Global)]
	public class MiscKeyboardHandler {

		public MiscKeyboardHandler() {
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += OnKeyboardPress_Anything;
		}

		void OnKeyboardPress_Anything(KeyEvent ke) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed()) // if the input is swallowed, do nothing
				return;

			switch (ke.key) {
				case KeyCode.KC_MINUS: // the -_ key
					LKernel.GetG<DebugOverlayManager>().ToggleDebugOverlay();
					break;
#if DEBUG
				case KeyCode.KC_X:
					MogreDebugDrawer.Singleton.Clear();
					break;
				case KeyCode.KC_I:
					PhysicsMain.DrawLines = !PhysicsMain.DrawLines;
					break;
				case KeyCode.KC_M:
					if (LKernel.GetG<SoundMain>().IsMusicEnabled)
						LKernel.GetG<SoundMain>().DisableMusic();
					else
						LKernel.GetG<SoundMain>().EnableMusic();
					break;
				case KeyCode.KC_P:
					if (LKernel.GetG<SoundMain>().IsSoundEnabled)
						LKernel.GetG<SoundMain>().DisableSounds();
					else
						LKernel.GetG<SoundMain>().EnableSounds();
					break;
				case KeyCode.KC_N:
					LKernel.GetG<SoundMain>().Play2D("Sweet Apple Acres 128bpm.ogg", true);
					break;
				case KeyCode.KC_U:
					LKernel.GetG<PlayerManager>().MainPlayer.Body.LinearVelocity += new Vector3(0, 4, 0);
					break;
				case KeyCode.KC_R:
					LKernel.GetG<LThingHelperManager>().CreateRotater(LKernel.GetG<PlayerManager>().MainPlayer.Kart, 0.25f, new Degree(90), RotaterAxisMode.RelativeY);
					break;
				case KeyCode.KC_F:
					LKernel.GetG<PlayerManager>().MainPlayer.Body.LinearVelocity *= 2f;
					break;
				case KeyCode.KC_F1:
					LKernel.GetG<CameraManager>().SwitchCurrentCamera("PlayerCamera");
					break;
				case KeyCode.KC_F2:
					LKernel.GetG<CameraManager>().SwitchCurrentCamera("FreeCamera");
					break;
				case KeyCode.KC_F3:
					LKernel.GetG<CameraManager>().SwitchCurrentCamera("KnightyCamera");
					break;
				case KeyCode.KC_F4:
					LKernel.GetG<CameraManager>().SwitchCurrentCamera("ChaseCamera");
					break;
				case KeyCode.KC_F5:
					LKernel.GetG<CameraManager>().SwitchCurrentCamera("AttachCamera");
					break;
				case KeyCode.KC_F6:
					LKernel.GetG<CameraManager>().SwitchCurrentCamera("SmoothFreeCamera");
					break;
				case KeyCode.KC_F11:
					uint width, height;
					Options.GetWindowDimensions(out width, out height);
					LKernel.GetG<RenderWindow>().SetFullscreen(!LKernel.GetG<RenderWindow>().IsFullScreen, width, height);
					break;
				case KeyCode.KC_Q:
					LKernel.GetG<PlayerManager>().MainPlayer.Body.ForceActivationState(ActivationState.WantsDeactivation);
					break;
				case KeyCode.KC_G:
					PhysicsMain.SlowMo = !PhysicsMain.SlowMo;
					break;
				case KeyCode.KC_C:
					if (!File.Exists("syncmedia.cmd"))
						break;
					using (var proc = Process.Start("syncmedia.cmd")) {
						proc.WaitForExit();
					}
                    #endif
					//LKernel.GetG<LuaMain>().Restart();
					LKernel.Get<WheelFactory>().ReadWheelsFromFiles();
					LKernel.Get<PhysicsMaterialFactory>().ReadMaterialsFromFiles();
					LKernel.GetG<CollisionShapeManager>().Clear();

					ResourceGroupManager.Singleton.InitialiseResourceGroup("General");
					if (ResourceGroupManager.Singleton.ResourceGroupExists(LKernel.GetG<LevelManager>().CurrentLevel.Name))
						ResourceGroupManager.Singleton.InitialiseResourceGroup(LKernel.GetG<LevelManager>().CurrentLevel.Name);
					//MaterialManager.Singleton.ReloadAll(false);
					//MeshManager.Singleton.ReloadAll(false);

					Settings.Default.Reload();
					break;
				// prnt scrn
				case KeyCode.KC_SYSRQ:
					LKernel.GetG<RenderWindow>().WriteContentsToTimestampedFile("Ponykart_", ".png");
					break;
			}
		}
	}
}
