#include <string>

namespace Ponykart
{
namespace UI
{
	class GameUIManager
	{
	public:
		GameUIManager();
		void setItemLevel(int level);
		void setItemImage(const std::string& name);
	};
}
}