cd "Ponykart\bin\x86\Debug\"
del Mogre.dll OgreMain.dll OgrePaging.dll OgreRTShaderSystem.dll OgreTerrain.dll
cd "media\plugins\"
del cg.dll Plugin_CgProgramManager.dll Plugin_OctreeSceneManager.dll Plugin_ParticleFX.dll RenderSystem_Direct3D9.dll plugins.cfg
cd ..\..\..\..\..\..\
xcopy "References\Debug\*" "Ponykart\bin\x86\Debug\" /S /C /Q /Y