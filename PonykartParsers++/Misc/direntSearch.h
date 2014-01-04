/*
 *  Created on: Feb 25, 2011
 *      Author: Paul
 *		Source: github.com/paulrehkugler/ExtensionSearch
 */

#ifndef DIRENTSEARCH_H_INCLUDED
#define DIRENTSEARCH_H_INCLUDED

// MSVC needs to be told what to export in the DLL
#ifdef _WIN32
#define DLLEXPORT __declspec( dllexport )
#else
#define DLLEXPORT
#endif

#include <vector>
#include <string>

namespace Extensions
{
	DLLEXPORT std::string getFilenameWithoutExtension(const std::string& path); ///< Keep everything between the last / and the last .
	DLLEXPORT std::vector<std::string> direntSearch(const std::string& directory, const std::string& extension); ///< Search only inside directory
	DLLEXPORT std::vector<std::string> direntSearchRec(const std::string& directory, const std::string& extension); ///< Search recursively
} // Extensions

#endif // DIRENTSEARCH_H_INCLUDED
