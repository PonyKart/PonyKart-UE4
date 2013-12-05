#ifndef LCAMERA_H_INCLUDED
#define LCAMERA_H_INCLUDED

#include <string>
#include <Ogre.h>

namespace Ponykart
{
namespace Core
{
// All camera classes should subclass from this.
class LCamera
{
public:
	LCamera(std::string name);
	// Getters
	const Ogre::Camera* const getCamera();
	const Ogre::SceneNode* const getCameraNode();
	std::string getName();

protected:
	Ogre::Camera* Camera; // The Ogre camera we're manipulating
	Ogre::SceneNode* CameraNode; // The scene node the camera is attached to
	std::string Name; // The name of this camera
	bool IsActive; // Is this camera active or not?
};
} // Core
} // Ponykart

#endif // LCAMERA_H_INCLUDED
