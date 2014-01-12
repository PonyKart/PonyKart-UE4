using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Ponykart.Actors;
using Ponykart.Levels;
using Ponykart.Physics;
using Ponykart.Players;
using BulletSharp;
using Mogre;

namespace Ponykart.Networking
{  
    public enum Commands {
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
    public enum NetworkTypes {Client, Host};

    /// <summary>
    /// This class manages Network connections
    /// </summary>
    public class NetworkManager
    {
        public static readonly byte[] Protocol = { 123, 10, 205, 7 };
        public static readonly int HOST = 0;
        public static readonly int CLIENT = 1;

        // For receiving connections from the other party
        private UdpClient Listener;
        private IPEndPoint ListenEP;
        // For sending information. Host has many, Client has one.
        private IDictionary<UInt32, Connection> Connections;
        public Connection SingleConnection;
        // Password for connection
        public string Password;

        // What kind of Networking connection we have
        public NetworkTypes NetworkType;

        // Keep track of how many concurrent connections we allow
        private int MaxConnections;

        // Networking thread 
        public Thread NetworkingThread;

        // Speed to send packets at
        public int PacketsPerSecond = 30;

        // Time last packet was sent
        long LastSentTicks;

        // Is the race currently running?
        public bool GameRunning = false;

        public List<NetworkEntity> Players; // Keep this synchronized among all connections!

        private int NextGlobalID = 0;

        private int LastQueriedKart;

        public List<Connection> ReadyConnections;

        bool ReadyOverride = false;

        public bool AllConnectionsReady {
            get {
                if (NetworkType == NetworkTypes.Client) { return ReadyOverride; }
                return ReadyOverride || ReadyConnections.Count == Connections.Count;
            }
            set {
                ReadyOverride = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NetworkManager() {
        }

        /// <summary>
        /// Initializes as Host
        /// </summary>
        /// <param name="port">Port to listen on</param>
        /// <param name="password">string password</param>
        public void InitManager(int port, string password) {
            NetworkType = NetworkTypes.Host;
            Password = password;
            Listener = new UdpClient(port);
            ListenEP = new IPEndPoint(IPAddress.Any, port);
            Connections = new Dictionary<UInt32,Connection>();
            Players = new List<NetworkEntity>();
            InitializeHandlers();
            Players.Add(new NetworkEntity());
            ReadyConnections = new List<Connection>();
            LevelManager.OnLevelPostLoad += new LevelEvent(OnLevelPostLoad);
        }

        void OnLevelPostLoad(LevelChangedEventArgs lcea) {
            if (NetworkType == NetworkTypes.Client) {
                SingleConnection.SendPacket(Commands.ReadyToRumble);
            }
        }
        /// <summary>
        /// Initializes as Client
        /// </summary>
        /// <param name="port">Port to connect on. Trusted to be valid.</param>
        /// <param name="ip">IP (TRUSTED!! To be valid)</param>
        public void InitManager(int port, string password, string ip) {
            NetworkType = NetworkTypes.Client;
            Password = password;
            Listener = new UdpClient(port); //Todo: Add checks
            ListenEP = new IPEndPoint(IPAddress.Parse(ip), port); //Todo: Add checks
            SingleConnection = new Connection(Listener, ListenEP, GenerateCID());
            Connections = new Dictionary<UInt32, Connection>();
            Connections.Add(0, SingleConnection);
            Players = new List<NetworkEntity>();
            InitializeHandlers();
            LevelManager.OnLevelPostLoad += new LevelEvent(OnLevelPostLoad);
        }

        public void InitializeHandlers() {
            LevelManager.OnLevelUnload += new LevelEvent((arg) => { GameRunning = false; });
            LevelManager.OnLevelLoad += new LevelEvent((arg) => { GameRunning = true; });
            //Launch.OnEveryUnpausedTenthOfASecondEvent += new TimerCallback(DebugGenerateKarts);            
        }

        void DebugGenerateKarts(object state) {
            if (GameRunning) {
                var _ = SerializeKarts();
            }
        }
        /// <summary>
        /// Called every time we receive a new packet.
        /// </summary>
        public void OnPacket(byte[] packet) {
           // Launch.Log("Processing packet.");
            UDPPacket p = new UDPPacket(packet);
    //        Launch.Log(string.Format("Protocol string: {0}. Ours: {1}", System.Text.ASCIIEncoding.ASCII.GetString(p.Protocol),
    //                                                                   System.Text.ASCIIEncoding.ASCII.GetString(Protocol)));
            if (p.Protocol.SequenceEqual(Protocol)) {
               // Launch.Log("Packet of our protocol.");
                if (NetworkType == NetworkTypes.Client) {
                  //  Launch.Log("Received packet as client");
                    if (p.CID == SingleConnection.UDPConnection.ConnectionID) {
                        SingleConnection.UDPConnection.Handle(p);
                    }
                }
                else if (NetworkType == NetworkTypes.Host) {
                  //  Launch.Log("Received packet as host");
                    UInt32 id = p.CID;
                    if (Connections.ContainsKey(id)) {
                        Connections[id].UDPConnection.Handle(p);
                    } else if (Connections.Keys.Count < MaxConnections) {
                        Launch.Log("Allowing new connection");
                        Connections.Add(id, new Connection(Listener, new IPEndPoint(ListenEP.Address, ListenEP.Port), id));
                        Connections[id].UDPConnection.Handle(p);
                    }

                }

            }
        }
        
        /// <summary>
        /// Starts up the listener thread.
        /// </summary>
        /// <param name="nConnections">Maximum concurrent connections</param>
        public void StartThread(int nConnections) {
            Thread t = new Thread(AcceptConnections);
            Launch.Log("Network thread opened.");
            NetworkingThread = t;
            t.Start(nConnections);
        }

        /// <summary>
        /// Kill network thread.
        /// </summary>
        public void StopThread() {
            Launch.Log("Network thread halting...");
            NetworkingThread.Abort();
            NetworkingThread.Join();
            Launch.Log("Network thread joined.");
        }

        /// <summary>
        /// Main loop for receiving packets and creating connections.
        /// </summary>
        /// <param name="data">integer, contains the number of concurrent connections to allow</param>
        void AcceptConnections(object data) {
            int nConnections = (int)data;
            MaxConnections = nConnections;
            Launch.Log("waiting for connections...");
            LastSentTicks = 0;
            try {
                while (!Launch.Quit) {
                    if (Listener.Client != null && Listener.Available > 0) {
                        //Launch.Log("Packet available");
                        OnPacket(Listener.Receive(ref ListenEP));
                    }
                    if (new TimeSpan((System.DateTime.Now.Ticks - LastSentTicks) * PacketsPerSecond).Seconds > 1)  {
                        LastSentTicks = System.DateTime.Now.Ticks;
                        ForEachUDPConnection(udpc => udpc.Send());
                    }
                    ForEachUDPConnection(udpc => udpc.ResendPackets());
                }

            } catch (ThreadAbortException) {
                Launch.Log("Networking thread aborted");
            }
        }

        public void ForEachConnection(Action<Connection> act) {
            var _Connections = Connections.Values.ToArray();
            foreach (var c in _Connections) {
                act(c);
            }
        }
        public void ForEachUDPConnection(Action<ReliableUDPConnection> act) {
            var _Connections = Connections.Values.ToArray();
            foreach (var c in _Connections) {
                act(c.UDPConnection);
            }
        }

        public void CloseConnection(Connection connection) {
            Connections.Remove(connection.Cid);
        }

        public UInt32 GenerateCID() {
            Random r = new Random();
            var randarr = new byte[4];
            r.NextBytes(randarr);
            return BitConverter.ToUInt32(randarr, 0);
        }

        public int AssignGlobalID() {
            return NextGlobalID++;
        }

        public string SerializeKarts() {
            string s = "<karts>";
            lock (Players) {
                for (int i = 0; i < Players.Count; i++) {
                    LastQueriedKart++;
                    if (LastQueriedKart >= Players.Count) {
                        LastQueriedKart = 0;
                    }
                    if (Players[LastQueriedKart].local || NetworkType == NetworkTypes.Host) {
                        var player = Players[LastQueriedKart];
                        var buff = Players[LastQueriedKart].SerializeLocation();
                        if (s.Length + buff.Length >= UDPPacket.MaxContentLength - 7) {
                            return s + "</karts>";
                        }
                        s += buff;
                    }
                }
            }
            return s + "</karts>";
        }
    }
}