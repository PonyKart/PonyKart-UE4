#ifndef SPLASH_H_INCLUDED
#define SPLASH_H_INCLUDED

#include <string>

namespace Ogre
{
	class Overlay;
	class OverlayContainer;
	class OverlayManager;
	class TextAreaOverlayElement;
} // Ogre

namespace Ponykart
{
	/// Manages the splash/loading screen during startup.
	class Splash
	{
	public:
		explicit Splash();
		~Splash();
		void updateGUI(); ///< Render a frame and process the window's message queue. Must be done periodically.
		void increment(const std::string& text);

	private:
		void createScene();

	private:
		Ogre::Overlay* overlay;
		Ogre::TextAreaOverlayElement* progressText; ///< Text in the progress bar. Should describe what is currently loading.
		Ogre::OverlayContainer* progressBG;
		Ogre::OverlayContainer* progressFG;
		Ogre::OverlayManager* overlayManager;
		static int current; ///< Progress bar internal counter
		static const int maximum; ///< Maximum for the progress bar's internal counter.
	};
}

#endif // SPLASH_H_INCLUDED
