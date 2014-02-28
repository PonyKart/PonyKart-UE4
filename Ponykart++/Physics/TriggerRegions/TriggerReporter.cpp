#include "pch.h"
#include <BulletCollision/CollisionDispatch/btCollisionObject.h>
#include <BulletCollision/CollisionDispatch/btGhostObject.h>
#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.H"
#include "Physics/CollisionReports/CollisionReporter.h"
#include "Physics/CollisionReports/CollisionReportInfo.h"
#include "Physics/PhysicsExtensions.h"
#include "Physics/TriggerRegions/TriggerReporter.h"

using namespace std;
using namespace Extensions;
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

void TriggerReporter::onLevelUnload(LevelChangedEventArgs* args) 
{
	for (auto tr : regions)
		delete tr.second;
	regions.clear();
}

/// Runs whenever we get a collision event from trigger/kart collisions
void TriggerReporter::collisionEvent(CollisionReportInfo* info) 
{
	// get our objects
	btRigidBody* kartBody;
	btGhostObject* triggerBody;
	if (getCollisionGroup(info->firstObject) == PonykartCollisionGroups::Triggers)
	{
		//ghost = info.SecondObject as GhostObject;
		triggerBody = (btGhostObject*)info->firstObject;
		kartBody = (btRigidBody*)info->secondObject;
	}
	else 
	{
		triggerBody = (btGhostObject*)info->secondObject;
		kartBody = (btRigidBody*)info->firstObject;
	}

	// get our region
	auto it = regions.find(getName(triggerBody));
	if (it != end(regions)) 
	{
		TriggerRegion* region = it->second;
		// started touching = enter
		if (info->flags == ObjectTouchingFlags::StartedTouching) 
		{
			region->currentlyCollidingWith.emplace(kartBody);
			region->invokeTrigger(kartBody, TriggerReportFlags::Enter, info);

			if (onTriggerContact.size())
				for (auto& fun : onTriggerContact)
					fun(region, kartBody, TriggerReportFlags::Enter, info);
			if (onTriggerEnter.size())
				for (auto& fun : onTriggerContact)
					fun(region, kartBody, TriggerReportFlags::Enter, info);
		}
		// stopped touching = leave
		else if (info->flags == ObjectTouchingFlags::StoppedTouching) 
		{
			region->currentlyCollidingWith.erase(kartBody);
			region->invokeTrigger(kartBody, TriggerReportFlags::Leave, info);

			if (onTriggerContact.size())
				for (auto& fun : onTriggerContact)
					fun(region, kartBody, TriggerReportFlags::Leave, info);
			if (onTriggerLeave.size())
				for (auto& fun : onTriggerContact)
					fun(region, kartBody, TriggerReportFlags::Leave, info);
		}
	}


}
