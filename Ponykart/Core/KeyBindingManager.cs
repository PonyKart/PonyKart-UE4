using System;
using System.Collections.Generic;
using MOIS;

namespace Ponykart.Core {
	/// <summary>
	/// Our key commands - these are for things that need to be polled. If you want to just respond to events, use the ones in InputMain.
	/// </summary>
	public enum LKey {
		Accelerate,
		TurnLeft,
		TurnRight,
		Drift,
		Reverse,
		SteeringAxis,
		AccelerateAxis,
		BrakeAxis,
        Item
	}

	/// <summary>
	/// This class provides an interface between game commands (accelerate, etc) and key presses (WASD, etc).
	/// This way we can change which keys do things at runtime
	/// </summary>
	public class KeyBindingManager {
		/// <summary>
		/// The dictionary that converts our key commands into MOIS keys
		/// </summary>
		private IDictionary<LKey, KeyCode> LKeysDict;
		private IDictionary<KeyCode, LKey> MOISKeysDict;
		private IDictionary<ControllerButtons, LKey> LButtonsDict;
		private IDictionary<ControllerAxis, LKey> LAxisDict;

		public IDictionary<LKey, Action> PressEventsDict { get; private set; }
		public IDictionary<LKey, Action> ReleaseEventsDict { get; private set; }
		public IDictionary<LKey, Action> AxisEvents { get; private set; }

		public KeyBindingManager() {
			Launch.Log("[Loading] Creating KeyBindingManager...");
			LKeysDict = new Dictionary<LKey, KeyCode>();
			MOISKeysDict = new Dictionary<KeyCode, LKey>();
			PressEventsDict = new Dictionary<LKey, Action>();
			ReleaseEventsDict = new Dictionary<LKey, Action>();
			AxisEvents = new Dictionary<LKey, Action>( );
			LButtonsDict = new Dictionary<ControllerButtons, LKey>( );
			LAxisDict = new Dictionary<ControllerAxis, LKey>( );

			SetupInitialBindings();

			var input = LKernel.GetG<InputMain>();
			input.OnKeyboardPress_Anything += new LymphInputEvent<KeyEvent>(OnKeyboardPressAnything);
			input.OnKeyboardRelease_Anything += new LymphInputEvent<KeyEvent>(OnKeyboardReleaseAnything);
			input.OnLeftXAxisMoved += new AxisMovedEventHandler( input_OnLeftXAxisMoved );

			if (Options.GetBool("Twh")) {
				input.OnMousePress_Anything += new LymphInputEvent<MouseEvent, MouseButtonID>(OnMousePress_Anything);
				input.OnMouseRelease_Anything += new LymphInputEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Anything);
			}
		}

		void input_OnLeftXAxisMoved( object sender, ControllerAxisArgument e )
		{
			if ( LKernel.GetG<InputSwallowerManager>( ).IsSwallowed( ) )
				return;

			Invoke( AxisEvents[LAxisDict[e.Axis]] );
		}

		// temporary, so twh can control the camera better when filming
		void OnMousePress_Anything(MouseEvent e, MouseButtonID id) {
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			switch (id) {
				case MouseButtonID.MB_Left:
					Invoke(PressEventsDict[LKey.Accelerate]); break;
				case MouseButtonID.MB_Button3:
					Invoke(PressEventsDict[LKey.TurnLeft]); break;
				case MouseButtonID.MB_Button4:
					Invoke(PressEventsDict[LKey.TurnRight]); break;
				case MouseButtonID.MB_Middle:
					Invoke(PressEventsDict[LKey.Drift]); break;
			}
		}
		void OnMouseRelease_Anything(MouseEvent e, MouseButtonID id) {
			switch (id) {
				case MouseButtonID.MB_Left:
					Invoke(ReleaseEventsDict[LKey.Accelerate]); break;
				case MouseButtonID.MB_Button3:
					Invoke(ReleaseEventsDict[LKey.TurnLeft]); break;
				case MouseButtonID.MB_Button4:
					Invoke(ReleaseEventsDict[LKey.TurnRight]); break;
				case MouseButtonID.MB_Middle:
					Invoke(ReleaseEventsDict[LKey.Drift]); break;
			}
		}

		/// <summary>
		/// Set up some initial key bindings
		/// TODO read these from a file
		/// </summary>
		void SetupInitialBindings() {
			LKeysDict[LKey.Accelerate] = KeyCode.KC_W;
			LKeysDict[LKey.TurnLeft] = KeyCode.KC_A;
			LKeysDict[LKey.TurnRight] = KeyCode.KC_D;
			LKeysDict[LKey.Drift] = KeyCode.KC_SPACE;
			LKeysDict[LKey.Reverse] = KeyCode.KC_S;
            LKeysDict[LKey.Item] = KeyCode.KC_LSHIFT;
			MOISKeysDict[KeyCode.KC_W] = LKey.Accelerate;
			MOISKeysDict[KeyCode.KC_A] = LKey.TurnLeft;
			MOISKeysDict[KeyCode.KC_D] = LKey.TurnRight;
			MOISKeysDict[KeyCode.KC_SPACE] = LKey.Drift;
			MOISKeysDict[KeyCode.KC_S] = LKey.Reverse;
            MOISKeysDict[KeyCode.KC_LSHIFT] = LKey.Item;
			LButtonsDict[ControllerButtons.A] = LKey.Drift;		
			LAxisDict[ControllerAxis.LeftX] = LKey.SteeringAxis;

			PressEventsDict.Add(LKey.Accelerate, null);
			PressEventsDict.Add(LKey.TurnLeft, null);
			PressEventsDict.Add(LKey.TurnRight, null);
			PressEventsDict.Add(LKey.Drift, null);
            PressEventsDict.Add(LKey.Reverse, null);
            PressEventsDict.Add(LKey.Item, null);

			ReleaseEventsDict.Add(LKey.Accelerate, null);
			ReleaseEventsDict.Add(LKey.TurnLeft, null);
			ReleaseEventsDict.Add(LKey.TurnRight, null);
			ReleaseEventsDict.Add(LKey.Drift, null);
            ReleaseEventsDict.Add(LKey.Reverse, null);
            ReleaseEventsDict.Add(LKey.Item, null);
		}

		/// <summary>
		/// 
		/// </summary>
		void OnKeyboardPressAnything(KeyEvent ke) {
			// don't do anything if it's swallowed
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			LKey key;
			if (MOISKeysDict.TryGetValue(ke.key, out key))
				Invoke(PressEventsDict[key]);
		}

		/// <summary>
		/// 
		/// </summary>
		void OnKeyboardReleaseAnything(KeyEvent ke) {
			// don't do anything if it's swallowed
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return;

			LKey key;
			if (MOISKeysDict.TryGetValue(ke.key, out key))
				Invoke(ReleaseEventsDict[key]);
		}

		/// <summary>
		/// Is the associated key pressed or not?
		/// </summary>
		/// <returns>Whether the key is pressed or not, or false if the input is currently swallowed.</returns>
		public bool IsKeyPressed(LKey key) {
			// don't do anything if it's swallowed
			if (LKernel.GetG<InputSwallowerManager>().IsSwallowed())
				return false;

			return LKernel.GetG<InputMain>().InputKeyboard.IsKeyDown(LKeysDict[key]);
		}

		/// <summary>
		/// Changes the binding of "command" to "newKey"
		/// </summary>
		/// <param name="command"></param>
		/// <param name="newKey"></param>
		public void ChangeBinding(LKey command, KeyCode newKey) {
			LKeysDict[command] = newKey;
			MOISKeysDict[newKey] = command;
		}

		/// <summary>
		/// helper
		/// </summary>
		public void Invoke(Action e) {
			if (e != null)
				e.Invoke();
		}
	}
}
