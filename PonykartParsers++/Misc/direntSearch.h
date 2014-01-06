/*
 *  Created on: Feb 25, 2011
 *      Author: Paul
 *		Source: github.com/paulrehkugler/ExtensionSearch
 */

#ifndef DIRENTSEARCH_H_INCLUDED
#define DIRENTSEARCH_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifndef DLLEXPORT
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#define __cdecl __attribute__((__cdecl__))
#endif
#endif

#include <vector>
#include <string>

namespace Extensions
{
	DLLEXPORT std::string __cdecl getFilenameWithoutExtension(const std::string& path); ///< Keep everything between the last / and the last .
	DLLEXPORT std::vector<std::string> __cdecl direntSearch(const std::string& directory, const std::string& extension); ///< Search only inside directory
	DLLEXPORT std::vector<std::string> __cdecl direntSearchRec(const std::string& directory, const std::string& extension); ///< Search recursively
} // Extensions

#endif // DIRENTSEARCH_H_INCLUDED
