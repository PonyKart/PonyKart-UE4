#ifndef RIBBONCOMPONENT_H_INCLUDED
#define RIBBONCOMPONENT_H_INCLUDED

#include <string>

namespace Ogre
{
	class RibbonTrail;
	class SceneNode;
} // Ogre

namespace PonykartParsers
{
	class RibbonBlock;
	class ThingBlock;
} // PonykartParsers

namespace Ponykart
{
namespace Actors
{
	class LThing;

	// Represents an ogre ribbon
	class RibbonComponent
	{
	public:
		RibbonComponent(LThing* lthing, PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::RibbonBlock* block); // For ribbons!
		~RibbonComponent();
		// Getters
		unsigned getId() const;
		std::string getName() const;
		Ogre::RibbonTrail* getRibbon(); // The ribbon emitter
		const Ogre::SceneNode* const getRibbonNode() const; // The SceneNode that the ribbon is attached to

	protected:
		unsigned id;
		std::string name;
		Ogre::RibbonTrail* ribbon; // The ribbon emitter
		Ogre::SceneNode* ribbonNode; // The SceneNode that the ribbon is attached to
		Ogre::SceneNode* trackedRibbonNode;
	};
} // Actors
} // Ponykart

#endif // RIBBONCOMPONENT_H_INCLUDED
