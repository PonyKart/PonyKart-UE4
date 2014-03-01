#ifndef TRIGGERREPORTER_H_INCLUDED
#define TRIGGERREPORTER_H_INCLUDED

#include <string>
#include <unordered_map>
#include "Kernel/LKernelObject.h"
#include "Physics/CollisionReports/CollisionReporter.h"
#include "Physics/TriggerRegions/TriggerRegion.h"

namespace Ponykart
{
namespace Levels{ class LevelChangedEventArgs; }
namespace Physics
{
	class TriggerRegion;
	/// The main thing of this you use is AddEvent. Stick in the name of the trigger region and the method you want
	/// to run when something enters/leaves it, and you're good to go. The method you give it can check stuff like which bodies
	/// were involved and whether it was an entry or leave event.
	/// (You can use the extension methods IsLeaveFlag and IsEnterFlag on TriggerFlags to help with this)
	/// 
	/// If you're using a handler class thingy, don't forget to add RemoveEvent in its Dispose method.
	class TriggerReporter : public LKernel::LKernelObject
	{
	public:
		TriggerReporter();
		void collisionEvent(CollisionReportInfo* info); ///< Runs whenever we get a collision event from trigger/kart collisions
		void onLevelUnload(Levels::LevelChangedEventArgs* args); ///< Clean up all of the regions when we unload a level
		TriggerRegion* addEvent(std::string& regionName, TriggerReportEvent handler);
		bool removeEvent(std::string& regionName, TriggerReportEvent handler);
		std::unordered_map<std::string, TriggerRegion*>& getRegions();

		TriggerReportEvent onTriggerContact;
		TriggerReportEvent onTriggerEnter;
		TriggerReportEvent onTriggerLeave;

	private:
		std::unordered_map<std::string, TriggerRegion*> regions;
	};
}
}
#endif TRIGGERREPORTER_H_INCLUDED