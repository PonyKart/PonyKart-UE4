#ifndef LCAMERA_H_INCLUDED
#define LCAMERA_H_INCLUDED

#include <string>

namespace Ogre
{
	struct FrameEvent;
	class Camera;
	class SceneNode;
}

namespace Ponykart
{
namespace Core
{
/// All camera classes should subclass from this.
class LCamera
{
public:
	LCamera(const std::string& Name);
	// Getters
	const Ogre::Camera* const getCamera() const;
	const Ogre::SceneNode* const getCameraNode() const;
	std::string getName() const;

protected:
	virtual bool updateCamera(Ogre::FrameEvent* evt);

protected:
	Ogre::Camera* camera; ///< The Ogre camera we're manipulating
	Ogre::SceneNode* cameraNode; ///< The scene node the camera is attached to
	std::string name; ///< The name of this camera
	bool isActive; ///< Is this camera active or not?
};
} // Core
} // Ponykart

#endif // LCAMERA_H_INCLUDED
