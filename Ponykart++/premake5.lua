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
		buildoptions "-std=c++11"
		includedirs {
			"/usr/local/include/OGRE/",
			"/usr/include/OGRE/",
			"/usr/local/include/OIS/",
			"/usr/include/OIS/",
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
			os.findlib("ogg"),
			os.findlib("openal"),
			os.findlib("boost_system"), 		
			-- Try in C:/
			"C:/OGRE/lib/release",
			"C:/BULLET_PHYSICS/lib/Release",
			"C:/Lua/5.1/lib",
			"C:/irrKlang-1.4.0/lib/Win32-visualStudio",
			"C:/OpenAL1.1/libs/Win64",
			"C:/OpenAL1.1/libs/Win32",
			"C:/libsndfile32/lib",
			"C:/libsndfile64/lib",
			"C:/libvorbis-1.3.4/lib/x32",
			"C:/libvorbis-1.3.4/lib/x64",
			"C:/libogg-1.3.1/lib/x32",
			"C:/libogg-1.3.1/lib/x64",
		}
		includedirs {
			"C:/Lua/5.1/include",
			"C:/OGRE/include/OGRE",
			"C:/OGRE/include/OIS",
			"C:/irrKlang-1.4.0/include",
			"C:/BULLET_PHYSICS/include/bullet",
			"C:/OpenAL1.1/include",
			"C:/libvorbis-1.3.4/include/vorbis",
			"C:/libvorbis-1.3.4/include",
			"C:/libogg-1.3.1/include/ogg",
			"C:/libogg-1.3.1/include",
			"C:/libsndfile32/include",
			"C:/libsndfile64/include",
		}
	configuration {}


	project "Ponykart++"
		kind "WindowedApp"
		location "../build/"

		files { "**.cpp", "**.h" }

		includedirs {
			".",
			"../PonykartParsers++/"
		}

		configuration { "Unix" }
			links {
				"PonykartParsers++",
				"OgreMain",
				"OIS",
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
			linkoptions  "-lluajit-5.1"
		configuration { "MacOSX" }
		configuration { "Windows" }
			flags { "StaticRuntime" } -- We need the /MT CRT, since that's what our dependencies are using
			buildoptions { "/W0" } -- Disable warnings for speed. Visual generates ~2500 warnings for Ogre only. 
			links {
				"PonykartParsers++",
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
			}
		configuration {}


	project "PonykartParsers++"
		kind "StaticLib"
		location "../build/"

		files {
			"../PonykartParsers++/**.cpp",
			"../PonykartParsers++/**.h",
		}

		includedirs { "../PonykartParsers++/" }
		
		configuration { "Windows" }
			flags { "StaticRuntime" } -- We need the /MT CRT, since that's what our dependencies are using
			buildoptions { "/W0" } -- Disable warnings for speed. Visual generates ~2500 warnings for Ogre only. 

