using System;
using BulletSharp;
using Mogre;

namespace Ponykart.Physics {
	/// <summary>
	/// Extensions for physics objects
	/// </summary>
	static class PhysicsExtensions {
		#region CollisionObject

		/// <summary>
		/// Helper method for getting a name out of a collision object.
		/// </summary>
		/// <returns>The name of the collision object if it has one</returns>
		public static string GetName(this CollisionObject obj) {
			CollisionObjectDataHolder holder = obj.UserObject as CollisionObjectDataHolder;
			if (holder != null) {
				return holder.Name;
			}
			throw new ArgumentException("This collision object does not have a CollisionObjectDataHolder associated with it!", "obj");
		}

		/// <summary>
		/// Helper method for getting a collision group from a collision object
		/// </summary>
		/// <returns>The group of the collision object if it has one</returns>
		public static PonykartCollisionGroups GetCollisionGroup(this CollisionObject obj) {
			CollisionObjectDataHolder holder = obj.UserObject as CollisionObjectDataHolder;
			if (holder != null) {
				return holder.CollisionGroup;
			}
			throw new ArgumentException("This collision object does not have a CollisionObjectDataHolder associated with it!", "obj");
		}

		// -------------------------------------------------------------------------

		/// <summary>
		/// Sets the orientation of this CollisionObject. This involves a bunch of matrix and quaternion stuff, so only use this if it's really necessary!
		/// </summary>
		/// <param name="newOrient"></param>
		public static void SetOrientation(this CollisionObject obj, Quaternion newOrient) {
			Matrix4 mat = new Matrix4(newOrient);
			// this avoids having to do GetTrans() and SetTrans(), which both do calculations that we don't want.
			mat[0, 3] = obj.WorldTransform[0, 3];
			mat[1, 3] = obj.WorldTransform[1, 3];
			mat[2, 3] = obj.WorldTransform[2, 3];
			mat[3, 3] = obj.WorldTransform[3, 3];
			// update our body
			obj.WorldTransform = mat;
		}
		#endregion

		#region World
		public static void AddRigidBody(this DynamicsWorld world, RigidBody body, PonykartCollisionGroups collisionGroup, PonykartCollidesWithGroups collidesWith) {
			world.AddRigidBody(body, collisionGroup.ToBullet(), collidesWith.ToBullet());
		}

		public static void AddCollisionObject(this DynamicsWorld world, CollisionObject obj, PonykartCollisionGroups collisionGroup, PonykartCollidesWithGroups collidesWith) {
			world.AddCollisionObject(obj, collisionGroup.ToBullet(), collidesWith.ToBullet());
		}
		#endregion
	}
}
