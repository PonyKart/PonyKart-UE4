#ifndef NETWORKMANAGER_H_INCLUDED
#define NETWORKMANAGER_H_INCLUDED
#include <stdint.h>
#include <list>
#include "Kernel/LKernelObject.h"

namespace Ponykart
{
namespace Networking
{
	class NetworkEntity;
	enum class Commands 
	{
		Connect = 0x0000,
		ConnectAccept = 0x0001,
		ConnectReject = 0x0002,

		GameFull = 0x0020,

		RequestPlayer = 0x0100,
		NewPlayer = 0x0101,
		RejectPlayer = 0x0102,
		NewLocalPlayer = 0x0103,

		RequestPlayerChange = 0x0110,
		PlayerChange = 0x0111,
		RejectChange = 0x0112,

		LeaveGame = 0x0F00,
		RemovePlayer = 0x0F01,

		SelectLevel = 0x1000,
		LevelAccept = 0x1001,
		CharacterSelect = 0x1010,
		CharacterTaken = 0x1011,
		Characters = 0x1100,
		StartGame = 0x1F00,
		StartAccept = 0x1F01,

		ReadyToRumble = 0x2000,
		BeginRumbling = 0x2001,

		SendPositions = 0x3000,

		ServerMessage = 0xF000,
		NoMessage = 0xFFFF // empty message only to send acks
	};
		
	enum class NetworkTypes 
	{ 
		Client, 
		Host 
	};

	/// This class manages Network connections
	class NetworkManager : public LKernel::LKernelObject
	{
	public:
		NetworkManager() = default;
		// TODO: Implement NetworkManager class, add all the members
	public:
		std::list<NetworkEntity*> players; // Keep this synchronized among all connections!
		static const int HOST = 0;
		static const int CLIENT = 1;
	};

}
}
#endif // NETWORKMANAGER_H_INCLUDED
