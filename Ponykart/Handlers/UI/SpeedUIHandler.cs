#if DEBUG
using Miyagi.UI.Controls;
using Mogre;
using MOIS;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Players;
using Ponykart.UI;

namespace Ponykart.Handlers {
	[Handler(HandlerScope.Global)]
	public class SpeedUIHandler {
		Label label;
        public SpeedUIHandler() {
			var gui = LKernel.GetG<UIMain>().GetGUI("level debug gui");

			label = gui.GetControl<Label>("speed label");

			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);
			LKernel.GetG<InputMain>().OnKeyboardPress_Anything += new LymphInputEvent<KeyEvent>(OnKeyboardPress_Anything);
		}

		void OnKeyboardPress_Anything(KeyEvent eventArgs) {
			if (!LKernel.GetG<InputSwallowerManager>().IsSwallowed()) {
				if (eventArgs.key == KeyCode.KC_MINUS) {
					label.Visible = !label.Visible;

					var button = LKernel.GetG<UIMain>().GuiManager.GetControl<Button>("show/hide commands button");
					button.Visible = !button.Visible;
				}
			}
		}

		float elapsed;
		bool FrameStarted(FrameEvent evt) {
			if (elapsed >= 0.1f) {
				elapsed = 0;

				var mainPlayer = LKernel.GetG<PlayerManager>().MainPlayer;

				if (label.Visible && LKernel.GetG<LevelManager>().IsValidLevel && mainPlayer != null && mainPlayer.Kart != null && !mainPlayer.Kart.Body.IsDisposed) {
					Kart kart = mainPlayer.Kart;
                    
                    Mogre.Vector3 axisA = new Mogre.Vector3(0,1,0);
                    Quaternion quat1;
                    quat1 = kart.ActualOrientation.XAxis.GetRotationTo(axisA);
                    
                    
                    label.Text = string.Format(
@"Speed: {0}
Turn angle: {1}, {2}, {3}, {4}
Linear velocity: {5}, {6}
Wheel friction: {7}, {8}
Skid info: {9}, {10}
Brake? {11}
AccelMultiplier: {12}
IntOr: {13}
Gravity: {14}
KartDriftState: {15} , WheelDriftState: {16}
Player Pos: {17} X, {18} Y, {19} Z
Quat: {20}
Quat W: {21}",
                        kart.VehicleSpeed,
                        Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(0)), Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(1)),
                        Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(2)), Math.RadiansToDegrees(kart.Vehicle.GetSteeringValue(3)),
                        kart.Body.LinearVelocity.Length, kart.Body.LinearVelocity,
                        kart.Vehicle.GetWheelInfo(0).FrictionSlip, kart.Vehicle.GetWheelInfo(2).FrictionSlip,
                        kart.Vehicle.GetWheelInfo(0).SkidInfo, kart.Vehicle.GetWheelInfo(2).SkidInfo,
                        kart.WheelFL.IsBrakeOn,
                        kart.Acceleration,
                        kart.InterpolatedOrientation,
                        kart.Body.Gravity,
                        kart.DriftState, kart.WheelFL.DriftState,
                        mainPlayer.NodePosition.x, mainPlayer.NodePosition.y, mainPlayer.NodePosition.z,
                        quat1,
                        quat1.w*180/Math.PI);
				}
			}
			elapsed += evt.timeSinceLastFrame;
			return true;
		}
	}
}
#endif