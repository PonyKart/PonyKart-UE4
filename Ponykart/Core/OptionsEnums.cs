
namespace Ponykart.Core {
	public enum ModelDetailOption {
		/// <summary>
		/// High detail. No billboard imposters, all static geometry visible at all times
		/// </summary>
		High,
		/// <summary>
		/// In the middle - you have billboard imposters, but only for far-off things and static geometry is hidden
		/// and replaced as you get far away.
		/// </summary>
		Medium,
		/// <summary>
		/// Low detail option. All trees and stuff are billboard all the time. Good for netbooks and really old computers.
		/// </summary>
		Low
	}

	public enum ShadowDetailOption {
		/// <summary>
		/// No shadows at all
		/// </summary>
		None,
		/// <summary>
		/// Some shadows on important things
		/// </summary>
		Some,
		/// <summary>
		/// Shadows on everything!
		/// </summary>
		Many
	}
}
