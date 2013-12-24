#ifndef BILLBOARDSETCOMPONENT_H_INCLUDED
#define BILLBOARDSETCOMPONENT_H_INCLUDED

#include <string>
#include <Ogre.h>

namespace PonykartParsers
{
    class ThingBlock;
    class BillboardBlock;
    class BillboardSetBlock;
} // PonykartParsers

namespace Ponykart
{
namespace Actors
{
	class LThing;

	// Represents a billboard set, aka a collection of billboards that share the same material, direction, etc.
	class BillboardSetComponent
	{
	public:
		BillboardSetComponent(LThing* lthing, PonykartParsers::ThingBlock* thingTemplate, PonykartParsers::BillboardSetBlock* block); // Woo billboards! These are pretty dissimilar from ogre entities - they only have a size and material, and no mesh or anything
		void createBillboard(const PonykartParsers::BillboardBlock* const block); // Make one billboard from each billboard block.
		// Getters
		unsigned getId() const;
		std::string getName() const;
		const Ogre::BillboardSet* const getBillboardSet() const;
	protected:
		unsigned id;
		std::string name;
		Ogre::BillboardSet* billboardSet;
	private:
		Ogre::SceneNode* localNode;
	};
} // Actors
} // Ponykart

#endif // BILLBOARDSETCOMPONENT_H_INCLUDED
