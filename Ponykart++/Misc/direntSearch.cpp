/*
 *  Created on: Feb 25, 2011
 *      Author: Paul
 *		Source: github.com/paulrehkugler/ExtensionSearch
 */

#include <unistd.h>
#include <iostream>
#include <dirent.h>
#include <vector>

std::string getFilenameWithoutExtension(const std::string& path) // TODO: Check if this is correct (getFilenameWithoutExtension)
{
	std::string result;
    auto pos = path.rfind('/');
    if (pos != std::string::npos)	result = std::string(path, pos);
    pos = result.rfind('\\');
    if (pos != std::string::npos)	result = std::string(result, pos);
    pos = result.rfind('.');
    if (pos != std::string::npos)	result = std::string(result, 0, pos-1);
	return result;
}

std::vector<std::string> direntSearch(const std::string& directory, const std::string& extension) // recursive search algorithm
{
	std::vector<std::string> results;
	DIR* dir_point = opendir(directory.c_str());
	dirent* entry = readdir(dir_point);
	while (entry){									// if !entry then end of directory
		if (entry->d_type == DT_REG){		// if entry is a regular file
			std::string fname = entry->d_name;	// filename
												// if filename's last characters are extension
			if (fname.find(extension, (fname.length() - extension.length())) != std::string::npos)
				results.push_back(fname);		// add filename to results vector
		}
		entry = readdir(dir_point);
	}
	return results;
}

std::vector<std::string> direntSearchRec(const std::string& directory, const std::string& extension) // recursive search algorithm
{
	std::vector<std::string> results;
	DIR* dir_point = opendir(directory.c_str());
	dirent* entry = readdir(dir_point);
	while (entry){									// if !entry then end of directory
		if (entry->d_type == DT_DIR){				// if entry is a directory
			std::string fname = entry->d_name;
			if (fname != "." && fname != "..")
				direntSearchRec(entry->d_name, extension);	// search through it
		}
		else if (entry->d_type == DT_REG){		// if entry is a regular file
			std::string fname = entry->d_name;	// filename
												// if filename's last characters are extension
			if (fname.find(extension, (fname.length() - extension.length())) != std::string::npos)
				results.push_back(fname);		// add filename to results vector
		}
		entry = readdir(dir_point);
	}
	return results;
}
