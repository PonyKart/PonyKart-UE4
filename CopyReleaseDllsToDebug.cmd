cd "Ponykart\bin\x86\Debug\"
del Mogre.dll *_d.dll
cd "media\plugins\"
del cg.dll Plugin_CgProgramManager_d.dll Plugin_OctreeSceneManager_d.dll Plugin_ParticleFX_d.dll RenderSystem_Direct3D9_d.dll plugins.cfg
cd ..\..\..\..\..\..\
xcopy "References\Release\*" "Ponykart\bin\x86\Debug\" /S /C /Q /Y