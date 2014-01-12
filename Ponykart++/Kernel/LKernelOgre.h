#include <string>

namespace Ponykart
{
namespace LKernel
{
	/// Implementation details that are not part of the interface.
	namespace details
	{
		void initOgreResources(); ///< Adds all of the resource locations but doesn't actually load anything.
		void loadOgreResourceGroups(); ///< Loads the resources into memory.
	} // details

	void initOgreRoot();
	void initOgreRenderSystem();
	void initOgreRenderWindow();
	void initOgreSceneManager();
	void initOgreViewportCam();

	void log(const std::string& message); ///< Logs a message with Ogre. Ogre must be initialized.
} // LKernel
} // Ponykart
