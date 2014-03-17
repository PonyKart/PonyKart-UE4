#ifndef ITEM_H_INCLUDED
#define ITEM_H_INCLUDED
#include <string>

namespace Ponykart
{
namespace Actors{ class LThing; }
namespace Players{ class Player; }
namespace Items
{
	class Item
	{
	public:
		
	protected:
		Item() {};
		Item(Players::Player* User, std::string itemName);
		virtual void onUse() {};
		virtual void everyTenth(void* o) {};
	protected:
		Actors::LThing* body;
		Players::Player* user;
	};
}
}
#endif // ITEM_H_INCLUDED
