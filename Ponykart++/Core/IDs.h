#ifndef IDS_H_INCLUDED
#define IDS_H_INCLUDED

#include <functional>

namespace Ponykart
{
	/// Handles getting ID numbers (abstract class)
	class IDs
	{
	public:
		static unsigned incremental();
		static int random();
		static float randomFloat();
	private:
		static unsigned counter;
		static std::function<int (void)> randomIntGen;
		static std::function<float (void)> randomFloatGen;
	};
} // Ponykart

#endif // IDS_H_INCLUDED
