#include "pch.h"
#include "Kernel/LKernel.h"
#include "Networking/NetworkEntity.h"

using namespace Ponykart;
using namespace Ponykart::Networking;

NetworkEntity::NetworkEntity(Connection* Parent, int Globalid, std::string Name, std::string Selection, bool IsLocal)
{
	nm = LKernel::get<NetworkManager>();
	if (Parent != nullptr) { owner = Parent; }
	isLocal = IsLocal;
	globalId = Globalid;
	selection = selection;
	name = name;
}

