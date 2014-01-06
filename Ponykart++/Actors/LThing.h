#ifndef LTHING_H_INCLUDED
#define LTHING_H_INCLUDED

#include <string>
#include <vector>
#include <Ogre.h>
#include <BulletDynamics/Dynamics/btRigidBody.h>
#include "Physics/CollisionGroups.h"

class btMotionState;
class btRigidBody;

namespace PonykartParsers
{
	class ThingBlock;
	class ThingDefinition;
} // PonykartParsers

namespace Ponykart
{
namespace Actors
{

class ModelComponent;
class ShapeComponent;
class RibbonComponent;
class BillboardSetComponent;
class SoundComponent;

/// Our game object class! Pretty much everything you see in the game uses this
class LThing
{
public:
	LThing(PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ThingDefinition* def);
	virtual ~LThing();
	virtual void changeAnimation(const std::string& animationName);
	void runScript();
	// Getters
	unsigned getID() const;
	bool getSoundsNeedUpdate() const;
	std::string getName() const;
	std::string getScript() const;
	Ogre::SceneNode* getRootNode();
	btRigidBody* getBody();
	Ogre::Vector3 getBodyLinearVelocity() const;
	inline Ogre::Vector3 getSpawnPosition() const {return spawnPosition;};
	inline Ogre::Quaternion getSpawnOrientation() const {return spawnOrientation;};
	inline Ogre::Vector3 getSpawnScale() const {return spawnScale;};
	inline Physics::PonykartCollisionGroups* getCollisionGroup() {return &collisionGroup;};
	const std::vector<ModelComponent*>& getModelComponents() const;
	const std::vector<ShapeComponent*>& getShapeComponents() const;
	const std::vector<RibbonComponent*>& getRibbonComponents() const;
	const std::vector<BillboardSetComponent*>& getBillboardSetComponents() const;
	const std::vector<SoundComponent*>& getSoundComponents() const;
	// Setters
	void setSoundsNeedUpdate(bool value);

protected:
	virtual btMotionState* getInitializationMotionState();
	/// Use this method if you need some more stuff to happen before the constructor starts setting everything up.
	/** For example if you need to get more things out of the ThingTemplate, you can use this for that. */
	virtual void preSetup(PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ThingDefinition* def){};
	virtual void initialiseComponents(PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ThingDefinition* def); ///< Make our components
	virtual void postInitialiseComponents(PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ThingDefinition* def){};
	virtual void preSetUpBodyInfo(PonykartParsers::ThingDefinition* def){}; ///< If you need anything before we set up the body info
	virtual void postSetUpBodyInfo(PonykartParsers::ThingDefinition* def){}; ///< Override this if you want to do more to the construction info before it's used to create the body but after it's been created
	virtual void postCreateBody(PonykartParsers::ThingDefinition* td){}; ///< Override this if you want to do more to the rigid body
	// Getters
	const btMotionState* const getMotionState() const;
private:
	void setupOgre(PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ThingDefinition* def); ///< Sets up ogre stuff, like our root scene node
	void setupPhysics(PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ThingDefinition* def);
	void deleteIfStaticOrInstanced(PonykartParsers::ThingDefinition* def);
	void setUpBodyInfo(PonykartParsers::ThingDefinition* def); ///< Set up all of the stuff needed before we create our body
	void createBody(PonykartParsers::ThingDefinition* def); ///< Creates the body and makes it static/kinematic if specified.
	void setBodyUserObject(); ///< Sets the body's UserObject

protected:
	unsigned id; ///< Every lthing has an ID, though it's mostly just used to stop ogre complaining about duplicate names
	std::string name; ///< This lthing's name. It's usually the same as its .thing filename.
	Ogre::SceneNode* rootNode; ///< A scene node that all of the model components attach stuff to.
	std::vector<ModelComponent*> modelComponents;
	std::vector<ShapeComponent*> shapeComponents;
	std::vector<RibbonComponent*> ribbonComponents;
	std::vector<BillboardSetComponent*> billboardSetComponents;
	std::vector<SoundComponent*> soundComponents;
	/// Physics! If we have 0 shape components, this is null; if we have 1 shape component, this is a body made from that shape;
	/// if we have 2 or more shape components, this is a body made from a compound shape using all of the components' shapes
	btRigidBody* body;
	btRigidBody::btRigidBodyConstructionInfo* info;
	Physics::PonykartCollisionGroups collisionGroup; ///< The body's collision group
	Physics::PonykartCollidesWithGroups collidesWith; ///< What does the body collide with?
private:
	Ogre::Vector3 spawnPosition; ///< The thing's initial position when it's first created
	Ogre::Quaternion spawnOrientation; ///< The thing's initial orientation when it's first created
	Ogre::Vector3 spawnScale; ///< The thing's initial scale when it's first created. This is only used if it doesn't have any shape components.
	bool soundsNeedUpdate;
	std::string script;
	btMotionState* motionState; ///< The actual motion state.
};
} // Actors
} // Ponykart

#endif // LTHING_H_INCLUDED
