#ifndef SDL2EXTENSIONS_H_INCLUDED
#define SDL2EXTENSIONS_H_INCLUDED

#include <string>
#include <SDL.h>


namespace Extensions
{
	class SDL2Exception {
	public:
		const char *what () const { return SDL_GetError(); }
	};
}

#endif // SDL2EXTENSIONS_H_INCLUDED
