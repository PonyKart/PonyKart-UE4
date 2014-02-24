#include "Levels/LevelManager.h"
#include "Physics/CollisionReports/CollisionReporter.h"
#include "Physics/PhysicsMain.h"

using namespace std;
using namespace std::placeholders;
using namespace Ponykart::Levels;
using namespace Ponykart::Physics;

CollisionReporter::CollisionReporter()
{
	// Allocate our 2D array
	reporters = new CollisionReportEvent*[HIGHEST_BIT_IN_COLLISION_GROUPS + 1];
	for (int i = 0; i <= HIGHEST_BIT_IN_COLLISION_GROUPS; i++)
		reporters[i] = new CollisionReportEvent[HIGHEST_BIT_IN_COLLISION_GROUPS + 1];

	// TODO: Implementing those and uncomment!
//	PhysicsMain::preSimulate.push_back(bind(&CollisionReporter::preSimulate, this, placeholders::_1, placeholders::_2));
//	PhysicsMain::postSimulate.push_back(bind(&CollisionReporter::postSimulate, this,placeholders::_1, placeholders::_2));
//	PhysicsMain::contactAdded.push_back(bind(&CollisionReporter::contactAdded,this,placeholders::_1,placeholders::_2,placeholders::_3,placeholders::_4,placeholders::_5,placeholders::_6,placeholders::_7));
//	LevelManager::onLevelUnload.push_back(bind(&CollisionReporter::onLevelUnload, this, placeholders::_1));
}

/// Hooks up an event handler to a collision event.
/// 
/// The order of the two groups do not matter, as it will add them to both [a,b] and [b,a].
/// 
/// For example, to listen for the player colliding with the wall, you want to use Groups.PlayerID and Groups.WallID.
/// @param handler The method that will run when the event is fired
void CollisionReporter::addEvent(PonykartCollisionGroups firstType, PonykartCollisionGroups secondType, CollisionReportEvent handler)
{
	addEvent((int)firstType, (int)secondType, handler);
}

/// Hooks up an event handler to a collision event.
/// 
/// The order of the two groups do not matter, as it will add them to both [a,b] and [b,a].
/// 
/// For example, to listen for the player colliding with the wall, you want to use Groups.PlayerID and Groups.WallID.
/// @param handler The method that will run when the event is fired
void CollisionReporter::addEvent(int firstType, int secondType, CollisionReportEvent handler)
{
	for (auto function : handler)
	{
		reporters[firstType][secondType].push_back(function);
		reporters[secondType][firstType].push_back(function);
	}
}
