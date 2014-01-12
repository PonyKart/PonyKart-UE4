
namespace Ponykart.Levels {
	public class LevelChangedEventArgs {

		/// <summary>
		/// If you're reading this from OnLevelUnload or OnLevelPreUnload, keep in mind that the new level has not been
		/// fully initialised yet, so properties such as Type and stuff will probably not be correct!.
		/// </summary>
		public readonly Level NewLevel;

		public readonly Level OldLevel;

		public readonly LevelChangeRequest Request;

		public LevelChangedEventArgs(Level newLevel, Level oldLevel, LevelChangeRequest request) {
			NewLevel = newLevel;
			OldLevel = oldLevel;
			Request = request;
		}
	}
}
