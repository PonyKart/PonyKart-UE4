using System;
using System.Drawing;
using System.Runtime.InteropServices;
using IrrKlang;
using Mogre;
using Ponykart.Core;
using Ponykart.Levels;
using Math = Mogre.Math;

namespace Ponykart {
	/// <summary>
	/// Some extension methods 
	/// </summary>
	static class Extensions {
		#region Vector3
		/// <summary>
		/// If you have a vector to be used for rotation but it's in degrees and you want radians, use this!
		/// </summary>
		public static Vector3 DegreeVectorToRadianVector(this Vector3 vec) {
			return new Vector3(Math.DegreesToRadians(vec.x), Math.DegreesToRadians(vec.y), Math.DegreesToRadians(vec.z));
		}

		/// <summary>
		/// If you have a vector to be used for rotation but it's in radians and you want degrees, use this!
		/// </summary>
		public static Vector3 RadianVectorToDegreeVector(this Vector3 vec) {
			return new Vector3(Math.RadiansToDegrees(vec.x), Math.RadiansToDegrees(vec.y), Math.RadiansToDegrees(vec.z));
		}

		//-----------------------------------------------------------------------------

		/// <summary>
		/// Creates a quaternion from a local degree vector.
		/// </summary>
		public static Quaternion DegreeVectorToLocalQuaternion(this Vector3 vec) {
			return new Quaternion().FromLocalEulerDegrees(vec);
		}
		/// <summary>
		/// Creates a quaternion from a global degree vector.
		/// </summary>
		public static Quaternion DegreeVectorToGlobalQuaternion(this Vector3 vec) {
			return new Quaternion().FromGlobalEulerDegrees(vec);
		}
		/// <summary>
		/// Creates a quaternion from a local radian vector.
		/// </summary>
		public static Quaternion RadianVectorToLocalQuaternion(this Vector3 vec) {
			return new Quaternion().FromLocalEuler(vec);
		}
		/// <summary>
		/// Creates a quaternion from a global degree vector.
		/// </summary>
		public static Quaternion RadianVectorToGlobalQuaternion(this Vector3 vec) {
			return new Quaternion().FromGlobalEuler(vec);
		}

		// -----------------------------------------------------------------------------

		public static Vector3D ToSoundVector(this Vector3 vec) {
			return new Vector3D(vec.x, vec.y, vec.z);
		}

		public static ColourValue ToColourValue(this Vector3 vec) {
			return new ColourValue(vec.x, vec.y, vec.z);
		}
		#endregion Vector3

		#region Quaternion
		// We have to make new quaternions because they're passed by value, not by reference
		// this thing is very useful: http://www.ogre3d.org/tikiwiki/Quaternion+and+Rotation+Primer

			#region Locals
			/// <summary>
			/// Extension method for Quaternion. Given three euler radian angles of local axes, we make a new quaternion from those angles and return it.
			/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
			/// do "Quaternion newQuat = new Quaternion().FromLocalEuler(x, y, z);"
			/// </summary>
			/// <param name="rotX">Rotation (in radians) on the local X axis</param>
			/// <param name="rotY">Rotation (in radians) on the local Y axis</param>
			/// <param name="rotZ">Rotation (in radians) on the local Z axis</param>
			/// <returns>A new quaternion</returns>
			public static Quaternion FromLocalEuler(this Quaternion quat, Radian rotX, Radian rotY, Radian rotZ) {
				return LocalEulerToQuat(rotX, rotY, rotZ);
			}

			/// <summary>
			/// Extension method for Quaternion. Given three euler radian angles from local axes, we make a new quaternion from those angles and return it.
			/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
			/// do "Quaternion newQuat = new Quaternion().FromLocalEuler(x, y, z);".
			/// Alternatively, you could do "Quaternion newQuat = myVector.ToQuaternion();".
			/// </summary>
			/// <param name="radianRotations">
			/// A vector3 representing the rotations on local axes, so that rotations.x represents
			/// the rotation on the X axis, etc. This must be in radians! Use FromLocalEulerDegrees if your vector is in degrees!
			/// </param>
			/// <returns>A new quaternion</returns>
			public static Quaternion FromLocalEuler(this Quaternion quat, Vector3 radianRotations) {
				return LocalEulerToQuat(radianRotations.x, radianRotations.y, radianRotations.z);
			}

			/// <summary>
			/// Extension method for Quaternion. Given three euler degree angles from local axes, we make a new quaternion from those angles and return it.
			/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
			/// do "Quaternion newQuat = new Quaternion().FromLocalEuler(x, y, z);".
			/// Alternatively, you could do "Quaternion newQuat = myVector.ToQuaternion();".
			/// </summary>
			/// <param name="radianRotations">
			/// A vector3 representing the rotations on local axes, so that rotations.x represents
			/// the rotation on the X axis, etc. This one is for degrees!
			/// </param>
			/// <returns>A new quaternion</returns>
			public static Quaternion FromLocalEulerDegrees(this Quaternion quat, Vector3 degreeRotations) {
				return FromLocalEuler(quat, degreeRotations.DegreeVectorToRadianVector());
			}

			/// <summary>
			/// Converts three euler angles of the local axes to an equivalent quaternion. This assumes that the given angles are in radians and not degrees.
			/// </summary>
			private static Quaternion LocalEulerToQuat(Radian rotX, Radian rotY, Radian rotZ) {
				Quaternion q1 = new Quaternion(),
						   q2 = new Quaternion(),
						   q3 = new Quaternion(),
						   q = new Quaternion();
				q1.FromAngleAxis(rotX, Vector3.UNIT_X);
				q2.FromAngleAxis(rotY, Vector3.UNIT_Y);
				q3.FromAngleAxis(rotZ, Vector3.UNIT_Z);

				// local axes
				q = q1 * q2 * q3;
				return q;
			}
			#endregion Locals

			#region Globals
			/// <summary>
			/// Extension method for Quaternion. Given three euler radian angles of global axes, we make a new quaternion from those angles and return it.
			/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
			/// do "Quaternion newQuat = new Quaternion().FromGlobalEuler(x, y, z);"
			/// </summary>
			/// <param name="rotX">Rotation (in radians) on the global X axis</param>
			/// <param name="rotY">Rotation (in radians) on the global Y axis</param>
			/// <param name="rotZ">Rotation (in radians) on the global Z axis</param>
			/// <returns>A new quaternion</returns>
			public static Quaternion FromGlobalEuler(this Quaternion quat, Radian rotX, Radian rotY, Radian rotZ) {
				return GlobalEulerToQuat(rotX, rotY, rotZ);
			}

			/// <summary>
			/// Extension method for Quaternion. Given three euler radian angles from global axes, we make a new quaternion from those angles and return it.
			/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
			/// do "Quaternion newQuat = new Quaternion().FromGlobalEuler(x, y, z);".
			/// Alternatively, you could do "Quaternion newQuat = myVector.ToQuaternion();".
			/// </summary>
			/// <param name="radianRotations">
			/// A vector3 representing the rotations on global axes, so that rotations.x represents
			/// the rotation on the X axis, etc. This must be in radians! Use FromGlobalEulerDegrees if your vector is in degrees!
			/// </param>
			/// <returns>A new quaternion</returns>
			public static Quaternion FromGlobalEuler(this Quaternion quat, Vector3 radianRotations) {
				return GlobalEulerToQuat(radianRotations.x, radianRotations.y, radianRotations.z);
			}

			/// <summary>
			/// Extension method for Quaternion. Given three euler degree angles from global axes, we make a new quaternion from those angles and return it.
			/// Keep in mind that this doesn't modify the original quaternion (since they're passed by value), so you'll need to
			/// do "Quaternion newQuat = new Quaternion().FromGlobalEuler(x, y, z);".
			/// Alternatively, you could do "Quaternion newQuat = myVector.ToQuaternion();".
			/// </summary>
			/// <param name="degreeRotations">
			/// A vector3 representing the rotations on global axes, so that rotations.x represents
			/// the rotation on the X axis, etc. This must be in degrees!
			/// </param>
			/// <returns>A new quaternion</returns>
			public static Quaternion FromGlobalEulerDegrees(this Quaternion quat, Vector3 degreeRotations) {
				return FromGlobalEuler(quat, degreeRotations.DegreeVectorToRadianVector());
			}


			/// <summary>
			/// Converts three euler angles of the global axes to an equivalent quaternion. This assumes that the given angles are in radians and not degrees.
			/// </summary>
			private static Quaternion GlobalEulerToQuat(Radian rotX, Radian rotY, Radian rotZ) {
				Quaternion q1 = new Quaternion(),
						   q2 = new Quaternion(),
						   q3 = new Quaternion(),
						   q = new Quaternion();
				q1.FromAngleAxis(rotX, Vector3.UNIT_X);
				q2.FromAngleAxis(rotY, Vector3.UNIT_Y);
				q3.FromAngleAxis(rotZ, Vector3.UNIT_Z);

				// global axes
				q = q3 * q2 * q1;
				return q;
			}
			#endregion Globals

			#region ColourValue
			/// <summary>
			/// Converts a quaternion to a colour value, mostly used for colour stuff in the .thing and .muffin files.
			/// </summary>
			/// <param name="quat">x for R, y for G, z for B, w for A</param>
			public static ColourValue ToColourValue(this Quaternion quat) {
				return new ColourValue(quat.x, quat.y, quat.z, quat.w);
			}
			#endregion

		#endregion Quaternion

		#region Matrix3

			#region Getters
			/// <summary>
			/// Gets the local X axis of the matrix.
			/// </summary>
			public static Vector3 GetLocalXAxis(this Matrix3 matrix) {
				return matrix.GetColumn(0);
			}

			/// <summary>
			/// Gets the local Y axis of the matrix.
			/// </summary>
			public static Vector3 GetLocalYAxis(this Matrix3 matrix) {
				return matrix.GetColumn(1);
			}

			/// <summary>
			/// Gets the local Z axis of the matrix.
			/// </summary>
			public static Vector3 GetLocalZAxis(this Matrix3 matrix) {
				return matrix.GetColumn(2);
			}
			#endregion Getters

			#region Setters
			/// <summary>
			/// Sets the local X axis of the matrix.
			/// </summary>
			/// <param name="vec">Radians I think</param>
			public static void SetLocalXAxis(this Matrix3 matrix, Vector3 vec) {
				matrix.SetColumn(0, vec);
			}
			/// <summary>
			/// Sets the local Y axis of the matrix.
			/// </summary>
			/// <param name="vec">radians?</param>
			public static void SetLocalYAxis(this Matrix3 matrix, Vector3 vec) {
				matrix.SetColumn(1, vec);
			}
			/// <summary>
			/// Sets the local Z axis of the matrix.
			/// </summary>
			/// <param name="vec">radians?</param>
			public static void SetLocalZAxis(this Matrix3 matrix, Vector3 vec) {
				matrix.SetColumn(2, vec);
			}
			#endregion Setters

		#endregion Matrix3

		#region Node
		/// <summary>
		/// Get the local X axis of this node, in radians.
		/// </summary>
		public static Vector3 GetLocalXAxis(this SceneNode node) {
			return node.LocalAxes.GetColumn(0);
		}

		/// <summary>
		/// Get the local Y axis of this node, in radians.
		/// </summary>
		public static Vector3 GetLocalYAxis(this SceneNode node) {
			return node.LocalAxes.GetColumn(1);
		}

		/// <summary>
		/// Get the local Z axis of this node, in radians.
		/// </summary>
		public static Vector3 GetLocalZAxis(this SceneNode node) {
			return node.LocalAxes.GetColumn(2);
		}
		#endregion Node

		#region RenderWindow

		// How to change the icon of the window ogre created
		// from http://www.ogre3d.org/tikiwiki/Set+icon+of+Window+(Mogre)

		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")]
		private static extern int DrawMenuBar(int currentWindow);

		private const int WM_SETICON = 0x80;
		private const int ICON_SMALL = 0, ICON_BIG = 1;

		/// <summary>
		/// Sets the icon of a render window.
		/// </summary>
		public static void SetIcon(this RenderWindow window, Icon icon) {
			IntPtr hwnd = new IntPtr();
			window.GetCustomAttribute("WINDOW", out hwnd);

			// check parameters
			if (icon == null || hwnd == null || hwnd == IntPtr.Zero)
				return;

			// Set the icon with SendMessage
			SendMessage(hwnd, WM_SETICON, (IntPtr) ICON_SMALL, (IntPtr) icon.Handle);
			SendMessage(hwnd, WM_SETICON, (IntPtr) ICON_BIG, (IntPtr) icon.Handle);
			// Force windows to update the icon
			DrawMenuBar((int) hwnd);
		}

		#endregion

		#region SceneManager

		public static void SetupShadows(this SceneManager sceneMgr, Level level) {
			if (Options.ShadowDetail != ShadowDetailOption.None) {
				sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;
				sceneMgr.ShadowFarDistance = float.Parse(Options.Get("ShadowDistance"));
				sceneMgr.ShadowColour = level.Definition.GetVectorProperty("ShadowColour", new Vector3(0.8f, 0.8f, 0.8f)).ToColourValue();
			}
			else {
				sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_NONE;
			}

			/*
			sceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_MODULATIVE_INTEGRATED;

			sceneMgr.SetShadowTextureCountPerLightType(Light.LightTypes.LT_DIRECTIONAL, 3);
			sceneMgr.ShadowTextureCount = 3;
			sceneMgr.SetShadowTextureConfig(0, 1024, 1024, PixelFormat.PF_FLOAT32_R);
			sceneMgr.SetShadowTextureConfig(1, 512, 512, PixelFormat.PF_FLOAT32_R);
			sceneMgr.SetShadowTextureConfig(2, 512, 512, PixelFormat.PF_FLOAT32_R);
			sceneMgr.ShadowTextureSelfShadow = true;
			sceneMgr.ShadowCasterRenderBackFaces = false;
			sceneMgr.ShadowFarDistance = 150;
			sceneMgr.SetShadowTextureCasterMaterial("PSSM/shadow_caster");
			sceneMgr.SetShadowTextureFadeStart(0.7f);

			PSSMShadowCameraSetup pssm = new PSSMShadowCameraSetup();
			pssm.SplitPadding = 1f;
			pssm.CalculateSplitPoints(3, 0.01f, sceneMgr.ShadowFarDistance - 10);
			pssm.SetOptimalAdjustFactor(0, 2);
			pssm.SetOptimalAdjustFactor(1, 1f);
			pssm.SetOptimalAdjustFactor(2, 0.5f);
			pssm.UseSimpleOptimalAdjust = false;

			sceneMgr.SetShadowCameraSetup(new ShadowCameraSetupPtr(pssm));*/

		}
		#endregion
	}
}
