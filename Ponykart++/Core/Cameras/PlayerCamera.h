#include "Core/Cameras/LCamera.h"

namespace Ponykart
{
namespace Core
{
	/// A basic third-person camera with some smoothing.
	class PlayerCamera : LCamera
	{
	public:
		PlayerCamera(const std::string& name);
	};
}
}