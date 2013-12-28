/*
 *  Created on: Feb 25, 2011
 *      Author: Paul
 *		Source: github.com/paulrehkugler/ExtensionSearch
 */

#ifndef DIRENTSEARCH_H_INCLUDED
#define DIRENTSEARCH_H_INCLUDED

#include <vector>
#include <string>

namespace Extensions
{
	std::string getFilenameWithoutExtension(const std::string& path); ///< Keep everything between the last / and the last .
	std::vector<std::string> direntSearch(const std::string& directory, const std::string& extension); ///< Search only inside directory
	std::vector<std::string> direntSearchRec(const std::string& directory, const std::string& extension); ///< Search recursively
} // Extensions

#endif // DIRENTSEARCH_H_INCLUDED
