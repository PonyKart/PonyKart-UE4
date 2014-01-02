using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Mogre;
using Ponykart.Core;
using Ponykart.Physics;
using Ponykart.UI;
using Timer = System.Threading.Timer;

namespace Ponykart {
	public static class Launch {
		/// <summary>
		/// Is fired every 1/10th of a second when we aren't paused, since there's lots of things that want to run frequently but not every frame.
		/// The object is something that can be passed around from the timer, but it's just null for now.
		/// </summary>
		public static event TimerCallback OnEveryUnpausedTenthOfASecondEvent;
		private static Timer tenthTimer;
		public static bool Quit = false;

		[STAThread]
		public static void Main() {
			//#if !DEBUG
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
			//#endif

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");

			LKernel.Initialise();
			Options.Initialise();

			InitializeOgre();
			StartRendering();
		}

		private static void InitializeOgre() {
			Splash splash = new Splash();
			splash.Show();
			try {
				LKernel.LoadInitialObjects(splash);
				GC.Collect();
			}
			finally {
				splash.Close();
				splash.Dispose();
			}
		}


		/// <summary>
		/// Starts the render loop!
		/// </summary>
		private static void StartRendering() {
			Root root = LKernel.GetG<Root>();
			RenderWindow window = LKernel.GetG<RenderWindow>();

			root.RenderOneFrame();
			window.SetVisible(true);

			tenthTimer = new Timer(OnTenthTimerTick, null, 1000, 100);

			while (!Quit && !window.IsClosed) {
				if (!root.RenderOneFrame())
					break;
				// this is for stuff like window selection, moving, etc
				Application.DoEvents();
			}

			LKernel.GetG<UIMain>().Dispose();
			LKernel.GetG<PhysicsMain>().Dispose();
			tenthTimer.Dispose();
			if (root != null)
				root.Shutdown();
		}

		private static void OnTenthTimerTick(object o) {
			if (!Pauser.IsPaused)
				OnEveryUnpausedTenthOfASecondEvent.Invoke(o);
		}

		/// <summary>
		/// Fired whan an unhandled exception bubbles up to the AppDomain
		/// </summary>
		static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			var ex = e.ExceptionObject as Exception;
			if (ex != null) {
				MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, ex.GetType().ToString());
			}
			else if (OgreException.IsThrown) {
				MessageBox.Show(OgreException.LastException.FullDescription, "An Ogre exception has occurred! Check Ponykart.log for details.");
			}

			// sometimes ogre exceptions happen early on but they don't crash everything, like shader/material errors.
			// But when an actual error happens, the "ogre exception is thrown" flag is still set, so it displays
			// the wrong error message.

			// old version
			/*if (OgreException.IsThrown)
				MessageBox.Show(OgreException.LastException.FullDescription, "An Ogre exception has occurred!");
			else {
				var ex = e.ExceptionObject as Exception;
				if (ex != null)
					MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace, ex.GetType().ToString());
			}*/
		}

		/// <summary>
		/// Writes something to the console and also sticks it in ogre's log file
		/// </summary>
		/// <param name="message">The message to log</param>
		[DebuggerHidden]
		public static void Log(string message) {
#if DEBUG
			Debug.WriteLine(message);
#endif
			if (LogManager.Singleton != null)
				LogManager.Singleton.LogMessage(message);
		}
	}
}
