#ifndef EXTENSIONS_H_INCLUDED
#define EXTENSIONS_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifndef DLLEXPORT
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif
#endif

#include <irrKlang.h>

namespace Ogre{class Vector3;}

namespace Extensions
{
	DLLEXPORT irrklang::vec3df toSoundVector(const Ogre::Vector3& vec);
} // Extensions

#endif // EXTENSIONS_H_INCLUDED
