#ifndef INPUTSWALLOWERMANAGER_H_INCLUDED
#define INPUTSWALLOWERMANAGER_H_INCLUDED

#include <map>

namespace Ponykart
{
// "Swallowing" input is when we only give input to one particular thing. For example, if we have WASD as movement keys,
// if we have a text box come up asking us for our name or something, we don't want to start moving while we're doing that.
// We would say that text box has "swallowed" the input. This class manages that.
//
// Sexual references ahoy.
//
// Oh and pausing also swallows input. Might want to do something about that later.
class InputSwallowerManager
{
public:
	// Add an object that has something that can be swallowed. If it's part of a UI, this should be the class that manages that UI thingy.
	// The function should be read as "if this is true, then swallow the input".
	// condition: A condition for when the input should be swallowed. If this is true, it is swallowed.
	// swallower: The object that "manages" the thing doing the swallowing. For example if the swallower is part of the UI, this should be that UI thingy's "manager".
	void addSwallower(const bool* const condition, void* swallower);
	// Is the current input swallowed or not, with respect to me?
	// This is needed because otherwise nothing would be able to do anything if the input was swallowed.
	// So when we're checking the conditions, we excude any that are managed by the querier.
	bool isSwallowed(void* querier);
	// Is the current input swallowed or not? This method is the same as IsSwallowed(object) but without the extra condition.
	// This should be used by objects that do not swallow input themselves.
	bool isSwallowed();

private:
	std::map<const bool* const, void*> ThingsToCheck;
};
} // Ponykart
#endif // INPUTSWALLOWERMANAGER_H_INCLUDED
