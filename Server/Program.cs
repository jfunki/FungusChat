using Server.Net.IO;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Server
{
    class Program
    {
        static List<Client> _users;
        static TcpListener _listener;
        static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1860);
            _listener.Start();


            while (true)
            {
                var client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);
                BroadCastConnection();
            }
            
            
            
        }

        static void BroadCastConnection()
        {
            foreach(var user in _users)
            {
                foreach(var usr in _users)
                {
                    var broadcastpacket = new PacketBuilder();
                    broadcastpacket.WriteOpCode(1);
                    broadcastpacket.WriteMessage(usr.Username);
                    broadcastpacket.WriteMessage(usr.UID.ToString());
                    user.ClientSocket.Client.Send(broadcastpacket.GetPacketBytes());
                }
            }
        }

        public static void BroadCastMessage(string message)
        {
            foreach (var user in _users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(5);
                msgPacket.WriteMessage(message);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }

        public static void BroadCastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();

            _users.Remove(disconnectedUser);
            foreach (var user in _users)
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(10);
                broadcastPacket.WriteMessage(uid);
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }

            BroadCastMessage($"[{disconnectedUser.Username}] hat den Chat verlassen");
        }
    }
}