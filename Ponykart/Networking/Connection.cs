using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Ponykart.Core;
using Ponykart.Handlers;
using Ponykart.Properties;
using Ponykart.UI;
using Miyagi.UI.Controls;

namespace Ponykart.Networking {
	/// <summary>
	/// Represents the connection between a client and host across UDP.
	/// </summary>
	public class Connection {
		private DateTime LastSentTime, LastRecvTime;
        public long ZeroMoment,RemoteOffset;
		public bool validated;
        public bool ReadyToSend = false;
        public readonly ReliableUDPConnection UDPConnection;
        public readonly UInt32 Cid;
        Label LobbyLabel;
        Queue<Message> OutgoingQueue;
        NetworkManager nm;
        /// <summary>
        /// Enumerate all the players that are on the other end of this connection.
        /// </summary>
        public IEnumerable<NetworkEntity> Players {
            get {
                return (from ent in LKernel.Get<NetworkManager>().Players
                        where ent.owner == this
                        select ent);
            }
        }

        /// <summary>
        /// Gets the top message on the queue, then removes it (!). 
        /// Generates an empty message if there are none waiting.
        /// </summary>
        public Message TopMessage {
            get {
                if (OutgoingQueue.Count > 0) {
                    if (OutgoingQueue.First().Type == Commands.Connect || ReadyToSend) {
                        return OutgoingQueue.Dequeue();
                    } else {
                        throw new Exception("Not ready yet");
                    }
                } else {
                    if (nm.GameRunning) {
                        return new Message(Commands.SendPositions, nm.SerializeKarts(), true);
                    }
                    return new Message(Commands.NoMessage, "", true);
                }
            }
        }

		/// <summary>
		/// Creates a connection, given a destination to send to and a connection ID.
		/// </summary>
		public Connection(UdpClient sender, IPEndPoint destinationep, UInt32 cid) {

            var LobbyGUI = LKernel.Get<UIMain>().GetGUI("menu lobby gui");
            LobbyLabel = LobbyGUI.GetControl<Label>("lobby label");

            UDPConnection = new ReliableUDPConnection(sender, destinationep, cid, this);
            UDPConnection.OnPacketRecv += (p) => PacketHandler(p.Contents);
            OutgoingQueue = new Queue<Message>();
            Cid = cid;
            nm = LKernel.GetG<NetworkManager>();
		}

		/// <summary>
		/// Handle the information in a packet received via this connection.
		/// </summary>
		public void PacketHandler(PonykartPacket packet) {
            LastRecvTime = System.DateTime.Now;
            string contents = packet.StringContents;
            byte[] contentsArr = packet.ToBytes();
			//Launch.Log(string.Format("Received packet type {1}: {0}", contents, packet.Type));
			NetworkManager nm = LKernel.Get<NetworkManager>();
			if (!Enum.IsDefined(typeof(Commands), (Commands) packet.Type)) {
				// Unrecognized packet type!
                Launch.Log(string.Format("Error: Malformed packet type: {0}", packet.Type)); 
				return;
			}
			switch ((Commands) packet.Type) {
#region Connection Negotiation
				case Commands.Connect:
					if (nm.NetworkType == NetworkTypes.Client) {
						return;
					} else {
						if (nm.Password.SequenceEqual(contents)) {
							Launch.Log(string.Format("Client provided correct password ({0})", contents));
                            validated = true;
                            ReadyToSend = true;
							SendPacket(Commands.ConnectAccept, contents);
                            foreach(NetworkEntity ne in nm.Players) {
                                SendPacket(Commands.NewPlayer, ne.Serialize());
                            }
                            LobbyLabel.Text += String.Format("Player {0} joined.\n", nm.Players.Count);
						} else {
							Launch.Log(string.Format("Client gave bad password: {0} instead of {1}", contents, nm.Password));
							SendPacket((Int16) Commands.ConnectReject, (string) null);
                            CloseConnection();
						}
					}
					break;
				case Commands.ConnectAccept:
					Launch.Log("Server accepted our password.");
                    LobbyLabel.Text += "Connected.\n";
                    ReadyToSend = true;
					break;
				case Commands.ConnectReject:
					Launch.Log("Server rejected our password.");
					break;
#endregion
#region Player Management
                case Commands.RequestPlayer :
                    if (nm.NetworkType == NetworkTypes.Host) {
                        if (nm.Players.Count < Settings.Default.NumberOfPlayers) {
                            var NPlayer = new NetworkEntity(this);
                            nm.Players.Add(NPlayer);
                            nm.ForEachConnection((c) => c.SendPacket((c == this)?Commands.NewLocalPlayer:Commands.NewPlayer, NPlayer.Serialize()));
                        } else {
                            SendPacket(Commands.RejectPlayer, "Full");
                        }
                    }
                    break;

                case Commands.NewPlayer :
                    if (nm.NetworkType == NetworkTypes.Client) {
                        nm.Players.Add(NetworkEntity.Deserialize(packet.StringContents,this, false));
                    }
                    break;

                case Commands.NewLocalPlayer:
                    if (nm.NetworkType == NetworkTypes.Client) {
                        nm.Players.Add(NetworkEntity.Deserialize(packet.StringContents, null, true));
                    }
                    break;

                case Commands.RejectPlayer :
                    // TODO: move forward in menus only when applicable
                    break;

                case Commands.RequestPlayerChange :
                    if (nm.NetworkType == NetworkTypes.Host) {
                        if (NetworkEntity.PerformChange(packet.StringContents, this)) {
                            nm.ForEachConnection((c) => c.SendPacket(Commands.PlayerChange, packet.StringContents));
                        } else { 
                            this.SendPacket(Commands.RejectChange,packet.StringContents); // TODO: include reason?
                        }
                    }
                    break;
                case Commands.PlayerChange :
                    if (nm.NetworkType == NetworkTypes.Client) {
                        if (!NetworkEntity.PerformChange(packet.StringContents, this)) {
                            // TODO: Request new player list.
                        }
                    }
                    break;

                case Commands.LeaveGame:
                    if (nm.NetworkType == NetworkTypes.Host) {
                        if (!NetworkEntity.PerformChange(packet.StringContents, this)) {
                            // TODO: Request new player list.
                        }
                    }
                    break;

                case Commands.RemovePlayer:
                    Launch.Log(string.Format("Got unimplemented packet type: {0}", packet.Type));

                    break;

#endregion
#region Pre-game Setup
                case Commands.SelectLevel:
					if (nm.NetworkType == NetworkTypes.Client) {
						LKernel.Get<MainMenuMultiplayerHandler>().LevelSelection = packet.StringContents;
						SendPacket(Commands.LevelAccept, "");
					}
					break;
				case Commands.StartGame:
                    if (nm.NetworkType == NetworkTypes.Client) {
                        LKernel.Get<MainMenuMultiplayerHandler>().LevelSelection = packet.StringContents;
						LKernel.Get<MainMenuMultiplayerHandler>().Start_Game();
					}
					break;
#endregion
#region Game
                case Commands.ReadyToRumble :
                    if (nm.NetworkType == NetworkTypes.Host) {
                        nm.ReadyConnections.Add(this);
                        if (nm.AllConnectionsReady) {
                            LKernel.Get<RaceCountdown>().Start();
                            nm.ForEachConnection((c) => c.SendPacket(Commands.BeginRumbling));
                        }

                    }
                    break;
                case Commands.BeginRumbling :
                    if (nm.NetworkType == NetworkTypes.Client) {
                        nm.AllConnectionsReady = true;
                        LKernel.Get<RaceCountdown>().Start();
                    }
                    break;
                case Commands.SendPositions:
                    if (nm.NetworkType == NetworkTypes.Client) {
                        NetworkEntity.DeserializeLocations(packet.StringContents, this);
                    } else {
                        NetworkEntity.DeserializeLocations(packet.StringContents, this);
                    }
                    break;
#endregion
				case Commands.ServerMessage:
					Launch.Log(string.Format("Server message: '{0}'", contents));
					break;
                case Commands.NoMessage: // Do nothing.
                    break;
				default: // Unimplemented packet type
					Launch.Log(string.Format("Got unimplemented packet type: {0}", packet.Type));
					break;
			}
		}
		/// <summary>
		/// Sends a packet down this connection with the given contents and type.
		/// </summary>
		public void SendPacket(Int16 type, string contents, bool isVolatile=false) {
			SendPacket((Commands)type, contents, isVolatile);
		}
        /// <summary>
        /// Sends a packet down this connection with the given contents and type.
        /// </summary>
		public void SendPacket(Commands type, string contents="", bool isVolatile=false) {
			SendPacket(type, System.Text.ASCIIEncoding.ASCII.GetBytes(contents??""),isVolatile);
		}
        /// <summary>
        /// Sends a packet down this connection with the given contents and type.
        /// </summary>
        public void SendPacket(Commands type, byte[] contents, bool isVolatile=false) {
            LastSentTime = System.DateTime.Now;
            //var message = new UDPPacket(new PonykartPacket(type, contents, this), UDPConnection);
            var message = new Message(type, contents, isVolatile);
            Launch.Log(String.Format("Queued outgoing packet of type {0}", type));
            OutgoingQueue.Enqueue(message);
        }

        /// <summary>
        /// Close the UDP connection attached to this connection, and then the connection itself
        /// </summary>
        public void CloseConnection() {
            UDPConnection.Close();
            LKernel.Get<NetworkManager>().CloseConnection(this);
        }
    }
}