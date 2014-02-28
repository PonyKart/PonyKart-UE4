#include "pch.h"
#include "Core/Cameras/CameraManager.h"

using namespace Ponykart::Core;

CameraManager::CameraManager()
{
	// TODO: Implement thoses
	//LevelManager.OnLevelLoad += new LevelEvent(OnLevelLoad);
	//LevelManager.OnLevelUnload += new LevelEvent(OnLevelUnload);
}

const LCamera* const CameraManager::getCurrentCamera()
{
	return currentCamera;
}
