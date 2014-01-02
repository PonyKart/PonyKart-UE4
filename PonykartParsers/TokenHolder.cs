using System;
using System.Collections.Generic;
using Mogre;

namespace PonykartParsers {
	/// <summary>
	/// Since both the .thing and its blocks can all have properties, they all use this abstract class to give them dictionaries and a few helpful methods
	/// </summary>
	public abstract class TokenHolder {
		public IDictionary<string, ThingEnum> EnumTokens { get; protected set; }
		public IDictionary<string, string> StringTokens { get; protected set; }
		public IDictionary<string, float> FloatTokens { get; protected set; }
		public IDictionary<string, bool> BoolTokens { get; protected set; }
		public IDictionary<string, Vector3> VectorTokens { get; protected set; }
		public IDictionary<string, Quaternion> QuatTokens { get; protected set; }

		public virtual void SetUpDictionaries() {
			EnumTokens = new Dictionary<string, ThingEnum>();
			StringTokens = new Dictionary<string, string>();
			FloatTokens = new Dictionary<string, float>();
			BoolTokens = new Dictionary<string, bool>();
			VectorTokens = new Dictionary<string, Vector3>();
			QuatTokens = new Dictionary<string, Quaternion>();
		}

		/// <summary>
		/// Gets an enum property from the dictionaries.
		/// </summary>
		/// <param name="propertyName">The name of the property to look for</param>
		/// <param name="defaultValue">If the property was not found, use this instead. Pass null if this is a required property.</param>
		public ThingEnum GetEnumProperty(string propertyName, ThingEnum? defaultValue) {
			ThingEnum te;
			if (EnumTokens.TryGetValue(propertyName.ToLower(), out te))
				return te;
			else if (defaultValue == null)
				throw new ArgumentException("That property was not found in the .thing file!", propertyName);
			else
				return (ThingEnum) defaultValue;
		}

		/// <summary>
		/// Gets a string property from the dictionaries.
		/// </summary>
		/// <param name="propertyName">The name of the property to look for</param>
		/// <param name="defaultValue">If the property was not found, use this instead. Pass null if this is a required property.</param>
		public string GetStringProperty(string propertyName, string defaultValue) {
			string s;
			if (StringTokens.TryGetValue(propertyName.ToLower(), out s))
				return s;
			else if (defaultValue == null)
				throw new ArgumentException("That property was not found in the .thing file!", propertyName);
			else
				return defaultValue;
		}

		/// <summary>
		/// Gets a float property from the dictionaries.
		/// </summary>
		/// <param name="propertyName">The name of the property to look for</param>
		/// <param name="defaultValue">If the property was not found, use this instead. Pass null if this is a required property.</param>
		public float GetFloatProperty(string propertyName, float? defaultValue) {
			float f;
			if (FloatTokens.TryGetValue(propertyName.ToLower(), out f))
				return f;
			else if (defaultValue == null)
				throw new ArgumentException("That property was not found in the .thing file!", propertyName);
			else
				return (float) defaultValue;
		}

		/// <summary>
		/// Gets a boolean property from the dictionaries.
		/// </summary>
		/// <param name="propertyName">The name of the property to look for</param>
		/// <param name="defaultValue">If the property was not found, use this instead. Pass null if this is a required property.</param>
		public bool GetBoolProperty(string propertyName, bool? defaultValue) {
			bool b;
			if (BoolTokens.TryGetValue(propertyName.ToLower(), out b))
				return b;
			else if (defaultValue == null)
				throw new ArgumentException("That property was not found in the .thing file!", propertyName);
			else
				return (bool) defaultValue;
		}

		/// <summary>
		/// Gets a vector property from the dictionaries.
		/// </summary>
		/// <param name="propertyName">The name of the property to look for</param>
		/// <param name="defaultValue">If the property was not found, use this instead. Pass null if this is a required property.</param>
		public Vector3 GetVectorProperty(string propertyName, Vector3? defaultValue) {
			Vector3 v;
			if (VectorTokens.TryGetValue(propertyName.ToLower(), out v))
				return v;
			else if (defaultValue == null)
				throw new ArgumentException("That property was not found in the .thing file!", propertyName);
			else
				return (Vector3) defaultValue;
		}

		/// <summary>
		/// Gets a quaternion property from the dictionaries.
		/// </summary>
		/// <param name="propertyName">The name of the property to look for</param>
		/// <param name="defaultValue">If the property was not found, use this instead. Pass null if this is a required property.</param>
		public Quaternion GetQuatProperty(string propertyName, Quaternion? defaultValue) {
			Quaternion q;
			if (QuatTokens.TryGetValue(propertyName.ToLower(), out q))
				return q;
			else if (defaultValue == null)
				throw new ArgumentException("That property was not found in the .thing file!", propertyName);
			else
				return (Quaternion) defaultValue;
		}

		public virtual void Finish() { }

		public virtual void Dispose() {
			EnumTokens.Clear();
			StringTokens.Clear();
			FloatTokens.Clear();
			BoolTokens.Clear();
			VectorTokens.Clear();
			QuatTokens.Clear();
		}
	}
}
