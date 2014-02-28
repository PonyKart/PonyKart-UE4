#include "pch.h"
#include "Core/InputSwallowerManager.h"

using namespace std;
using namespace Ponykart;

void InputSwallowerManager::addSwallower(const bool* const condition, void* swallower)
{
	ThingsToCheck.insert(pair<const bool* const,void*>(condition, swallower));
}

bool InputSwallowerManager::isSwallowed(void* querier)
{
	bool result = false;
	// we go through our conditions to check
	for (auto pair : ThingsToCheck)
	{
		// and we don't count things that are managed by the querier
		if (pair.second == querier)
			continue;
		// If any of the conditions are true, the input is swallowed.
		result = (result || *(pair.first));
	}
	return result;
}

bool InputSwallowerManager::isSwallowed()
{
	bool result = false;
	// we go through our conditions to check
	for (auto pair : ThingsToCheck) // If any of the conditions are true, the input is swallowed.
		result = (result || *(pair.first)); 
	return result;
}
