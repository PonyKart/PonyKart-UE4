#include "pch.h"
#include <vector>
#include <OgreMesh.h>
#include <BulletCollision/CollisionShapes/btTriangleMesh.h>
#include "Kernel/LKernelOgre.h"
#include "Misc/bulletExtensions.h"
#include "Physics/OgreToBulletMesh.h"

using namespace Ogre;
using namespace Extensions;
using namespace Ponykart::LKernel;
using namespace Ponykart::Physics;

btTriangleMesh* OgreToBulletMesh::convert(MeshPtr mesh, const Vector3& pos,
	const Quaternion& orientation, const Vector3& scale)
{
	// get our two main objects
	btTriangleMesh* bulletMesh = new btTriangleMesh(true, false);

	LKernel::log("[Loading] Converting " + mesh->getName() + " to a BulletSharp.TriangleMesh");

	unsigned vertex_count, index_count;
	std::vector<Vector3> vertices;
	std::vector<unsigned> indices;

	getMeshInformation(mesh, vertex_count, vertices, index_count, indices, pos, orientation, scale);

	bulletMesh->preallocateIndices((int)index_count);
	bulletMesh->preallocateVertices((int)vertex_count);
	//bulletMesh.WeldingThreshold = 0.1f;

	for (int a = 0; a < index_count; a += 3) {
		bulletMesh->addTriangle(toBtVector3(vertices[indices[a]]), toBtVector3(vertices[indices[a + 1]]), 
								toBtVector3(vertices[indices[a + 2]]), true);
	}

	return bulletMesh;
}

void OgreToBulletMesh::getMeshInformation(MeshPtr mesh, unsigned& vertex_count, std::vector<Vector3>& vertices,
									unsigned& index_count, std::vector<unsigned>& indices, const Vector3& position,
									const Quaternion& orientation, const Vector3& scale)
{
	bool added_shared = false;
	unsigned current_offset = 0;
	unsigned shared_offset = 0;
	unsigned next_offset = 0;
	unsigned index_offset = 0;

	vertex_count = index_count = 0;

	for (uint16_t i = 0; i < mesh->getNumSubMeshes(); ++i) 
	{
		SubMesh* submesh = mesh->getSubMesh(i);
		if (submesh->useSharedVertices) 
		{
			if (!added_shared) 
			{
				vertex_count += mesh->sharedVertexData->vertexCount;
				added_shared = true;
			}
		}
		else
			vertex_count += submesh->vertexData->vertexCount;

		index_count += submesh->indexData->indexCount;
	}

	vertices.clear();
	vertices.reserve(vertex_count);
	indices.clear();
	indices.reserve(index_count);
	added_shared = false;

	for (uint16_t i = 0; i < mesh->getNumSubMeshes(); ++i) 
	{
		SubMesh* submesh = mesh->getSubMesh(i);
		VertexData* vertex_data = submesh->useSharedVertices ? mesh->sharedVertexData : submesh->vertexData;

		if (!submesh->useSharedVertices || (submesh->useSharedVertices && !added_shared)) 
		{
			if (submesh->useSharedVertices) 
			{
				added_shared = true;
				shared_offset = current_offset;
			}

			const VertexElement* posElem = vertex_data->vertexDeclaration->findElementBySemantic(VertexElementSemantic::VES_POSITION);
			HardwareVertexBufferSharedPtr vbuf = vertex_data->vertexBufferBinding->getBuffer(posElem->getSource());

			uint8_t* vertex = (uint8_t*)vbuf->lock(HardwareBuffer::LockOptions::HBL_READ_ONLY);
			float* pReal;

			for (int j = 0; j < vertex_data->vertexCount; ++j, vertex += vbuf->getVertexSize()) 
			{
				posElem->baseVertexPointerToElement(vertex, &pReal);
				Vector3 pt(pReal[0], pReal[1], pReal[2]);
				vertices[current_offset + j] = (orientation * (pt * scale)) + position;
			}
			vbuf->unlock();
			next_offset += vertex_data->vertexCount;
		}

		IndexData* index_data = submesh->indexData;
		uint numTris = index_data->indexCount / 3;
		HardwareIndexBufferSharedPtr ibuf = index_data->indexBuffer;

		bool use32bitindexes = (ibuf->getType() == HardwareIndexBuffer::IndexType::IT_32BIT);

		uint* pLong = (uint*)ibuf->lock(HardwareBuffer::LockOptions::HBL_READ_ONLY);
		ushort* pShort = (ushort*)pLong;
		uint offset = submesh->useSharedVertices ? shared_offset : current_offset;
		if (use32bitindexes)
			for (int k = 0; k < index_data->indexCount; ++k)
				indices[index_offset++] = pLong[k] + offset;
		else
			for (int k = 0; k < index_data->indexCount; ++k)
				indices[index_offset++] = (uint)pShort[k] + (uint)offset;
		ibuf->unlock();
		current_offset = next_offset;
	}
}

/// Give it an entity and it'll create a BulletSharp.TriangleMesh out of it
/// @param ent The entity to convert. It'll grab its mesh and use all of its submeshes
/// @param nodeThe node the entity is attached to. We aren't modifying it, but we'll use its transforms
/// @return A bullet trimesh
btTriangleMesh* OgreToBulletMesh::convert(Entity* ent, SceneNode* node)
{
	return convert(ent->getMesh(), node->_getDerivedPosition(), node->_getDerivedOrientation(), node->_getDerivedScale());
}