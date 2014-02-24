namespace Ponykart
{
namespace Physics
{
	/// Flags we use when firing trigger region events
	enum TriggerReportFlags
	{
		/// Not sure when you'd use "none", but oh well here it is
		None,
		/// Something has entered the region
		Enter,
		/// Something has left the region
		Leave
	};
}
}
