using System.Linq;
using BulletSharp;
using Mogre;

namespace Ponykart.Physics {
	/// <summary>
	/// Class to convert a ogre mesh to a bullet mesh
	/// </summary>
	public class OgreToBulletMesh {

		
		/// <summary>
		/// Give it a mesh and it'll create a BulletSharp.TriangleMesh out of it
		/// </summary>
		/// <param name="mesh">The mesh you're converting</param>
		/// <returns>A bullet trimesh</returns>
		public static TriangleMesh Convert(MeshPtr mesh, Vector3 pos, Quaternion orientation, Vector3 scale) {

			// get our two main objects
			TriangleMesh BulletMesh = new TriangleMesh(true, false);

			Launch.Log("[Loading] Converting " + mesh.Name + " to a BulletSharp.TriangleMesh");

			uint vertex_count = default(uint);
			Vector3[] vertices = default(Vector3[]);
			uint index_count = default(uint);
			uint[] indices = default(uint[]);

			GetMeshInformation(mesh, ref vertex_count, ref vertices, ref index_count, ref indices, pos, orientation, scale);

			BulletMesh.PreallocateIndexes((int) index_count);
			BulletMesh.PreallocateVertices((int) vertex_count);
			//BulletMesh.WeldingThreshold = 0.1f;

			for (int a = 0; a < index_count; a += 3) {
				BulletMesh.AddTriangle(vertices[indices[a]], vertices[indices[a + 1]], vertices[indices[a + 2]], true);
			}

			return BulletMesh;
		}

		public static ConvexHullShape ConvertToHull(MeshPtr mesh, Vector3 pos, Quaternion orientation, Vector3 scale) {

			Launch.Log("[Loading] Converting " + mesh.Name + " to a BulletSharp.ConvexHull");

			uint vertex_count = default(uint);
			Vector3[] vertices = default(Vector3[]);
			uint index_count = default(uint);
			uint[] indices = default(uint[]);

			GetMeshInformation(mesh, ref vertex_count, ref vertices, ref index_count, ref indices, pos, orientation, scale);

			ConvexHullShape hull = new ConvexHullShape(vertices.Distinct().ToArray());

			return hull;
		}



		/// <summary>
		/// Give it an entity and it'll create a BulletSharp.TriangleMesh out of it
		/// </summary>
		/// <param name="ent">The entity to convert. It'll grab its mesh and use all of its submeshes</param>
		/// <param name="node">The node the entity is attached to. We aren't modifying it, but we'll use its transforms</param>
		/// <returns>A bullet trimesh</returns>
		public static TriangleMesh Convert(Entity ent, SceneNode node) {
			return Convert(ent.GetMesh(), node._getDerivedPosition(), node._getDerivedOrientation(), node._getDerivedScale());
		}

		/// <summary>
		/// Give it an entity and it'll create a BulletSharp.ConvexHullShape out of it
		/// </summary>
		/// <param name="ent">The entity to convert. It'll grab its mesh and use all of its submeshes</param>
		/// <param name="node">The node the entity is attached to. We aren't modifying it, but we'll use its transforms</param>
		/// <returns>A bullet trimesh</returns>
		public static ConvexHullShape ConvertToHull(Entity ent, SceneNode node) {
			return ConvertToHull(ent.GetMesh(), node._getDerivedPosition(), node._getDerivedOrientation(), node._getDerivedScale());
		}

		public unsafe static void GetMeshInformation(MeshPtr mesh, ref uint vertex_count, ref Vector3[] vertices, ref uint index_count, ref uint[] indices,
			Vector3 position, Quaternion orientation, Vector3 scale)
		{
			bool added_shared = false;
			uint current_offset = 0;
			uint shared_offset = 0;
			uint next_offset = 0;
			uint index_offset = 0;

			vertex_count = index_count = 0;

			for (ushort i = 0; i < mesh.NumSubMeshes; ++i) {
				SubMesh submesh = mesh.GetSubMesh(i);
				if (submesh.useSharedVertices) {
					if (!added_shared) {
						vertex_count += mesh.sharedVertexData.vertexCount;
						added_shared = true;
					}
				}
				else {
					vertex_count += submesh.vertexData.vertexCount;
				}

				index_count += submesh.indexData.indexCount;
			}

			vertices = new Vector3[vertex_count];
			indices = new uint[index_count];
			added_shared = false;

			for (ushort i = 0; i < mesh.NumSubMeshes; ++i) {
				SubMesh submesh = mesh.GetSubMesh(i);
				VertexData vertex_data = submesh.useSharedVertices ? mesh.sharedVertexData : submesh.vertexData;

				if (!submesh.useSharedVertices || (submesh.useSharedVertices && !added_shared)) {
					if (submesh.useSharedVertices) {
						added_shared = true;
						shared_offset = current_offset;
					}

					VertexElement posElem = vertex_data.vertexDeclaration.FindElementBySemantic(VertexElementSemantic.VES_POSITION);
					HardwareVertexBufferSharedPtr vbuf = vertex_data.vertexBufferBinding.GetBuffer(posElem.Source);

					byte* vertex = (byte*) vbuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
					float* pReal;

					for (int j = 0; j < vertex_data.vertexCount; ++j, vertex += vbuf.VertexSize) {
						posElem.BaseVertexPointerToElement(vertex, &pReal);
						Vector3 pt = new Vector3(pReal[0], pReal[1], pReal[2]);
						vertices[current_offset + j] = (orientation * (pt * scale)) + position;
					}
					vbuf.Unlock();
					vbuf.Dispose();
					next_offset += vertex_data.vertexCount;
				}

				IndexData index_data = submesh.indexData;
				uint numTris = index_data.indexCount / 3;
				HardwareIndexBufferSharedPtr ibuf = index_data.indexBuffer;

				bool use32bitindexes = (ibuf.Type == HardwareIndexBuffer.IndexType.IT_32BIT);

				uint* pLong = (uint*) ibuf.Lock(HardwareBuffer.LockOptions.HBL_READ_ONLY);
				ushort* pShort = (ushort*) pLong;
				uint offset = submesh.useSharedVertices ? shared_offset : current_offset;
				if (use32bitindexes) {
					for (int k = 0; k < index_data.indexCount; ++k) {
						indices[index_offset++] = pLong[k] + offset;
					}
				}
				else {
					for (int k = 0; k < index_data.indexCount; ++k) {
						indices[index_offset++] = (uint) pShort[k] + (uint) offset;
					}
				}
				ibuf.Unlock();
				ibuf.Dispose();
				current_offset = next_offset;
			}

		}
	}
}
