#include <OgreMesh.h>

class btTriangleMesh;

namespace Ponykart
{
namespace Physics
{
	/// Class to convert a ogre mesh to a bullet mesh
	class OgreToBulletMesh
	{
	public:
		static btTriangleMesh* convert(Ogre::MeshPtr mesh, const Ogre::Vector3& pos,
									const Ogre::Quaternion& orientation, const Ogre::Vector3& scale);
		static btTriangleMesh* convert(Ogre::Entity* ent, Ogre::SceneNode* node);
	};
}
}