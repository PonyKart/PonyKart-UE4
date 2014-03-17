solution "Ponykart++"
	configurations { "Debug", "Release" }
	platforms { "Unix", "MacOSX", "Windows" }
	location "../build/"
	startproject "Ponykart++"


	configuration { "Debug" }
		defines { "DEBUG" }
		flags { "Symbols" }
		targetsuffix "-d"
	configuration { "Release" }
		flags { "Symbols" }
		optimize "Speed"
	configuration {}

	configuration { "Unix" }
		buildoptions "-std=c++11 -pthread"
		includedirs {
			"/usr/local/include/SDL2/",
			"/usr/include/SDL2/",
			"/usr/local/include/OGRE/",
			"/usr/include/OGRE/",
			"/usr/local/include/bullet/",
			"/usr/include/bullet/",
			"/usr/local/include/luajit-2.0/",
			"/usr/include/luajit-2.0/",
			"/usr/local/include/AL",
			"/usr/include/AL",
			"/usr/local/include/vorbis",
			"/usr/include/vorbis",
			"/usr/local/include/opus",
			"/usr/include/opus",
		}
	configuration { "MacOSX" }
		buildoptions "-std=c++11"
		
	configuration { "Windows" }
		libdirs { 
			-- Try the default paths
			os.findlib("OgreMain"), 
			os.findlib("OIS"), 
			os.findlib("BulletDynamics"), 
			os.findlib("BulletCollision"), 
			os.findlib("BulletFileLoader"), 
			os.findlib("BulletWorldImporter"), 
			os.findlib("LinearMath"), 
			os.findlib("lua5.1"), 
			os.findlib("vorbis"),
			os.findlib("vorbisfile"),
			os.findlib("sndfile"),
			os.findlib("opus"), 		
			os.findlib("opusfile"), 		
			os.findlib("ogg"),
			os.findlib("openal-soft"),
			os.findlib("boost_system"), 		
			-- Try in C:/
			"C:/Lua/5.1/lib",
			"C:/irrKlang-1.4.0/lib/Win32-visualStudio",
			"C:/libsndfile32/lib",
			"C:/libsndfile64/lib",
		}
		includedirs {
			"C:/SDL2/include/SDL2",
			"C:/Lua/5.1/include",
			"C:/OGRE/include/OGRE",
			"C:/OGRE/include/OIS",
			"C:/irrKlang-1.4.0/include",
			"C:/BULLET_PHYSICS/include/bullet",
			"C:/libvorbis-1.3.4/include/vorbis",
			"C:/libvorbis-1.3.4/include",
			"C:/libogg-1.3.1/include/ogg",
			"C:/libogg-1.3.1/include",
			"C:/libsndfile32/include",
			"C:/libsndfile64/include",
			"C:/libopus-1.1/include",
			"C:/libopusfile-0.5/include",
			-- Creative's OpenAL is dead, we'll use OpenAL-soft as a drop-in replacement.
			--"C:/OpenAL1.1/include", 
			"C:/openal-soft-1.15.1-bin/include/AL/",
		}
	configuration { "Windows", "Debug" }	
		libdirs { 
			"C:/SDL2/lib/Debug",
			"C:/OGRE/lib/debug",
			"C:/BULLET_PHYSICS/lib/Debug",
			"C:/libvorbis-1.3.4/lib/Win32/Debug",
			"C:/libvorbis-1.3.4/lib/x64/Debug",
			"C:/libogg-1.3.1/lib/Win32/Debug",
			"C:/libogg-1.3.1/lib/x64/Debug",
			"C:/libopus-1.1/lib/Win32/Debug",
			"C:/libopus-1.1/lib/x64/Debug",
			"C:/libopusfile-0.5/lib/Win32-Debug",
			-- Creative's OpenAL is dead, we'll use OpenAL-soft as a drop-in replacement.
			"C:/openal-soft-1.15.1-bin/lib/Win32-MSVC12-Debug",
		}
	configuration { "Windows", "Release" }	
		libdirs { 
			"C:/SDL2/lib/Release",
			"C:/OGRE/lib/release",
			"C:/BULLET_PHYSICS/lib/Release",
			"C:/libvorbis-1.3.4/lib/Win32/Release",
			"C:/libvorbis-1.3.4/lib/x64/Release",
			"C:/libogg-1.3.1/lib/Win32/Release",
			"C:/libogg-1.3.1/lib/x64/Release",
			"C:/libopus-1.1/lib/Win32/Release",
			"C:/libopus-1.1/lib/x64/Release",
			"C:/libopusfile-0.5/lib/Win32-Release",
			-- Creative's OpenAL is dead, we'll use OpenAL-soft as a drop-in replacement.
			"C:/openal-soft-1.15.1-bin/lib/Win32-MSVC12-Release",
		}
	configuration {}


	project "Ponykart++"
		kind "WindowedApp"
		location "../build/"

		files { "**.cpp", "**.h" }

		includedirs {
			"../Ponykart++/",
			"../PonykartParsers++/"
		}

		pchheader "pch.h"
		pchsource "pch.cpp"

		configuration { "Unix" }
			links {
				"PonykartParsers++",
				"SDL2",
				"OgreMain",
				"BulletDynamics",
				"BulletCollision",
				"BulletWorldImporter",
				"LinearMath",
				"openal",
				"sndfile",
				"vorbisfile",
				"vorbis",
				"opusfile",
				"opus",
				"ogg",
				"boost_system",
			}
			linkoptions  "-pthread -lluajit-5.1"
		configuration { "MacOSX" }
		configuration { "Windows" }
			buildoptions { "/W0" } -- Disable warnings for speed. Visual generates ~2500 warnings for Ogre only. 
			buildoptions { "/Zm500" } -- Increase MSVC's internal heap limit for PCHs
			links {
				"PonykartParsers++",
				"SDL2main",
				"SDL2",
				"OgreMain",
				"OIS",
				"BulletDynamics",
				"BulletCollision",
				"BulletFileLoader",
				"BulletWorldImporter",
				"LinearMath",
				"OpenAL32",
				"libsndfile-1",
				"libvorbisfile_static",
				"libvorbis_static",
				"libogg_static",
				"lua51",
				"opus",
				"celt",
				"silk_common",
				"opusfile",
			}
		configuration {}


	project "PonykartParsers++"
		kind "StaticLib"
		location "../build/"

		files {
			"../PonykartParsers++/**.cpp",
			"../PonykartParsers++/**.h",
		}

		includedirs {
			"../Ponykart++/",
			"../PonykartParsers++/"
		}
		
		configuration { "Windows" }
			buildoptions { "/W0" } -- Disable warnings for speed. Visual generates ~2500 warnings for Ogre only. 
			buildoptions { "/Zm500" } -- Increase MSVC's internal heap limit for PCHs

