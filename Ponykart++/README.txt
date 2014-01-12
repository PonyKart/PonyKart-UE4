# Dependencies : 
- Ogre (and OIS) 1.8.1
- Bullet2 (Dynamics, Collision, LinearMath, FileLoader, WorldImporter) >= 2.81-r2613
- Boost >= 1.55.0
- Lua 5.1
- irrKlang 1.4.0

# Build instructions :
- Install the dependencies (use the same compiler for all the dependencies and the project)
- Compile the project, a Visual Studio 2013 and Code::Blocks project files are provided for Windows and Linux
- Copy the media/ folder in the build/ folder
- Run from the build/ folder

## Windows :
- Compile all the dependencies with MSVC 2013
- Install them in C:\ in their respective folders (OGRE, irrKlang-1.4.0, BULLET_PHYSICS, boost_1_55_0, Lua\5.1)
- Open the Visual Studio project, build and run.

## Linux :
- Install the dependencies with your package manager, or manually in /usr/local/
- Open the Code::Blocks project, build and run.