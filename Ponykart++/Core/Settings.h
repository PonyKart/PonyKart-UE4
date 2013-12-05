#ifndef SETTINGS_H_INCLUDED
#define SETTINGS_H_INCLUDED

namespace Settings
{

constexpr static char WorldFileLocation[] = "media/worlds/";
constexpr static char MainMenuName[] = "Level0";
constexpr static bool EnableGlowyRegions = false;
constexpr static int NumberOfPlayers = 2;
constexpr static float CameraTightness = 21;
constexpr static float Gravity = -15;
constexpr static float CameraNodeYOffset = 1.25;
constexpr static float CameraNodeZOffset = -2.85;
constexpr static float CameraTargetYOffset = 0.9;
constexpr static float KartGravity = -14;

} // Settings

#endif // SETTINGS_H_INCLUDED
