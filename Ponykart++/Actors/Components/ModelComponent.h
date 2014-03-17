#ifndef MODELCOMPONENT_H_INCLUDED
#define MODELCOMPONENT_H_INCLUDED

#include <Ogre.h>

namespace PonykartParsers
{
	class ThingBlock;
	class ModelBlock;
	class ThingDefinition;
} // PonykartParsers

namespace Ogre{class AnimationBlender;}
namespace Ponykart
{
namespace Actors
{

class LThing;

/// Represents an ogre node and mesh
class ModelComponent
{
public:
	ModelComponent(LThing* lthing, PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::ModelBlock* block, PonykartParsers::ThingDefinition* def); ///< Creates a model component for a Thing.
	// Getters
	Ogre::SceneNode* getNode();
	Ogre::Entity* getEntity();
	unsigned getId() const;
	std::string getName() const;
	const LThing* const getOwner() const;
protected:
	void setupAnimation(PonykartParsers::ModelBlock* block); ///< Only does simple animations for now
	void setupEntity(Ogre::SceneManager* sceneMgr, PonykartParsers::ModelBlock* block);
private:
	Ogre::Quaternion initSpawnOrientation(const PonykartParsers::ModelBlock* const block) const;
public:
	Ogre::AnimationBlender* animationBlender;
	Ogre::AnimationState* animationState;
	const Ogre::Vector3 spawnPosition;
	const Ogre::Quaternion spawnOrientation;
	const Ogre::Vector3 spawnScale;
protected:
	Ogre::SceneNode* node;
	Ogre::Entity* entity;
	unsigned id;
	std::string name;
	LThing* owner;
};

} // Actors
} // Ponykart

#endif // MODELCOMPONENT_H_INCLUDED
