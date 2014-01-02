
namespace Ponykart.Physics {
	/// <summary>
	/// Flags we use when firing trigger region events
	/// </summary>
	public enum TriggerReportFlags {
		/// <summary>
		/// Not sure when you'd use "none", but oh well here it is
		/// </summary>
		None,
		/// <summary>
		/// Something has entered the region
		/// </summary>
		Enter,
		/// <summary>
		/// Something has left the region
		/// </summary>
		Leave
	}
}
