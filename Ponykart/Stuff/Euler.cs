using System;
using System.Text.RegularExpressions;


namespace Mogre {

	// ABOUT THIS CODE:
	// 
	// Developed by user Kojack in January 2012
	// Extended by Beauty and Tubulii
	//
	// Detailed information about usage:
	// http://www.ogre3d.org/tikiwiki/Euler+Angle+Class
	// 
	// "official place" of C# version:
	// http://www.ogre3d.org/tikiwiki/Euler+Angle+Class+Mogre
	// 
	// For questions and bug reports use this forum topic:
	// viewtopic.php?f=8&t=29262
	//




	/// <summary>
	/// This struct manages rotations by use of Euler Angles. 
	/// The rotation will be applied in the order Yaw-Pitch-Roll, which are related to the axis order Y-X-Z. 
	/// It's fully compatible with Ogre::Matrix3::FromEulerAnglesYXZ and Ogre::Matrix3::ToEulerAnglesYXZ. 
	/// For the Yaw angle the standard anticlockwise right handed rotation is used (as common in Ogre).
	/// The Yaw-Pitch-Roll ordering is most convenient for upright character controllers and cameras.
	/// </summary>
	public struct Euler {

		/// <summary>Get or set the Yaw angle.</summary>
		public Radian Yaw {
			get { return mYaw; }
			set {
				mYaw = value;
				mChanged = true;
			}
		}


		/// <summary>Get or set the Pitch angle.</summary>
		public Radian Pitch {
			get { return mPitch; }
			set {
				mPitch = value;
				mChanged = true;
			}
		}


		/// <summary>Get or set the Roll angle.</summary>
		public Radian Roll {
			get { return mRoll; }
			set {
				mRoll = value;
				mChanged = true;
			}
		}


		/// <summary>
		/// Constructor to create a new Euler Angle struct from angle values. 
		/// The rotation will be applied in the order Yaw-Pitch- Roll, which are related to the axis order Y-X-Z. 
		/// </summary>
		public Euler(Radian yaw, Radian pitch, Radian roll) {
			mYaw = yaw;
			mPitch = pitch;
			mRoll = roll;
			mChanged = true;
			mCachedQuaternion = Quaternion.IDENTITY;
		}


		/// <summary>
		/// Constructor which calculates the Euler Angles from a quaternion.
		/// </summary>
		public Euler(Quaternion oriantation) {
			Matrix3 rotMat;
			rotMat = oriantation.ToRotationMatrix();

			// BUGGY METHOD (NaN return in some cases)
			// rotMat.ToEulerAnglesYXZ(out mYaw, out mPitch, out mRoll);

			// WORKAROUND
			bool success;
			Matrix3ToEulerAnglesYXZ(rotMat, out mYaw, out mPitch, out mRoll, out success);

			mChanged = true;
			mCachedQuaternion = Quaternion.IDENTITY;
		}



		/// <summary>
		/// Apply a relative yaw. (Adds the angle to the current yaw value)
		/// </summary>
		/// <param name="yaw">Yaw value as radian</param>
		public void AddYaw(Radian yaw) {
			mYaw += yaw;
			mChanged = true;
		}


		/// <summary>
		/// Apply a relative pitch. (Adds the angle to the current pitch value)
		/// </summary>
		/// <param name="pitch">Pitch value as radian</param>
		public void AddPitch(Radian pitch) {
			mPitch += pitch;
			mChanged = true;
		}


		/// <summary>
		/// Apply a relative roll. (Adds the angle to the current roll value)
		/// </summary>
		/// <param name="roll">Roll value as radian</param>
		public void AddRoll(Radian roll) {
			mRoll += roll;
			mChanged = true;
		}


		/// <summary>
		/// Apply a relative yaw. (Adds the angle to the current yaw value)
		/// </summary>
		/// <param name="yaw">Yaw value as degree</param>
		public void AddYaw(Degree yaw) {
			mYaw += yaw;
			mChanged = true;
		}


		/// <summary>
		/// Apply a relative pitch. (Adds the angle to the current pitch value)
		/// </summary>
		/// <param name="pitch">Pitch value as degree</param>
		public void AddPitch(Degree pitch) {
			mPitch += pitch;
			mChanged = true;
		}


		/// <summary>
		/// Apply a relative roll. (Adds the angle to the current roll value)
		/// </summary>
		/// <param name="roll">Roll value as degree</param>
		public void AddRoll(Degree roll) {
			mRoll += roll;
			mChanged = true;
		}


		/// <summary>Get a vector pointing forwards. </summary>
		public Vector3 Forward {
			get { return ToQuaternion() * Vector3.NEGATIVE_UNIT_Z; }
		}


		/// <summary>Get a vector pointing to the right.</summary>
		public Vector3 Right {
			get { return ToQuaternion() * Vector3.UNIT_X; }
		}


		/// <summary> Get a vector pointing up.</summary>
		public Vector3 Up {
			get { return ToQuaternion() * Vector3.UNIT_Y; }
		}


		/// <summary>
		/// Calculate the quaternion of the euler object. 
		/// The result is cached. It will only be recalculated when the component euler angles are changed.
		/// </summary>
		public Quaternion ToQuaternion() {
			if (mChanged) {
				mCachedQuaternion = new Quaternion(mYaw, Vector3.UNIT_Y) * new Quaternion(mPitch, Vector3.UNIT_X) * new Quaternion(mRoll, Vector3.UNIT_Z);
				mChanged = false;
			}
			return mCachedQuaternion;
		}


		/// <summary>
		/// Return a String with degree values of the axis rotations (human readable style). 
		/// For example "X-axis: 0°   Y-axis: 36°   Z-axis: 90°"
		/// </summary>
		public String ToAxisString() {
			return String.Format("X: {0:00}°   Y: {1:00}°   Z: {2:00}°", Pitch.ValueDegrees, Yaw.ValueDegrees, Roll.ValueDegrees);
		}




		/// <summary>
		/// Return a String with degree values in the applied rotation order (human readable style).
		/// For example "Yaw: 0°   Pitch: 36°   Roll: 90°"
		/// </summary>
		public String ToYawPitchRollString() {
			return String.Format("Yaw: {0:00}°   Pitch: {1:00}°   Roll: {2:00}°", Yaw.ValueDegrees, Pitch.ValueDegrees, Roll.ValueDegrees);
		}




		/// <summary>
		/// Try to parse 3 floating point values from a string, which are seperated by spaces or tabulators. 
		/// Input order for rotation values:: Yaw, Pitch, Roll (all in degree)
		/// If success, an Euler struct will be returned. 
		/// If parsing failed, a FormatException will be thrown. 
		/// Example: "111  .99   -66" 
		/// </summary>
		/// <param name="valueString">String which contains 3 floating point values</param>
		/// <remarks>
		/// Multiple and mixed usage of space/tabulator/commas are possible. 
		/// As decimal seperator a dot "." is expected. 
		/// </remarks>
		/// <returns>Returns an Euler struct or a FormatException</returns>
		public static Euler ParseStringYawPitchRoll(String valueString) {
			Single[] values = Parse3Params(valueString);

			if (values == null)
				throw new FormatException(String.Format("Can't parse floating point values of string '{0}'", valueString));

			return new Euler(new Degree(values[0]), new Degree(values[1]), new Degree(values[2]));
		}



		/// <summary>
		/// Try to parse 3 floating point values from a string, which are seperated by spaces or tabulators or comma. 
		/// Input order for rotation values: X-axis, Y-axis, Z-axis (all in degree)
		/// If success, an Euler struct will be returned. 
		/// If parsing failed, a FormatException will be thrown. 
		/// Example: "111  .99   -66" 
		/// </summary>
		/// <param name="valueString">String which contains 3 floating point values</param>
		/// <remarks>
		/// Multiple and mixed usage of space/tabulator/commas are possible. 
		/// As decimal seperator a dot "." is expected. 
		/// </remarks>
		/// <returns>Returns an Euler struct or a FormatException</returns>
		public static Euler ParseStringAxisXYZ(String valueString) {
			Single[] values = Parse3Params(valueString);

			if (values == null)
				throw new FormatException(String.Format("Can't parse floating point values of string '{0}'", valueString));

			return new Euler(new Degree(values[1]), new Degree(values[0]), new Degree(values[2]));
		}



		/// <summary>
		/// Try to parse 3 floating point values from a string, which are seperated by spaces or tabulators or comma. 
		/// If parsing failed, null will be returned. 
		/// Example: "111  .99   -66" 
		/// </summary>
		/// <param name="valueString">String which contains 3 floating point values</param>
		/// <remarks>
		/// Multiple and mixed usage of space/tabulator/commas are possible. 
		/// As decimal seperator a dot "." is expected. 
		/// </remarks>
		/// <returns>Returns 3 Single values or null</returns>
		private static Single[] Parse3Params(String valueString) {
			// Some Regex explanation:
			//
			// The "@" prefix in front of the String means: 
			//         Backslash are processed as Text instead of special symbols.
			//         Advantage: Just write "\" instead of "\\" for each backslash
			// 
			// "^" at first position means:  No text is allowed before
			// "$" at the end means:         No text is allowed after that
			// 
			// Floating point values are matched
			//         Expression: "-?\d*\.?\d+"
			//         Examples:   "111",  "0.111",  ".99",  "-66"
			//
			// Seperator can be tabs or spaces or commas (at least one symbol; mixing is possible)
			//         Expression: "[, \t]+"


			String val = @"[-\d\.]+";     // simplified (faster) floating point pattern (exact pattern would be @"-?\d*\.?\d+" )
			String sep = @"[, \t]+";      // seperator pattern

			// build target pattern
			String searchPattern = "^(" + val + ")" + sep + "(" + val + ")" + sep + "(" + val + ")$";

			Match match = Regex.Match(valueString, searchPattern);

			try {
				if (match.Success) {
					// Force to parse "." as decimal char.  (Can be different with other culture settings. E.g. German culture expect "," instad of ".")
					System.Globalization.CultureInfo englishCulture = new System.Globalization.CultureInfo("en-GB");

					Single[] result = new Single[3];
					result[0] = Convert.ToSingle(match.Groups[1].Value, englishCulture);
					result[1] = Convert.ToSingle(match.Groups[2].Value, englishCulture);
					result[2] = Convert.ToSingle(match.Groups[3].Value, englishCulture);
					return result;
				}
				else
					return null;
			}
			catch (FormatException) { return null; }
			catch (OverflowException) { return null; }

		} // Parse3Params()




		/// <summary>
		/// Return the Euler rotation state as quaternion.
		/// </summary>
		/// <param name="e">Euler Angle state</param>
		/// <returns>Rotation state as Quaternion</returns>
		public static implicit operator Quaternion(Euler e) {
			return e.ToQuaternion();
		}



		/// <summary>
		/// Set the yaw and pitch to face in the given direction. 
		/// The direction doesn't need to be normalised. 
		/// Roll is always unaffected.
		/// </summary>
		/// <param name="directionVector">Vector which points to the wanted direction</param>
		/// <param name="setYaw">if false, the yaw isn't changed.</param>
		/// <param name="setPitch">if false, the pitch isn't changed.</param>
		public void SetDirection(Vector3 directionVector, bool setYaw, bool setPitch) {
			Vector3 d = directionVector.NormalisedCopy;
			if (setPitch)
				mPitch = Math.ASin(d.y);
			if (setYaw)
				mYaw = Math.ATan2(-d.x, -d.z);//+Math.PI/2.0;
			mChanged = setYaw || setPitch;
		}



		/// <summary>
		/// Normalise the selected rotations to be within the +/-180 degree range. 
		/// The normalise uses a wrap around, so for example a yaw of 360 degrees becomes 0 degrees, 
		/// and -190 degrees becomes 170. 
		/// By the parameters it's possible to choose which angles should be normalised.
		/// </summary>
		/// <param name="normYaw">If true, the angle will be normalised.</param>
		/// <param name="normPitch">If true, the angle will be normalised.</param>
		/// <param name="normRoll">If true, the angle will be normalised.</param>
		/// <remarks></remarks>
		public void Normalise(bool normYaw, bool normPitch, bool normRoll) {
			if (normYaw) {
				Single yaw = mYaw.ValueRadians;
				if (yaw < -Math.PI) {
					yaw = (Single) System.Math.IEEERemainder(yaw, Math.PI * 2.0);
					if (yaw < -Math.PI) {
						yaw += Math.PI * 2.0f;
					}
					mYaw = yaw;
					mChanged = true;
				}
				else if (yaw > Math.PI) {
					yaw = (Single) System.Math.IEEERemainder(yaw, Math.PI * 2.0f);
					if (yaw > Math.PI) {
						yaw -= Math.PI * 2.0f;
					}
					mYaw = yaw;
					mChanged = true;
				}
			}

			if (normPitch) {
				Single pitch = mPitch.ValueRadians;
				if (pitch < -Math.PI) {
					pitch = (Single) System.Math.IEEERemainder(pitch, Math.PI * 2.0f);
					if (pitch < -Math.PI) {
						pitch += Math.PI * 2.0f;
					}
					mPitch = pitch;
					mChanged = true;

#if DEBUG
					if (Single.IsNaN(mPitch.ValueDegrees)) // DEBUGGING
			 {
					}  // add breakpoint here
#endif
				}
				else if (pitch > Math.PI) {
					pitch = (Single) System.Math.IEEERemainder(pitch, Math.PI * 2.0f);
					if (pitch > Math.PI) {
						pitch -= Math.PI * 2.0f;
					}
					mPitch = pitch;
					mChanged = true;

#if DEBUG
					if (Single.IsNaN(mPitch.ValueDegrees)) // DEBUGGING
			 {
					}  // add breakpoint here
#endif
				}
			}

			if (normRoll) {
				Single roll= mRoll.ValueRadians;
				if (roll < -Math.PI) {
					roll = (Single) System.Math.IEEERemainder(roll, Math.PI * 2.0f);
					if (roll < -Math.PI) {
						roll += Math.PI * 2.0f;
					}
					mRoll = roll;
					mChanged = true;
				}
				else if (roll > Math.PI) {
					roll = (Single) System.Math.IEEERemainder(roll, Math.PI * 2.0f);
					if (roll > Math.PI) {
						roll -= Math.PI * 2.0f;
					}
					mRoll = roll;
					mChanged = true;
				}
			}
		} // Normalise()




		/// <summary>
		/// Return the relative euler angles required to rotate from the current forward direction to the specified direction vector. 
		/// The result euler can then be added to the current euler to immediately face dir. 
		/// Rotation is found to face the correct direction. For example, when false a yaw of 1000 degrees and a dir of
		/// (0,0,-1) will return a -1000 degree yaw. When true, the same yaw and dir would give 80 degrees (1080 degrees faces
		/// the same way as (0,0,-1).
		/// The rotation won't flip upside down then roll instead of a 180 degree yaw.
		/// </summary>
		/// <param name="direction">...TODO...</param>
		/// <param name="shortest">If false, the full value of each angle is used. If true, the angles are normalised and the shortest rotation is found to face the correct direction.</param>
		/// <param name="setYaw">If true the angles are calculated. If false, the angle is set to 0. </param>
		/// <param name="setPitch">If true the angles are calculated. If false, the angle is set to 0. </param>
		public Euler GetRotationTo(Vector3 direction, bool setYaw, bool setPitch, bool shortest) {
			Euler t1 = Euler.IDENTITY;
			Euler t2;
			t1.SetDirection(direction, setYaw, setPitch);
			t2 = t1 - this;
			if (shortest && setYaw) {
				t2.Normalise(true, true, true);
			}
			return t2;
		}



		/// <summary>
		/// Clamp the yaw angle to a range of +/-limit.
		/// </summary>
		/// <param name="limit">Wanted co-domain for the Yaw angle.</param>
		public void LimitYaw(Radian limit) {
			if (mYaw > limit) {
				mYaw = limit;
				mChanged = true;
			}
			else if (mYaw < -limit) {
				mYaw = -limit;
				mChanged = true;
			}
		}



		/// <summary>
		/// Clamp the pitch angle to a range of +/-limit.
		/// </summary>
		/// <param name="limit">Wanted co-domain for the Pitch angle.</param>
		public void LimitPitch(Radian limit) {
			if (mPitch > limit) {
				mPitch = limit;
				mChanged = true;
			}
			else if (mPitch < -limit) {
				mPitch = -limit;
				mChanged = true;
			}
		}



		/// <summary>
		/// Clamp the roll angle to a range of +/-limit.
		/// </summary>
		/// <param name="limit">Wanted co-domain for the Roll angle.</param>
		public void LimitRoll(Radian limit) {
			if (mRoll > limit) {
				mRoll = limit;
				mChanged = true;
			}
			else if (mRoll < -limit) {
				mRoll = -limit;
				mChanged = true;
			}
		}




		/// <summary>
		/// Port of method <c>Matrix3.ToEulerAnglesYXZ()</c>, from MogreMatrix3.cpp as a workaround for a bug in the Math class. 
		/// (The bug was fixed, but is not present in common used binary files.)
		/// </summary>
		/// <param name="matrix">Rotation matrix</param>
		/// <param name="success">This COULD BE a flag for "success", but it's not documented in the original code.</param>
		public static void Matrix3ToEulerAnglesYXZ(Matrix3 matrix,
			out Radian rfYAngle, out Radian rfPAngle, out Radian rfRAngle, out bool success) {
			rfPAngle = Mogre.Math.ASin(-matrix.m12);

			if (rfPAngle < new Radian(Math.HALF_PI)) {
				if (rfPAngle > new Radian(-Math.HALF_PI)) {
					rfYAngle = Math.ATan2(matrix.m02, matrix.m22);
					rfRAngle = Math.ATan2(matrix.m10, matrix.m11);
					success = true;
				}
				else {
					// WARNING.  Not a unique solution.
					Radian fRmY = Math.ATan2(-matrix.m01, matrix.m00);
					rfRAngle = new Radian(0f);  // any angle works
					rfYAngle = rfRAngle - fRmY;
					success = false;
				}
			}
			else {
				// WARNING.  Not a unique solution.
				Radian fRpY = Math.ATan2(-matrix.m01, matrix.m00);
				rfRAngle = new Radian(0f);  // any angle works
				rfYAngle = fRpY - rfRAngle;
				success = false;
			}
		} // Matrix3ToEulerAnglesYXZ()




		/// <summary>
		/// Add two euler objects.
		/// </summary>
		/// <returns>Calculation result</returns>
		public static Euler operator +(Euler lhs, Euler rhs) {
			return new Euler(lhs.Yaw + rhs.Yaw, lhs.Pitch + rhs.Pitch, lhs.Roll + rhs.Roll);
		}


		/// <summary>
		/// Subtract two euler objects. This finds the difference as relative angles.
		/// </summary>
		/// <returns>Calculation result</returns>
		public static Euler operator -(Euler lhs, Euler rhs) {
			return new Euler(lhs.Yaw - rhs.Yaw, lhs.Pitch - rhs.Pitch, lhs.Roll - rhs.Roll);
		}


		/// <summary>
		/// Interpolate each euler angle by the given factor. 
		/// (Each angle will be multiplied with the factor.)
		/// </summary>
		/// <returns>Calculation result</returns>
		public static Euler operator *(Euler lhs, Single factor) {
			return new Euler(lhs.Yaw * factor, lhs.Pitch * factor, lhs.Roll * factor);
		}


		/// <summary>
		/// Interpolate the euler angles by lhs. 
		/// (Each angle will be multiplied with the factor.)
		/// </summary>
		/// <returns>Calculation result</returns>
		public static Euler operator *(Single factor, Euler rhs) {
			return new Euler(factor * rhs.Yaw, factor * rhs.Pitch, factor * rhs.Roll);
		}


		/// <summary>
		/// Apply the euler rotation to the vector rhs. 
		/// The calculation is equal to: quaternion*vector
		/// </summary>
		/// <returns>Calculation result</returns>
		public static Vector3 operator *(Euler lhs, Vector3 rhs) {
			return lhs.ToQuaternion() * rhs;
		}


		/// <summary>Base settings (all angles are 0)</summary>
		public static Euler IDENTITY = new Euler(new Radian(0), new Radian(0), new Radian(0));

		/// <summary>Rotation around the Y axis.</summary>
		private Radian mYaw;

		/// <summary>Rotation around the X axis.</summary>
		private Radian mPitch;

		/// <summary>Rotation around the Z axis.</summary>
		private Radian mRoll;

		/// <summary>Is the cached quaternion out of date?</summary>
		private bool mChanged;

		/// <summary>Cached quaternion equivalent of this euler object.</summary>
		private Quaternion mCachedQuaternion;


	}  // struct Euler
}  // namespace