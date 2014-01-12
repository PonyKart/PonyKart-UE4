#include "Core/Cameras/LCamera.h"

namespace Ponykart
{
	namespace Core
	{
		/// A basic third-person camera with some smoothing.
		class KnightyCamera : LCamera
		{
		public:
			KnightyCamera(const std::string& name);
		};
	}
}