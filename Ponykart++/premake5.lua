solution "Ponykart++"
	configurations { "Debug", "Release" }
	platforms { "Unix", "MacOSX", "Windows" }
	location "../build/"
	startproject "Ponykart++"

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
	}


	configuration { "Debug" }
		flags { "Symbols" }
	configuration { "Release" }
		flags { "Symbols", "Optimize" }
	configuration { "Linux" }
		buildoptions "-std=c++11"
	configuration { "MacOSX" }
		buildoptions "-std=c++11"
	configuration {}


	project "Ponykart++"
		kind "WindowedApp"
		location "../build/"

		files {
			"Actors/Components/BillboardSetComponent.cpp",
			"Actors/Components/BillboardSetComponent.h",
			"Actors/Components/ModelComponent.cpp",
			"Actors/Components/ModelComponent.h",
			"Actors/Components/RibbonComponent.cpp",
			"Actors/Components/RibbonComponent.h",
			"Actors/Components/ShapeComponent.cpp",
			"Actors/Components/ShapeComponent.h",
			"Actors/Components/SoundComponent.cpp",
			"Actors/Components/SoundComponent.h",
			"Actors/DashJavelin.cpp",
			"Actors/DashJavelin.h",
			"Actors/Driver.cpp",
			"Actors/Driver.h",
			"Actors/InstancedGeometryManager.cpp",
			"Actors/InstancedGeometryManager.h",
			"Actors/Kart.cpp",
			"Actors/Kart.h",
			"Actors/LThing.cpp",
			"Actors/LThing.h",
			"Actors/StaticGeometryManager.cpp",
			"Actors/StaticGeometryManager.h",
			"Actors/TwiCutlass.cpp",
			"Actors/TwiCutlass.h",
			"Actors/Wheels/DriftState.h",
			"Actors/Wheels/Wheel.cpp",
			"Actors/Wheels/Wheel.h",
			"Actors/Wheels/WheelFactory.cpp",
			"Actors/Wheels/WheelFactory.h",
			"Actors/Wheels/WheelID.h",
			"BMP/EasyBMP.cpp",
			"BMP/EasyBMP.h",
			"BMP/EasyBMP_BMP.h",
			"BMP/EasyBMP_DataStructures.h",
			"Core/Animation/AnimationBlender.cpp",
			"Core/Animation/AnimationBlender.h",
			"Core/Animation/AnimationManager.cpp",
			"Core/Animation/AnimationManager.h",
			"Core/Cameras/CameraManager.cpp",
			"Core/Cameras/CameraManager.h",
			"Core/Cameras/KnightyCamera.h",
			"Core/Cameras/LCamera.cpp",
			"Core/Cameras/LCamera.h",
			"Core/Cameras/PlayerCamera.h",
			"Core/ControllerManager.h",
			"Core/IDs.cpp",
			"Core/IDs.h",
			"Core/InputMain.cpp",
			"Core/InputMain.h",
			"Core/InputSwallowerManager.cpp",
			"Core/InputSwallowerManager.h",
			"Core/KeyBindingManager.cpp",
			"Core/KeyBindingManager.h",
			"Core/Options.cpp",
			"Core/Options.h",
			"Core/OptionsEnums.h",
			"Core/Pauser.cpp",
			"Core/Pauser.h",
			"Core/Settings.h",
			"Core/Spawner.cpp",
			"Core/Spawner.h",
			"Core/main.cpp",
			"Core/main.h",
			"Kernel/LKernel.cpp",
			"Kernel/LKernel.h",
			"Kernel/LKernelOgre.cpp",
			"Kernel/LKernelOgre.h",
			"Kernel/LKernelRegistration.cpp",
			"Levels/Level.cpp",
			"Levels/Level.h",
			"Levels/LevelChangeRequest.h",
			"Levels/LevelChangedEventArgs.cpp",
			"Levels/LevelChangedEventArgs.h",
			"Levels/LevelManager.cpp",
			"Levels/LevelManager.h",
			"Levels/LevelType.h",
			"Lua/LuaMain.cpp",
			"Lua/LuaMain.h",
			"Physics/CollisionGroups.h",
			"Physics/CollisionObjectDataHolder.cpp",
			"Physics/CollisionObjectDataHolder.h",
			"Physics/CollisionReports/CollisionReporter.cpp",
			"Physics/CollisionReports/CollisionReporter.h",
			"Physics/CollisionShapeManager.cpp",
			"Physics/CollisionShapeManager.h",
			"Physics/Materials/PhysicsMaterial.cpp",
			"Physics/Materials/PhysicsMaterial.h",
			"Physics/Materials/PhysicsMaterialFactory.cpp",
			"Physics/Materials/PhysicsMaterialFactory.h",
			"Physics/MotionStates/OgreMotionState.cpp",
			"Physics/MotionStates/OgreMotionState.h",
			"Physics/PhysicsMain.cpp",
			"Physics/PhysicsMain.h",
			"Players/Player.cpp",
			"Players/Player.h",
			"Players/PlayerManager.cpp",
			"Players/PlayerManager.h",
			"Sound/SoundMain.cpp",
			"Sound/SoundMain.h",
			"UI/LuaConsoleManager.cpp",
			"UI/LuaConsoleManager.h",
			"UI/Splash.cpp",
			"UI/Splash.h",
		}

		includedirs {
			".",
			"../PonykartParsers++/"
		}

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
			"boost_system",
		}
		linkoptions  "-lluajit-5.1"


	project "PonykartParsers++"
		kind "StaticLib"
		location "../build/"

		files {
			"../PonykartParsers++/Misc/bulletExtensions.cpp",
			"../PonykartParsers++/Misc/bulletExtensions.h",
			"../PonykartParsers++/Misc/direntSearch.cpp",
			"../PonykartParsers++/Misc/direntSearch.h",
			--"../PonykartParsers++/Misc/irrklangExtensions.cpp",
			--"../PonykartParsers++/Misc/irrklangExtensions.h",
			"../PonykartParsers++/Misc/ogreExtensions.cpp",
			"../PonykartParsers++/Misc/ogreExtensions.h",
			"../PonykartParsers++/Muffin/MuffinDefinition.cpp",
			"../PonykartParsers++/Muffin/MuffinDefinition.h",
			"../PonykartParsers++/Muffin/MuffinParser.cpp",
			"../PonykartParsers++/Muffin/MuffinParser.h",
			"../PonykartParsers++/Muffin/TokenHolder.cpp",
			"../PonykartParsers++/Muffin/TokenHolder.h",
			"../PonykartParsers++/Thing/Blocks/BillboardBlock.cpp",
			"../PonykartParsers++/Thing/Blocks/BillboardBlock.h",
			"../PonykartParsers++/Thing/Blocks/BillboardSetBlock.cpp",
			"../PonykartParsers++/Thing/Blocks/BillboardSetBlock.h",
			"../PonykartParsers++/Thing/Blocks/ModelBlock.cpp",
			"../PonykartParsers++/Thing/Blocks/ModelBlock.h",
			"../PonykartParsers++/Thing/Blocks/RibbonBlock.cpp",
			"../PonykartParsers++/Thing/Blocks/RibbonBlock.h",
			"../PonykartParsers++/Thing/Blocks/ShapeBlock.cpp",
			"../PonykartParsers++/Thing/Blocks/ShapeBlock.h",
			"../PonykartParsers++/Thing/Blocks/SoundBlock.cpp",
			"../PonykartParsers++/Thing/Blocks/SoundBlock.h",
			"../PonykartParsers++/Thing/Blocks/ThingBlock.cpp",
			"../PonykartParsers++/Thing/Blocks/ThingBlock.h",
			"../PonykartParsers++/Thing/ThingDatabase.cpp",
			"../PonykartParsers++/Thing/ThingDatabase.h",
			"../PonykartParsers++/Thing/ThingDefinition.cpp",
			"../PonykartParsers++/Thing/ThingDefinition.h",
			"../PonykartParsers++/Thing/ThingEnum.cpp",
			"../PonykartParsers++/Thing/ThingEnum.h",
			"../PonykartParsers++/Thing/ThingImporter.cpp",
			"../PonykartParsers++/Thing/ThingImporter.h",
			"../PonykartParsers++/Thing/ThingParser.cpp",
			"../PonykartParsers++/Thing/ThingParser.h",
			"../PonykartParsers++/WindowsCompat/dirent.h",
		}

		includedirs { "../PonykartParsers++/" }

