// uncomment if you want all input to be printed
//#define PRINTINPUT

using System;
using Mogre;
using MOIS;
using Ponykart.Levels;
using Ponykart.Core;
using Type = MOIS.Type;

namespace Ponykart {
	/// <summary>
	/// Class that handles all of the input. This acts as a "layer" between the input library and the rest of the program.
	/// This makes it easier to change libraries and stuff, since you'd only need to change this class instead of everything
	/// that uses input.
	/// 
	/// This class does not do anything with the input besides fire off more events.
	/// 
	/// Other classes (mostly handlers) should only use events fired off from this class and not ones fired off from the
	/// input library.
	/// </summary>
	public class InputMain {
		public InputManager InputManager { get; private set; }
		public Keyboard InputKeyboard { get; private set; }
		public Mouse InputMouse { get; private set; }
		public ControllerManager InputController { get; private set; }

		public InputMain() {
			Launch.Log("[Loading] Initialising MOIS input system");

			ParamList pl = new ParamList();
			IntPtr windowHnd;
			LKernel.GetG<RenderWindow>().GetCustomAttribute("WINDOW", out windowHnd); // window is your RenderWindow!
			pl.Insert("WINDOW", windowHnd.ToString());

//#if DEBUG
			// this stops MOIS from swallowing the mouse
			pl.Insert("w32_mouse", "DISCL_NONEXCLUSIVE");
			pl.Insert("w32_mouse", "DISCL_FOREGROUND");
//#endif

			InputManager = InputManager.CreateInputSystem(pl);

			// Create all devices (except joystick, as most people have Keyboard/Mouse) using buffered input.
			InputKeyboard = (Keyboard) InputManager.CreateInputObject(Type.OISKeyboard, true);
			InputMouse = (Mouse) InputManager.CreateInputObject(Type.OISMouse, true);
			InputController = new ControllerManager( );

			// sets the mouseState initial width and height (default is too low)
			MouseState_NativePtr mouseState = InputMouse.MouseState;
			mouseState.width = LKernel.GetG<Viewport>().ActualWidth;
			mouseState.height = LKernel.GetG<Viewport>().ActualHeight;

			LKernel.GetG<Root>().FrameStarted += new FrameListener.FrameStartedHandler(FrameStarted);

			CreateEventHandlers();

			Launch.Log("[Loading] MOIS input system loaded!");
		}

		/// <summary>
		/// Hook up to MOIS' event handlers
		/// </summary>
		private void CreateEventHandlers() {
			if (InputKeyboard != null) {
				Launch.Log("[Loading] Setting up keyboard listeners");
				InputKeyboard.KeyPressed += new KeyListener.KeyPressedHandler(KeyPressed);
				InputKeyboard.KeyReleased += new KeyListener.KeyReleasedHandler(KeyReleased);
			}
			if (InputMouse != null) {
				Launch.Log("[Loading] Setting up mouse listeners");
				InputMouse.MousePressed += new MouseListener.MousePressedHandler(MousePressed);
				InputMouse.MouseReleased += new MouseListener.MouseReleasedHandler(MouseReleased);
				InputMouse.MouseMoved += new MouseListener.MouseMovedHandler(MouseMotion);
			}
			if ( InputController != null )
			{
				Launch.Log( "[Loading] Setting up joypad listeners" );
			}
		}

		// ============================================================


		//float timeSinceLastFrame = 0;
		//private readonly float _inputCaptureRate = Settings.Default.InputCaptureRate;

		bool FrameStarted(FrameEvent e) {
			if (!LKernel.GetG<LevelManager>().IsValidLevel)
				return true;

			//timeSinceLastFrame += e.timeSinceLastFrame;
			//if (timeSinceLastFrame >= _inputCaptureRate) {
				// Capture all key presses since last check.
				InputKeyboard.Capture();
				// Capture all mouse movements and button presses since last check.
				InputMouse.Capture();
			//	timeSinceLastFrame -= _inputCaptureRate;
			//}

			return true;
		}

		// ============================================================

		#region Event firing helpers
		/// <summary>
		/// Fires an event for an axis on a joypad
		/// </summary>
		/// <param name="handler"></param>
		/// <param name="eventArgs"></param>
		void FireAxisEvent( AxisMovedEventHandler handler, ControllerAxisArgument eventArgs )
		{
			if ( handler != null )
				handler( this, eventArgs );
		}
		
		/// <summary>
		/// Fires an event. Helper method so I don't have to check every single event for null.
		/// </summary>
		/// <typeparam name="T">The parameter type of the event</typeparam>
		/// <param name="handler">The event handler</param>
		/// <param name="eventArgs">The parameter of the event</param>
		void FireEvent<T>(LymphInputEvent<T> handler, T eventArgs) {
			if (handler != null)
				handler(eventArgs);
		}

		/// <summary>
		/// Fires an event. Helper method so I don't have to check every single event for null.
		/// </summary>
		/// <typeparam name="T">The first parameter type of the event</typeparam>
		/// <typeparam name="U">The second parameter type of the event</typeparam>
		/// <param name="handler">The event handler</param>
		/// <param name="eventArg1">The first parameter of the event</param>
		/// <param name="eventArg2">The second parameter of the event</param>
		void FireEvent<T, U>(LymphInputEvent<T, U> handler, T eventArg1, U eventArg2) {
			if (handler != null)
				handler(eventArg1, eventArg2);
		}
		#endregion

		/// <summary>
		/// Handles key pressing and fires appropriate events
		/// </summary>
		bool KeyPressed(KeyEvent ke) {
#if PRINTINPUT
			Console.WriteLine("Pressed: " + ke.key);
#endif
			switch (ke.key) {
				case KeyCode.KC_ESCAPE:
					FireEvent<KeyEvent>(OnKeyboardPress_Escape, ke); break;
			}
			FireEvent<KeyEvent>(OnKeyboardPress_Anything, ke);
			return true;
		}

		/// <summary>
		/// Handles key releasing and fires appropriate events
		/// </summary>
		bool KeyReleased(KeyEvent ke) {
#if PRINTINPUT
			Console.WriteLine("Released: " + ke.key);
#endif
			FireEvent<KeyEvent>(OnKeyboardRelease_Anything, ke);
			return true;
		}

		/// <summary>
		/// Handles mouse pressing and fires appropriate events
		/// </summary>
		bool MousePressed(MouseEvent me, MouseButtonID id) {
#if PRINTINPUT
			Console.WriteLine("Mouse " + id + " pressed");
#endif
			FireEvent<MouseEvent, MouseButtonID>(OnMousePress_Anything, me, id);

			switch (id) {
				case MouseButtonID.MB_Left:
					FireEvent<MouseEvent, MouseButtonID>(OnMousePress_Left, me, id); break;
				case MouseButtonID.MB_Right:
					FireEvent<MouseEvent, MouseButtonID>(OnMousePress_Right, me, id); break;
				case MouseButtonID.MB_Middle:
					FireEvent<MouseEvent, MouseButtonID>(OnMousePress_Middle, me, id); break;
			}
			return true;

		}

		/// <summary>
		/// Handles mouse releasing and fires appropriate events
		/// </summary>
		bool MouseReleased(MouseEvent me, MouseButtonID id) {
#if PRINTINPUT
			Console.WriteLine("Mouse " + id + " released");
#endif
			FireEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Anything, me, id);
			
			switch (id) {
				case MouseButtonID.MB_Left:
					FireEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Left, me, id); break;
				case MouseButtonID.MB_Right:
					FireEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Right, me, id); break;
				case MouseButtonID.MB_Middle:
					FireEvent<MouseEvent, MouseButtonID>(OnMouseRelease_Middle, me, id); break;
			}
			return true;
		}

		/// <summary>
		/// Handles mouse movement and fires appropriate events
		/// - scroll wheel counts as a movement rather than a press
		/// </summary>
		bool MouseMotion(MouseEvent me) {
			// you can use handler.state.Y.rel for relative position, and handler.state.Y.abs for absolute
#if PRINTINPUT
			Console.WriteLine("Mouse moved: x " + me.state.X.rel + " | y " + me.state.Y.rel);
#endif
			FireEvent<MouseEvent>(OnMouseMove, me);
			return true;
		}

		bool ControllerAxisMoved( ControllerAxisArgument e )
		{
			switch ( e.Axis )
			{
				case ControllerAxis.LeftX:
					FireAxisEvent( OnLeftXAxisMoved, e ); break;
			}
			return true;
		}

		// =========================================================

		#region Events
		/// <summary> When any keyboard button is pressed. This should eventually be removed once we know what all of the keys are. </summary>
		public event LymphInputEvent<KeyEvent> OnKeyboardPress_Anything;
		public event LymphInputEvent<KeyEvent> OnKeyboardRelease_Anything;

		public event LymphInputEvent<KeyEvent> OnKeyboardPress_Escape;

		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMousePress_Anything;
		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMouseRelease_Anything;

		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMousePress_Left;
		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMouseRelease_Left;
		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMousePress_Right;
		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMouseRelease_Right;
		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMousePress_Middle;
		public event LymphInputEvent<MouseEvent, MouseButtonID> OnMouseRelease_Middle;

		public event LymphInputEvent<MouseEvent> OnMouseMove;

		public event AxisMovedEventHandler OnLeftXAxisMoved;
		#endregion
	}

	public delegate void LymphInputEvent<T>(T eventArgs);
	public delegate void LymphInputEvent<T, U>(T eventArg1, U eventArg2);
	public delegate void AxisMovedEventHandler( object sender, ControllerAxisArgument e );
}
