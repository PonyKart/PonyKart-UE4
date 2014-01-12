
namespace Ponykart.UI {
	/// <summary>
	/// special properties for UI objects go here
	/// 
	/// they go in the .UserData thing
	/// </summary>
	public class UIUserData {
		/// <summary>
		/// If the user clicks on this object, should it affect anything in the viewport?
		/// For example, if we click on the "menu" button, then nothing should happen to the gameplay, 
		/// whereas if we click on a see-through "messages" text box on the screen, clicks should still go "through".
		/// 
		/// True means this obstructs and "absorbs" click events, false means it does not.
		/// 
		/// Default is false.
		/// </summary>
		public bool ObstructsViewport { get; set; }

		public UIUserData() {
			ObstructsViewport = false;
		}
	}
}
