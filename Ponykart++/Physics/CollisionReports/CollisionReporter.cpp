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

	PhysicsMain::preSimulate.push_back(bind(&CollisionReporter::preSimulate, this, _1, _2));
	PhysicsMain::postSimulate.push_back(bind(&CollisionReporter::postSimulate, this, _1, _2));
	PhysicsMain::contactAdded.push_back(bind(&CollisionReporter::contactAdded,this,_1,_2,_3,_4,_5,_6,_7));
	LevelManager::onLevelUnload.push_back(bind(&CollisionReporter::onLevelUnload, this, _1));
}