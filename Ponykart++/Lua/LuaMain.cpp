#include "Kernel/LKernel.h"
#include "Kernel/LKernelOgre.h"
#include "Levels/LevelManager.h"
#include "Lua/LuaMain.h"

#include "lua.hpp"
#include "lualib.h"
#include "lauxlib.h"

using namespace Ponykart;
using namespace Ponykart::Levels;
using namespace Ponykart::LKernel;
using namespace Ponykart::Lua;

LuaMain::LuaMain()
{
    initLuaState();
}

void LuaMain::registerLuaFunctions(std::function<void ()> func)
{
    onRegister.push_back(func);
}

void LuaMain::runRegisterEvent()
{
    typedef std::vector<std::function<void ()>>::iterator itr_t;
    for(itr_t itr = onRegister.begin(); itr != onRegister.end(); itr++) {
        (*itr)();
    }
}

void LuaMain::doFunctionForLThing(std::string& functionName, Actors::LThing* thing)
{
    if(LKernel::getG<LevelManager>()->getIsValidLevel()) {
        luaMutex.lock();

        lua_getglobal(lua, functionName.c_str());
        lua_pushlightuserdata(lua, thing);

        luaMutex.unlock();
    }
}

void LuaMain::doFile(const std::string &filename)
{
    if(LKernel::getG<LevelManager>()->getIsValidLevel()) {
        LKernel::log("[LuaMain] Running file: " + filename);
        luaL_dofile(lua, filename.c_str());
    }
}

void LuaMain::doString(const std::string &s)
{
    if(LKernel::getG<LevelManager>()->getIsValidLevel()) {
        luaL_dostring(lua, s.c_str());
    }
}

void LuaMain::quit()
{
    lua_close(lua);
}

void LuaMain::restart()
{
    quit();
    initLuaState();
    runRegisterEvent();
}

void LuaMain::print(const std::string &s)
{
    LKernel::log("[Lua] " + s);
}

void LuaMain::initLuaState()
{
    lua = luaL_newstate();

    luaopen_io(lua);
    luaopen_base(lua);
    luaopen_table(lua);
    luaopen_string(lua);
    luaopen_math(lua);
}
