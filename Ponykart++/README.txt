# Dependencies : 
- Ogre 1.8.1
-- OIS
- Bullet2 (Dynamics, Collision, LinearMath, FileLoader, WorldImporter) >= 2.81-r2613
- Boost >= 1.55.0
- Lua >= 5.1
- OpenAL 1.1 OR OpenAL-Soft 1.15.1
- libogg >= 1.3.1
- libopus >= 1.1
- libopusfile >= 0.5
- libsndfile
- libvorbis >= 1.3.4

# Build instructions :
- Checkout a branch. master should always work, dev may not always compile successfully.
- Install the dependencies (use the same compiler toolchain for the dependencies and the project)
- Compile the project, you can generate project files using premake5.
- Copy the media/ folder in the build/ folder
- Run from the build/ folder

## Windows :
All windows libraries this project depends on MUST be linked against the Static CRT (/MT or /MTd).
They MUST use the Visual Studio 2013 (v120) toolset.
They SHOULD NOT use /ZI in any configuration, but SHOULD use /Zi instead.
The use of precompiled SDKs is not recommended, build from source instead.
Failure to follow these instruction will result in cryptic errors at run-time and (hopefully) compile-time.
- Compile all the dependencies with MSVC 2013
- Install them in C:\ in their respective folders or in the default locations (as defined in premake5.lua)
- Create the project files with premake5 (run the command "premake5 vs2013")
- Open the Visual Studio project, build and run.

## Linux :
- Install the dependencies with your package manager, or manually in /usr/local/
- Generate project files with premake5, build and run.
