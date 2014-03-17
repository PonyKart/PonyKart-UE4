#ifndef NETWORKENTITY_H_INCLUDED
#define NETWORKENTITY_H_INCLUDED
#include <string>

namespace Ponykart
{
namespace Players{ class Player; }
namespace Networking
{
	class Connection;
	class NetworkManager;
	class NetworkEntity
	{
	public:
		/// Create a new NetworkEntity that is fully designated by a remote host.
		NetworkEntity(Connection* Parent, int Globalid, std::string Name, std::string Selection, bool IsLocal);
		/// Generate a new remote NetworkEntity at this host.
		NetworkEntity(Connection* Parent);
		/// Generate a new local NetworkEntity at this host.
		NetworkEntity();
		// Getters
		int inline getGlobalId() { return globalId; };
		
	public:
		Players::Player* player;
		NetworkManager* nm;
		Connection* owner;
		bool isLocal;

	private:
		std::string name;
		std::string selection;
		int globalId;
	};
}
}
#endif // NETWORKENTITY_H_INCLUDED
