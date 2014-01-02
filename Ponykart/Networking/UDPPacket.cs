using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ponykart.Networking  {
    /// <summary>
    /// Represents a UDP Packet - Protocol ID, Connection ID, Sequence Number, acks, and a message.
    /// </summary>
    public class UDPPacket {
        public readonly byte[] Protocol;
        public readonly UInt32 CID;
        public UInt32 SequenceNo;
        public readonly UInt32 Ack;
        public readonly UInt32 AckField;
        public readonly PonykartPacket Contents;
        public DateTime LastSent;
        public bool Responded;
        public static int MaxContentLength = 512 - 32;
        /// <summary>
        /// Get specific header information from a packet
        /// Packet format: PPPPIIIISSSSAAAAFFFFCCCCCCCCCCC
        ///                0   4   8   12  16  20
        /// Protocol, Connection ID, Sequence no, Ack+Field, Contents
        /// </summary>
        static byte[] GetProtocol(byte[] packet)  {
            var protocol = new byte[4];
            Array.Copy(packet, protocol, 4);
            return protocol;
        }
        static UInt32 GetCID(byte[] packet) {
            return BitConverter.ToUInt32(packet,4);
        }
        static UInt32 GetSequenceNo(byte[] packet) {
            return BitConverter.ToUInt32(packet, 8);
        }
        static UInt32 GetAck(byte[] packet) {
            return BitConverter.ToUInt32(packet,12);
        }
        static UInt32 GetAckField(byte[] packet) {
            return BitConverter.ToUInt32(packet,16);
        }

        /// <summary>
        /// Strips out header information from a packet and returns the contents
        /// </summary>
        static byte[] GetContents(byte[] packet)  {
            var contents = new byte[packet.Length - 20];
            Array.Copy(packet, 20, contents, 0, packet.Length - 20);
            return contents;
        }

        public UDPPacket(byte[] creator) {
            Protocol = GetProtocol(creator);
            CID = GetCID(creator);
            SequenceNo = GetSequenceNo(creator);
            Ack = GetAck(creator);
            AckField = GetAckField(creator);
            Contents = new PonykartPacket(GetContents(creator));
            Responded = false;
        }

        public UDPPacket(PonykartPacket contents, ReliableUDPConnection sender) {
            Protocol = NetworkManager.Protocol;
            CID = (UInt32)sender.ConnectionID;
            SequenceNo = sender.NextSequenceNumber;
            Ack = sender.RemoteSeqNo;
            AckField = sender.AckField;
            Contents = contents;
            Responded = contents.Volatile;
        }
        /// <summary>
        /// Creates a packet byte array.
        /// </summary>
        public byte[] ToBytes() {
            var contentBytes = this.Contents.ToBytes();
            var packet = new byte[contentBytes.Length + 20];
            Array.Copy(NetworkManager.Protocol,0, packet, 0, 4);
            Array.Copy(BitConverter.GetBytes(CID), 0, packet, 4, 4);
            Array.Copy(BitConverter.GetBytes(SequenceNo), 0, packet, 8, 4);
            Array.Copy(BitConverter.GetBytes(Ack),0, packet, 12, 4);
            Array.Copy(BitConverter.GetBytes(AckField), 0, packet, 16, 4);
            Array.Copy(contentBytes, 0, packet, 20, contentBytes.Length);
            return packet;
        }
    }
}
