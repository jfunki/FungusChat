using FungusChat.MVVM.Model;
using FungusChat.MVVM.ViewModel;
using FungusChat.Net.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace FungusChat.Net
{
    internal class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;
        MainViewModel MainViewModel;
        public event Action connectedEvent;
        public event Action userDisconnectEvent;
        public event Action msgReceivedEvent;
        private UserModel _userModel;
        public Server()
        {
            _client = new TcpClient();
            _userModel = new UserModel();
        }

        // Zum Server connecten

        public void ConnectToServer(string username)
        {
            // Schauen ob Client schon verbunden ist
            if(!_client.Connected)
            {
                _client.Connect("127.0.0.1", 1860);

                PacketReader = new PacketReader(_client.GetStream());

                if(!string.IsNullOrEmpty(username))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();

                
            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("okay..");
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
            MessageToBackupFile(message);
        }

        public void MessageToBackupFile(string message)
        {
            using (StreamWriter writer = File.AppendText(@"C:\Users\ASUS\OneDrive\Desktop\Backup.txt"))
            {
                writer.WriteLine($"[{DateTime.Now}]: [{_userModel.Username}]: {message}");
            }
        }
    }
}
