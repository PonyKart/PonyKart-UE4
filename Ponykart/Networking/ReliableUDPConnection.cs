using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace Ponykart.Networking {
    public delegate void PacketHandler(UDPPacket P);
    public class ReliableUDPConnection {
        private UdpClient Sender;
        public UInt32 ConnectionID;
        public byte[] IDArray;
        public IPEndPoint DestinationEP;
        private Dictionary<UInt32,UDPPacket> Sent;
        private HashSet<UInt32> Received;

        private UInt32 _AckField = 0;
        private UInt32 _RemoteSeqNo = 0;

        public UInt32 RemoteSeqNo {
            get {
                var tmp = SpecialAck ?? _RemoteSeqNo;
                SpecialAck = null;
                return tmp;
            }
        }

        public UInt32 AckField {
            get {
                var tmp = SpecialField ?? _AckField;
                SpecialField = null;
                return tmp;
            }
        }
        int sequenceNo = 0;
        long LastReceivedTicks = 0;
        Connection Owner;
        public event PacketHandler OnPacketRecv;
        UInt32? SpecialAck = null;
        UInt32? SpecialField = null;

        public ReliableUDPConnection(UdpClient sender, IPEndPoint destinationep, UInt32 cid, Connection owner) {
            DestinationEP = destinationep;
            ConnectionID = cid;
            Sender = sender;
            Sent = new Dictionary<UInt32,UDPPacket>();
            Owner = owner;
            Received = new HashSet<UInt32>();
            OnPacketRecv += new PacketHandler(RegisterPacket);
            OnPacketRecv += new PacketHandler(OnPacket);
        }

        /// <summary>
        /// Determine if a given UDP packet has been seen before (this is a resend)
        /// </summary>
        bool DuplicatePacket(UDPPacket p) { 
            return Received.Contains(p.SequenceNo);
        }

        /// <summary>
        /// Notes that we have received this packet, keeping track via sequence number
        /// </summary>
        void RegisterPacket(UDPPacket p) {
            Received.Add(p.SequenceNo);
        }

        /// <summary>
        /// Called whenever we receive a packet.
        /// </summary>
        public void Handle(UDPPacket p) {
            AddAck(p);
            ProcessAcks(p.Ack, p.AckField);
            if (!DuplicatePacket(p)) {
                OnPacketRecv(p);
            } 
        }

        public void OnPacket(UDPPacket p) {
            if (p.Contents.Type != Commands.NoMessage && p.Contents.Type != Commands.SendPositions) {
                Launch.Log(String.Format("[Networking] Got packet of type {0}", p.Contents.Type));
            }
        }
            
        /// <summary>
        /// Adds a packet that has been received to the next ack that will be sent
        /// </summary>
        /// <param name="p"></param>
        void AddAck(UDPPacket p) {
            LastReceivedTicks = System.DateTime.Now.Ticks;
            if (p.SequenceNo < RemoteSeqNo - 32) {
                PrepareSpecialAck(p.SequenceNo);
            }
            // evil bit-field hacking. TODO: explain?
            if (p.SequenceNo > RemoteSeqNo) {
                _AckField <<= (int)(p.SequenceNo - RemoteSeqNo);
            }
            _AckField |= (UInt32)(1 << (int)(RemoteSeqNo - p.SequenceNo));
            if (p.SequenceNo >= RemoteSeqNo) {
                _RemoteSeqNo = p.SequenceNo;
            }
        }

        private void PrepareSpecialAck(uint p) {
            SpecialAck = p;
            SpecialField = 1;
        }

        /// <summary>
        /// Reads the acks from a packet and notes which packets have been received
        /// </summary>
        /// <param name="Ack"></param>
        /// <param name="AckField"></param>
        void ProcessAcks(UInt32 Ack, UInt32 AckField) {
            // evil bit-field hacking. TODO: explain?
            for (int i = 0; i < 32; i++) {
                if (((int)AckField & 1) == 1) {
                    if (Sent.ContainsKey((UInt32)(Ack - i))) {
                        var packet = Sent[(UInt32)(Ack - i)];
                        if (packet.Contents.Type != Commands.NoMessage && !packet.Responded)  {
                            Launch.Log(String.Format("Remote partner received packet {0} type {1}", packet.SequenceNo,
                               packet.Contents.Type));
                        }
                        packet.Responded = true;
                    }
                }
                AckField >>= 1;
            }
        }


        public UInt32 NextSequenceNumber {
            get {
                return (UInt32)sequenceNo++;
            }
        }
        bool SendPacket(UDPPacket packet) {
            packet.LastSent = DateTime.Now;
            var bytes = packet.ToBytes();
            if (Sender.Send(bytes, bytes.Length, DestinationEP) == 0) {
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Resends any packets that have not received a reply
        /// </summary>
        public void ResendPackets() {
            var unresponded = from message in Sent.Values
                              where !message.Responded
                              where (DateTime.Now - message.LastSent) > TimeSpan.FromSeconds(5)
                              select message;
            foreach (var message in unresponded) {
                Launch.Log(String.Format("[Networking] Resent packet {0} due to possible timeout", message.SequenceNo)); 
                //message.SequenceNo = NextSequenceNumber;
                SendPacket(message);
            }
        }

        public void AddPacket(UDPPacket message) {            
            Sent[message.SequenceNo] = message;
        }

        public void Send() {
            PonykartPacket TopPacket;
            try {
                TopPacket = Owner.TopMessage.ToPKPacket(Owner);
            } catch (Exception e) { return; } 
            var message = new UDPPacket(TopPacket,this);
            if (!TopPacket.Volatile) {
                AddPacket(message);
            }
            SendPacket(message);
        }

        public void Close() {
            Sender.Close();
        }
    }
}
