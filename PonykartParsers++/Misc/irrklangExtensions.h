#ifndef EXTENSIONS_H_INCLUDED
#define EXTENSIONS_H_INCLUDED

#include <irrKlang.h>

namespace Ogre{class Vector3;}

namespace Extensions
{
	irrklang::vec3df toSoundVector(const Ogre::Vector3& vec);
} // Extensions

#endif // EXTENSIONS_H_INCLUDED
