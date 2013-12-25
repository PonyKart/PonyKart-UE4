/*
 *  Created on: Feb 25, 2011
 *      Author: Paul
 *		Source: github.com/paulrehkugler/ExtensionSearch
 */

#ifndef DIRENTSEARCH_H_INCLUDED
#define DIRENTSEARCH_H_INCLUDED

#include <vector>
#include <string>

std::string getFilenameWithoutExtension(const std::string& path);
std::vector<std::string> direntSearch(const std::string& directory, const std::string& extension); // Search only inside directory
std::vector<std::string> direntSearchRec(const std::string& directory, const std::string& extension); // Search recursively

#endif // DIRENTSEARCH_H_INCLUDED
