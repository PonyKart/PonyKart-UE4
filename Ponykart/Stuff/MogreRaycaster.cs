using Mogre;
using Ponykart.Physics;

namespace Ponykart.Stuff {
	public class MogreRaycaster : LDisposable {
		RaySceneQuery raySceneQuery;

		public MogreRaycaster() {
			var sceneMgr = LKernel.GetG<SceneManager>();

			// create the ray scene query object
			raySceneQuery = sceneMgr.CreateRayQuery(new Ray()/*, SceneManager.WORLD_GEOMETRY_TYPE_MASK*/);
			if (raySceneQuery != null) {
				raySceneQuery.SetSortByDistance(true);
			}
		}

		// raycast from a point in to the scene.
		// returns success or failure.
		// on success the point is returned in the result.
		public bool RaycastFromPoint(Vector3 point, Vector3 normal, ref Vector3 result, ref Vector3 resNormal) {
			// create the ray to test
			Ray ray = new Ray(point, normal);

			// check we are initialised
			if (raySceneQuery != null) {
				// create a query object
				raySceneQuery.Ray = ray;

				// execute the query, returns a vector of hits
				RaySceneQueryResult rayresult = raySceneQuery.Execute();
				if (rayresult.Count <= 0) {
					rayresult.Dispose();
					// raycast did not hit an objects bounding box
					return false;
				}

				rayresult.Dispose();
			}
			else {
				return false;
			}

			// at this point we have raycast to a series of different objects bounding boxes.
			// we need to test these different objects to see which is the first polygon hit.
			// there are some minor optimizations (distance based) that mean we wont have to
			// check all of the objects most of the time, but the worst case scenario is that
			// we need to test every triangle of every object.
			float closest_distance = -1.0f;
			Vector3 closest_result = Vector3.ZERO;
			Vector3 vNormal = Vector3.ZERO;
			RaySceneQueryResult query_result = raySceneQuery.GetLastResults();

			foreach (RaySceneQueryResultEntry this_result in query_result) {
				// stop checking if we have found a raycast hit that is closer
				// than all remaining entities
				if ((closest_distance >= 0.0f) &&
					(closest_distance < this_result.distance)) {
					break;
				}

				// only check this result if its a hit against an entity
				if ((this_result.movable != null) && (this_result.movable.MovableType == "Entity")) {
					// get the entity to check
					Entity pentity = (Entity) this_result.movable;

					// mesh data to retrieve 
					uint vertex_count = 0;
					uint index_count = 0;
					Vector3[] vertices = new Vector3[0];
					uint[] indices = new uint[0];

					// get the mesh information
					OgreToBulletMesh.GetMeshInformation(pentity.GetMesh(),
						ref vertex_count, ref vertices, ref index_count, ref indices,
						pentity.ParentNode._getDerivedPosition(),    // WorldPosition
						pentity.ParentNode._getDerivedOrientation(), // WorldOrientation
						pentity.ParentNode.GetScale());

					int ncf = -1; // new_closest_found

					// test for hitting individual triangles on the mesh
					for (int i = 0; i < (int) index_count; i += 3) {
						// check for a hit against this triangle
						Pair<bool, float> hit = Mogre.Math.Intersects(ray, vertices[indices[i]],
							vertices[indices[i + 1]], vertices[indices[i + 2]], true, false);

						// if it was a hit check if its the closest
						if (hit.first) {
							if ((closest_distance < 0.0f) ||
								(hit.second < closest_distance)) {
								// this is the closest so far, save it off
								closest_distance = hit.second;
								ncf = i;
							}
						}
					}

					if (ncf > -1) {
						closest_result = ray.GetPoint(closest_distance);
						// if you don't need the normal, comment this out; you'll save some CPU cycles.
						Vector3 v1 = vertices[indices[ncf]] - vertices[indices[ncf + 1]];
						Vector3 v2 = vertices[indices[ncf + 2]] - vertices[indices[ncf + 1]];
						vNormal = v1.CrossProduct(v2);
					}

					// free the verticies and indicies memory
					vertices = null;
					indices = null;
				}
			}

			query_result.Dispose();

			// if we found a new closest raycast for this object, update the
			// closest_result before moving on to the next object.
			if (closest_distance >= 0.0f) {
				result = new Vector3(closest_result.x, closest_result.y, closest_result.z);
				resNormal = vNormal / vNormal.Normalise();

                
                // //this visualizes the 'result' position 
                //if (!sceneMgr.HasSceneNode("marker"))
                //{
                //    SceneNode node = sceneMgr.CreateSceneNode("marker");
                //    Entity ent = sceneMgr.CreateEntity("marker", "Cube.mesh");
                //    node.AttachObject(ent);
                //    node.Position = result;
                //    node.Scale(0.25f, 0.25f, 0.25f);
                //    sceneMgr.RootSceneNode.AddChild(node);
                //}
                //else
                //{
                //    sceneMgr.GetSceneNode("marker").Position = result;
                //}
                

				// raycast success
				return true;
			}
			else {
				// raycast failed
				return false;
			}
		} // RayCastFromPoint

		protected override void Dispose(bool disposing) {
			if (IsDisposed)
				return;

			raySceneQuery.Dispose();

			base.Dispose(disposing);
		}
	}
}
