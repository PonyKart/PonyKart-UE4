#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.H"
#include "Physics/CollisionReports/CollisionReporter.h"
#include "Physics/TriggerRegions/TriggerReporter.h"

using namespace std;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Physics;

TriggerReporter::TriggerReporter()
{
	log("[Loading] Creating TriggerReporter...");
	
	LevelManager::onLevelUnload.push_back(bind(&TriggerReporter::onLevelUnload, this, placeholders::_1));
	vector<std::function<void(CollisionReportInfo* info)>> newEvent;
	newEvent.push_back(bind(&TriggerReporter::collisionEvent, this, placeholders::_1));
	LKernel::getG<CollisionReporter>()->addEvent(PonykartCollisionGroups::Karts, PonykartCollisionGroups::Triggers, newEvent);
}

std::unordered_map<std::string, TriggerRegion*>& TriggerReporter::getRegions()
{
	return regions;
}
