#ifndef THINGENUM_H_INCLUDED
#define THINGENUM_H_INCLUDED


namespace PonykartParsers
{
// All of the different enums we can get from the .thing and .muffin files. Used by the two parsers.
enum ThingEnum
{
	// physics type
	Dynamic, // For dynamic bodies
	Kinematic, // For kinematic bodies
	Static, // For static bodies
	None, // For bodies with no type


	// physics shape
	Box,
	Capsule, // a capsule aligned with the Y axis
	Sphere,
	Cylinder, // A cylinder aligned with the Y axis
	Cone, // A cone aligned with the Y axis
	Hull, // For convex hulls
	Mesh, // For trimeshes
	Heightmap, // For heightmaps


	// collision groups
	// see CollisionGroups.h
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
	Point, //Standard point billboard (default), always faces the camera completely and is always upright
	OrientedCommon, //Billboards are oriented around a shared direction vector (used as Y axis) and only rotate around this to face the camera
	OrientedSelf, //Billboards are oriented around their own direction vector (their own Y axis) and only rotate around this to face the camera
	PerpendicularCommon, //Billboards are perpendicular to a shared direction vector (used as Z axis, the facing direction) and X, Y axis are determined by a shared up-vector
	PerpendicularSelf, //Billboards are perpendicular to their own direction vector (their own Z axis, the facing direction) and X, Y axis are determined by a shared up-vector


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
	Many
};
} // PonykartParsers

#endif // THINGENUM_H_INCLUDED
