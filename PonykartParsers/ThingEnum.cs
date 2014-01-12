
namespace PonykartParsers {
	/// <summary>
	/// All of the different enums we can get from the .thing and .muffin files. Used by the two parsers.
	/// </summary>
	public enum ThingEnum {
		// physics type

		/// <summary>
		/// For dynamic bodies
		/// </summary>
		Dynamic,
		/// <summary>
		/// For kinematic bodies
		/// </summary>
		Kinematic,
		/// <summary>
		/// For static bodies
		/// </summary>
		Static,
		/// <summary>
		/// For bodies with no type
		/// </summary>
		None,


		// physics shape

		Box,
		/// <summary> a capsule aligned with the Y axis </summary>
		Capsule,
		Sphere,
		/// <summary>
		/// A cylinder aligned with the Y axis
		/// </summary>
		Cylinder,
		/// <summary>
		/// A cone aligned with the Y axis
		/// </summary>
		Cone,
		/// <summary>
		/// For convex hulls
		/// </summary>
		Hull,
		/// <summary>
		/// For trimeshes
		/// </summary>
		Mesh,
		/// <summary>
		/// For heightmaps
		/// </summary>
		Heightmap,


		// collision groups
		// see CollisionGroups.cs

	  //None,
		All,
		Default,
		Environment,
		Affectors,
		Road,
		Triggers,
		Karts,
		InvisibleWalls,


		// billboard types

		/// <summary> Standard point billboard (default), always faces the camera completely and is always upright </summary>
		Point,
		/// <summary> Billboards are oriented around a shared direction vector (used as Y axis) and only rotate around this to face the camera </summary>
		OrientedCommon,
		/// <summary> Billboards are oriented around their own direction vector (their own Y axis) and only rotate around this to face the camera </summary>
		OrientedSelf,
		/// <summary> Billboards are perpendicular to a shared direction vector (used as Z axis, the facing direction) and X, Y axis are determined by a shared up-vector </summary>
		PerpendicularCommon,
		/// <summary> Billboards are perpendicular to their own direction vector (their own Z axis, the facing direction) and X, Y axis are determined by a shared up-vector </summary>
		PerpendicularSelf,


		// billboard origins

		TopLeft,
		TopCenter,
		TopRight,
		CenterLeft,
		Center,
		CenterRight,
		BottomLeft,
		BottomCenter,
		BottomRight,


		// for the MuffinParser

		Menu,
		Race,
		EmptyLevel,


		// for shadows

	  //None,
		Some,
		Many,
	}
}
