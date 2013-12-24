#ifndef LUAMAIN_H_INCLUDED
#define LUAMAIN_H_INCLUDED

#include <vector>
#include <functional>

namespace Ponykart
{

namespace Actors{class LThing;}

namespace Lua
{

using LuaEvent = std::vector<std::function<void ()>>;

class LuaMain // TODO: Implement LuaMain
{
public:
	LuaMain();
	void doFunctionForLThing(std::string& functionName, Actors::LThing* thing);
};

} // Lua
} // Ponykart

#endif // LUAMAIN_H_INCLUDED
