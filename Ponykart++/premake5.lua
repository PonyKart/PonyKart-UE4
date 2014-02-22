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
		flags { "Symbols", "Optimize" }
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
		}
	configuration { "MacOSX" }
		buildoptions "-std=c++11"
	configuration { "Windows" }
		includedirs {
			-- Add windows includes here
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
				"BulletSoftBody",
				"BulletWorldImporter",
				"LinearMath",
				"openal",
				"sndfile",
				"vorbisfile",
				"vorbis",
				"ogg",
				"boost_system",
			}
			linkoptions  "-lluajit-5.1" --workaround for issue #42
		configuration { "MacOSX" }
		configuration { "Windows" }
			links {
				-- Add windows libraries here
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

