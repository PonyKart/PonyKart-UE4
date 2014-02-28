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
- Install the dependencies (use the same compiler for all the dependencies and the project)
  The dependencies must compiled for a 32bit architecture, because irrKlang is 32bit only.
- Compile the project, a Visual Studio 2013 and Code::Blocks project files are provided for Windows and Linux
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
- Open the Visual Studio project, build and run.

## Linux :
If you use a package manager, installing the 32bit dependencies on a 64bit system is tricky (but possible).
It's easier to either use a 32bit kernel, or compile the dependencies yourself.
- Install the dependencies with your package manager, or manually in /usr/local/
- Open the Code::Blocks project, build and run.
