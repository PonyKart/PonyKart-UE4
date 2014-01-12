using Mogre;

namespace Ponykart.Stuff {
	public class DebugOverlayManager {
		/// <summary>
		/// The Ogre DebugOverlay.
		/// </summary>
		private Overlay overlay;
		private OverlayElement guiAvg, guiCurr, guiTris, guiBatches;

		public DebugOverlayManager() {
			this.overlay = OverlayManager.Singleton.GetByName("Core/DebugOverlay");
			Launch.OnEveryUnpausedTenthOfASecondEvent += EveryTenth;

#if DEBUG
			ShowDebugOverlay(true);
#endif
			guiAvg = OverlayManager.Singleton.GetOverlayElement("Core/AverageFps", false);
			guiCurr = OverlayManager.Singleton.GetOverlayElement("Core/CurrFps", false);
			guiTris = OverlayManager.Singleton.GetOverlayElement("Core/NumTris", false);
			guiBatches = OverlayManager.Singleton.GetOverlayElement("Core/NumBatches", false);
		}

		/// <summary> 
		/// Turns debug overlay on or off
		/// </summary>
		public void ShowDebugOverlay(bool show) {
			if (this.overlay != null) {
				if (show)
					overlay.Show();
				else
					overlay.Hide();
			}
		}

		public void ToggleDebugOverlay() {
			ShowDebugOverlay(!overlay.IsVisible);
		}

		void EveryTenth(object o) {
			// update stats when necessary
			RenderTarget.FrameStats stats = LKernel.GetG<RenderWindow>().GetStatistics();

			guiAvg.Caption = "Average FPS: " + stats.AvgFPS;
			guiCurr.Caption = "Current FPS: " + stats.LastFPS;
			guiTris.Caption = "Triangle Count: " + stats.TriangleCount;
			guiBatches.Caption = "Batch Count: " + stats.BatchCount;
		}
	}
}
