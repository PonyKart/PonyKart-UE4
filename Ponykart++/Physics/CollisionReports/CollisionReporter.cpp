#include "pch.h"
#include "Levels/LevelManager.h"
#include "Misc/ogreExtensions.h"
#include "Physics/CollisionReports/CollisionReporter.h"
#include "Physics/PhysicsExtensions.h"
#include "Physics/PhysicsMain.h"

using namespace std;
using namespace std::placeholders;
using namespace Extensions;
using namespace Ponykart::Levels;
using namespace Ponykart::Physics;

CollisionReporter::CollisionReporter()
{
	// Allocate our 2D array
	reporters = new CollisionReportEvent*[HIGHEST_BIT_IN_COLLISION_GROUPS + 1];
	for (int i = 0; i <= HIGHEST_BIT_IN_COLLISION_GROUPS; i++)
		reporters[i] = new CollisionReportEvent[HIGHEST_BIT_IN_COLLISION_GROUPS + 1];

	PhysicsMain::preSimulate.push_back(bind(&CollisionReporter::preSimulate, this, placeholders::_1, placeholders::_2));
	PhysicsMain::postSimulate.push_back(bind(&CollisionReporter::postSimulate, this,placeholders::_1, placeholders::_2));
	PhysicsMain::contactAdded.push_back(bind(&CollisionReporter::contactAdded,this,placeholders::_1,placeholders::_2,placeholders::_3,placeholders::_4,placeholders::_5,placeholders::_6,placeholders::_7));
	LevelManager::onLevelUnload.push_back(bind(&CollisionReporter::onLevelUnload, this, placeholders::_1));
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

void CollisionReporter::preSimulate(btDiscreteDynamicsWorld* world, const Ogre::FrameEvent& evt) 
{
	// we start with an empty dict and then gradually build it up. After the frame, we replace the old dict with this one,
	// and then find which pairs did not exist in the new one and fire stoppedtouching events for them

	newCollidingWith.clear();
	newCollidingWith.reserve(currentlyCollidingWith.size());
}

void CollisionReporter::postSimulate(btDiscreteDynamicsWorld* world, const Ogre::FrameEvent& evt) 
{
	for (auto& oldPair : currentlyCollidingWith) 
	{
		// does the new dict have the old key?
		if (newCollidingWith.find(oldPair.first) != end(newCollidingWith))
		{
			// find all of the objects that were in the old list but weren't in the new one
			// TODO: Check if this is correct.
			auto& newList = newCollidingWith[oldPair.first];
			for (auto& obj : oldPair.second)
			{
				if (newList.find(obj) != end(newList))
					continue;
				// fire events for each of them
				// this stops us from firing events twice
				if (getCollisionGroup(oldPair.first) < getCollisionGroup(obj))
					setupAndFireEvent(oldPair.first, obj, nullptr, nullptr, ObjectTouchingFlags::StoppedTouching);
			}
		}
		// if it doesn't, that means two things stopped touching, and the new dict only had one object for that key.
		else if (oldPair.second.size() > 0) 
		{
			// this stops us from firing events twice
			const btCollisionObject* toStopObjectA = oldPair.first;
			const btCollisionObject* toStopObjectB = oldPair.second.begin().operator*();

			if (getCollisionGroup(toStopObjectA) < getCollisionGroup(toStopObjectB))
				setupAndFireEvent(toStopObjectA, toStopObjectB, nullptr, nullptr, ObjectTouchingFlags::StoppedTouching);
		}
	}

	currentlyCollidingWith = newCollidingWith;
}

bool CollisionReporter::contactAdded(btManifoldPoint& point, btCollisionObjectWrapper* objectA, int partId0, 
									 int index0, btCollisionObjectWrapper* objectB, int partId1, int index1) 
{
	// if one of the two objects is deactivated, we don't care
	if (!objectA->getCollisionObject()->isActive() && !objectB->getCollisionObject()->isActive())
		return false;

	int objectACollisionGroup = (int)getCollisionGroup(objectA->getCollisionObject());
	int objectBCollisionGroup = (int)getCollisionGroup(objectB->getCollisionObject());

	// do we have any events that care about these groups? if not, then skip this collision pair
	if (reporters[objectACollisionGroup, objectBCollisionGroup] == nullptr
		&& reporters[objectBCollisionGroup, objectACollisionGroup] == nullptr)
		return false;

	// when the actual bodies are touching and not just their AABB's
	if (point.getDistance() <= 0.05) 
	{
		// get the lists
		unordered_set<const btCollisionObject*> objectAList = getCollisionListForObject(objectA->getCollisionObject(), currentlyCollidingWith),
			newObjectAList = getCollisionListForObject(objectA->getCollisionObject(), newCollidingWith),
			objectBList = getCollisionListForObject(objectB->getCollisionObject(), currentlyCollidingWith),
			newObjectBList = getCollisionListForObject(objectB->getCollisionObject(), newCollidingWith);

		// see if the other object is in there
		if (objectAList.find(objectB->getCollisionObject()) == end(objectAList)
			|| objectBList.find(objectA->getCollisionObject()) == end(objectBList)) 
		{
			// if it isn't, add it! this means we have a new collision and need to fire off an event!
			// okay now we need to get the point where it contacted!
			// Limitation with this system: if we're already colliding with B and then collide with it in a different place without
			// leaving the original place, we won't get another event. Why? Well because what if something's sliding along?
			// Don't need loads of events for that

			// make sure we add it to our collections! The hashset means we don't have to worry about duplicates
			objectAList.emplace(objectB->getCollisionObject());
			objectBList.emplace(objectA->getCollisionObject());
			newObjectAList.emplace(objectB->getCollisionObject());
			newObjectBList.emplace(objectA->getCollisionObject());

			// update the dictionaries (is this necessary?)
			currentlyCollidingWith[objectA->getCollisionObject()] = objectAList;
			currentlyCollidingWith[objectB->getCollisionObject()] = objectBList;
			newCollidingWith[objectA->getCollisionObject()] = newObjectAList;
			newCollidingWith[objectB->getCollisionObject()] = newObjectBList;

			
			Ogre::Vector3 pos = toOgreVector3(point.getPositionWorldOnA()).midPoint(toOgreVector3(point.getPositionWorldOnB()));
			Ogre::Vector3 normal = toOgreVector3(point.m_normalWorldOnB);

			// woop woop they started touching, so we fire off an event!
			setupAndFireEvent(objectA->getCollisionObject(), objectB->getCollisionObject(), 
								&pos, &normal, ObjectTouchingFlags::StartedTouching);
		}
		else 
		{
			// already in the dictionary, no new collisions. Add it to the new dictionary anyway though, because if we don't then it thinks
			// they stopped colliding. Which we don't want!
			newObjectAList.emplace(objectB->getCollisionObject());
			newObjectBList.emplace(objectA->getCollisionObject());

			newCollidingWith[objectA->getCollisionObject()] = newObjectAList;
			newCollidingWith[objectB->getCollisionObject()] = newObjectBList;
		}
	}
	// This means they're still inside each other's AABB's, but they aren't actually touching
	//else {

	//}

	return false;
}

void CollisionReporter::setupAndFireEvent(const btCollisionObject* const objectA, const btCollisionObject* const objectB,
	Ogre::Vector3* position, Ogre::Vector3* normal, ObjectTouchingFlags flags) 
{
	PonykartCollisionGroups groupA = getCollisionGroup(objectA);
	PonykartCollisionGroups groupB = getCollisionGroup(objectB);
	//int groupIDA = (int) groupA;
	//int groupIDB = (int) groupB;

	CollisionReportInfo info{groupA,groupB,objectA,objectB,position,normal,flags};

	fireEvent(&info);
}

void CollisionReporter::fireEvent(CollisionReportInfo* info)
{
	CollisionReportEvent& e = reporters[(int)info->firstGroup][(int)info->secondGroup];
	if (e.size())
		for (auto& fun : e)
			fun(info);
}

void CollisionReporter::onLevelUnload(LevelChangedEventArgs* eventArgs)
{
	currentlyCollidingWith.clear();
}

/// Gets the collision set associated with this CollisionObject in the CurrentlyCollidingWith dictionary.
/// If the object doesn't have a set in the dictionary, this method makes one and adds it to the dictionary.
/// It does not add anything to the new set, but it initialises it.
/// @param colObj The CollisionObject to use as the "key" in the dictionary.
/// @param dict The dictionary to check against
/// @return The collision set associated with the given CollisionObject.
std::unordered_set<const btCollisionObject*> CollisionReporter::getCollisionListForObject(const btCollisionObject* const colObj,
	std::unordered_map<const btCollisionObject*, std::unordered_set<const btCollisionObject*>>& dict)
{
	unordered_set<const btCollisionObject*> colSet;
	auto it = dict.find(colObj);
	if (it == end(dict))
		dict.emplace(colObj, colSet);
	else
		colSet = it->second;
	return colSet;
}
