#ifndef OPTIONSENUMS_H_INCLUDED
#define OPTIONSENUMS_H_INCLUDED

namespace Ponykart
{
namespace Core
{
enum ModelDetailOption
{
	High, ///< High detail. No billboard imposters, all static geometry visible at all times
	Medium, ///< In the middle - you have billboard imposters, but only for far-off things and static geometry is hidden and replaced as you get far away.
	Low ///< Low detail option. All trees and stuff are billboard all the time. Good for netbooks and really old computers.
};

enum ShadowDetailOption
{
	None, ///< No shadows at all
	Some, ///< Some shadows on important things
	Many ///< Shadows on everything!
};
} // Core
} // Ponykart

#endif // OPTIONSENUMS_H_INCLUDED
