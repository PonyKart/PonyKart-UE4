using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Ponykart.Players;

namespace Ponykart.Networking {
    public class NetworkEntity {
        public Player player;
        public bool local;
        NetworkManager nm;
        internal int _GlobalID;
        internal string _Name;
        private string _Selection;

        #region Properties

        /// <summary>
        /// Assigned global player ID
        /// </summary>
        public int GlobalID {
            get {
                return _GlobalID;
            }
        }
        /// <summary>
        /// Current player name
        /// </summary>
        public string Name {
            get {
                return _Name;
            }
        }
        /// <summary>
        /// Current Kart selection
        /// </summary>
        public string Selection {
            get {
                return _Selection ?? "Twilight Sparkle";
            }
        }
        #endregion

        public Connection owner;

        /// <summary>
        /// Create a new NetworkEntity that is fully designated by a remote host.
        /// </summary>
        public NetworkEntity(Connection parent, int globalid, string name, string selection, bool islocal) {
            nm = LKernel.Get<NetworkManager>();
            if (parent != null) { owner = parent; }
            local = islocal;
            _GlobalID = globalid;
            _Selection = selection;
            _Name = name;
        }
        /// <summary>
        /// Generate a new remote NetworkEntity at this host.
        /// </summary>
        public NetworkEntity(Connection parent) {
            nm = LKernel.Get<NetworkManager>();
            local = false;
            owner = parent;
            _GlobalID = nm.AssignGlobalID();
            _Name = String.Format("Ponefag{0}", _GlobalID);
            _Selection = "Twilight Sparkle";
        }

        /// <summary>
        /// Generate a new local NetworkEntity at this host.
        /// </summary>
        public NetworkEntity() {
            nm = LKernel.Get<NetworkManager>();
            local = true;
            owner = null;
            _GlobalID = nm.AssignGlobalID();
            _Selection = "Twilight Sparkle";
            _Name = String.Format("Ponefag{0}", _GlobalID);
        }

        /// <summary>
        /// Use to attempt to set the name among all instances.
        /// </summary>
        public void SetName(string name) {
            if (nm.NetworkType == NetworkTypes.Client) {
                owner.SendPacket(Commands.RequestPlayerChange, SerializeChange("Name",name));
            } else {
                _Name = name;
                nm.ForEachConnection((c) => c.SendPacket(Commands.PlayerChange, SerializeChange("Name", name)));
            }
        }

        /// <summary>
        /// Use to attempt to set the selection among all instances.
        /// </summary>
        public void SetSelection(string selection) {
            if (nm.NetworkType == NetworkTypes.Client) {
                owner.SendPacket(Commands.RequestPlayerChange, SerializeChange("Selection", selection));
            } else {
                _Selection = selection;
                nm.ForEachConnection((c) => c.SendPacket(Commands.PlayerChange, SerializeChange("Selection", selection)));
            }
        }
        /// <summary>
        /// Turn this NetworkEntity into a string that can be sent to a client
        /// </summary>
        public string Serialize() {
            var SWriter = new StringWriter();
            var XWriter = new XmlTextWriter(SWriter);
            
            var AsXML = new XElement("Entity", new XAttribute("Name",Name),
                new XAttribute("Id",_GlobalID), new XAttribute("Selection", _Selection));

            AsXML.WriteTo(XWriter);
            return SWriter.ToString();
        }

        /// <summary>
        /// Turn a string representing a player into a NetworkEntity
        /// </summary>
        public static NetworkEntity Deserialize(string contents, Connection parent, bool local) {
            var AsXML = XElement.Parse(contents);

            var Name = AsXML.Attribute("Name").Value;
            var ID = int.Parse(AsXML.Attribute("Id").Value);
            var Selection = AsXML.Attribute("Selection").Value;

            return new NetworkEntity(parent, ID, Name, Selection, local);
        }

        /// <summary>
        /// Turn a status change into an XML string
        /// </summary>
        public string SerializeChange(string index, string val) {
            var SWriter = new StringWriter();
            var XWriter = new XmlTextWriter(SWriter);

            var AsXML = new XElement("Change", new XAttribute("Id", _GlobalID),
                new XAttribute("Property", index), new XAttribute("Value", val));

            AsXML.WriteTo(XWriter);
            return SWriter.ToString();
        }

        /// <summary>
        /// Changes this entity's properties according to the given xml schema
        /// </summary>
        public static bool PerformChange(string contents, Connection sender) {
            var AsXML = XElement.Parse(contents);

            var Property = AsXML.Attribute("Property").Value;
            var Value = AsXML.Attribute("Value").Value;
            var ID = int.Parse(AsXML.Attribute("Id").Value);

            NetworkEntity target = LKernel.Get<NetworkManager>().Players.Find((e) => e.GlobalID == ID);
            if (target == null) { return false; }
            if (LKernel.Get<NetworkManager>().NetworkType == NetworkTypes.Host &&  target.owner != sender) { return false; }
            switch (Property) {
                case "Selection":
                    target._Selection = Value;
                    break;
                case "Name":
                    target._Name = Value;
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Turn this player's current location, velocity, and orientation into a string.
        /// </summary>
        public string SerializeLocation() {
            try {
                var nm = LKernel.Get<NetworkManager>();
                var pos = player.Kart.Body.CenterOfMassPosition;
                var vel = player.Kart.Body.LinearVelocity;
                var orn = player.Kart.Body.Orientation;
                var XKart = new XElement("Kart", new XAttribute("Id", _GlobalID),
                                                new XAttribute("Pos", String.Format("{0} {1} {2}", pos.x, pos.y, pos.z)),
                                                new XAttribute("Vel", String.Format("{0} {1} {2}", vel.x, vel.y, vel.z)),
                                                new XAttribute("Or", String.Format("{0} {1} {2} {3}", orn.w, orn.x, orn.y, orn.z)));
                return XKart.ToString();
            } catch (Exception e) { return ""; }
        }

        /// <summary>
        /// Perform the given location change if valid.
        /// </summary>
        public static void DeserializeLocations(string contents, Connection sender) {
            var AsXML = XElement.Parse(contents);

            // anonymous types!
            var Karts = (from x in AsXML.Elements("Kart") 
                         select new {
                             IDStr = x.Attribute("Id").Value,
                             PositionStr = x.Attribute("Pos").Value,
                             SpeedStr = x.Attribute("Vel").Value, 
                             OrientationStr = x.Attribute("Or").Value
                         }).ToDictionary((a) => Int32.Parse(a.IDStr));
            foreach (NetworkEntity ne in LKernel.Get<NetworkManager>().Players) {
                if (Karts.ContainsKey(ne.GlobalID) && !ne.local) {
                    if (ne.owner == sender || ne.nm.NetworkType == NetworkTypes.Client) {
                        try {
                            var Kart = Karts[ne.GlobalID];
                            var PosList = Kart.PositionStr.Split(' ').Select((s) => float.Parse(s)).ToList();
                            var SpeedList = Kart.SpeedStr.Split(' ').Select((s) => float.Parse(s)).ToList();
                            var OrList = Kart.OrientationStr.Split(' ').Select((s) => float.Parse(s)).ToList();
                            var Pos = new Mogre.Vector3(PosList[0], PosList[1], PosList[2]);
                            var Speed = new Mogre.Vector3(SpeedList[0], SpeedList[1], SpeedList[2]);
                            var Or = new Mogre.Quaternion(OrList[0], OrList[1], OrList[2], OrList[3]);
                            ne.player.Kart.SetState(Pos, Speed, Or);
                        } catch (Exception e) { }
                    }
                }
            }
            return;
        }
    }
}
