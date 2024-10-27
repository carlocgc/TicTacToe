using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TicTacToe.Interfaces;
using TicTacToe.Models;

namespace TicTacToe.Services
{
    /// <summary> Manages the connection between two players, can send and receive message packets </summary>
    public class MessageService : IMessageService
    {
        /// <summary> The local ip address </summary>
        private readonly IPAddress _LocalAddress;
        /// <summary> public ip address </summary>
        private readonly IPAddress _PublicAddress;
        /// <summary> The message port </summary>
        private readonly Int32 _LocalPort;
        /// <summary> Waits for a client and connected them </summary>
        private readonly TcpListener _ServerListener;
        /// <summary> Connected client </summary>
        private TcpClient _Client;
        /// <summary> The clients message stream </summary>
        private NetworkStream _MsgStream;

        /// <summary> Whether the message service is connected to the client </summary>
        public Boolean Connected { get; private set; }

        public Boolean IsHost { get; private set; }

        public MessageService()
        {
            try
            {
                _LocalAddress = GetLocalIpAddress();
                _PublicAddress = GetPublicIpAddress();
            }
            catch (Exception e)
            {
                Console.WriteLine("Network error: " + e.Message);
                Console.WriteLine("Game exiting...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            _LocalPort = 6600;
            _ServerListener = new TcpListener(_LocalAddress, _LocalPort);
            Connected = false;
        }

        /// <summary> Gets the environment IPv4 address </summary>
        /// <returns></returns>
        private IPAddress GetLocalIpAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipAddress;
                }
            }
            throw new Exception("No local IPv4 device detected for this environment.");
        }

        /// <summary>
        /// Gets the public ip address of the network
        /// </summary>
        /// <returns></returns>
        private IPAddress GetPublicIpAddress()
        {
            String address;

            WebRequest req = WebRequest.Create("http://checkip.dyndns.org/");

            using (WebResponse response = req.GetResponse())
            {
                try
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
                    {
                        address = reader.ReadToEnd();
                    }
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"Could not get public ip address: {e.Message}");
                    return null;
                }
            }

            Int32 first = address.IndexOf("Address: ", StringComparison.Ordinal) + 9;
            Int32 last = address.LastIndexOf("</body>", StringComparison.Ordinal);
            address = address.Substring(first, last - first);

            if (IPAddress.TryParse(address, out IPAddress addressParsed))
            {
                return addressParsed;
            }

            Console.WriteLine("Could not determine public address");
            return null;
        }

        /// <summary>
        /// Asks the player if they are a host or a client and configures the network accordingly
        /// </summary>
        public void Initialise()
        {
            Boolean valid = false;

            while (!valid)
            {
                Console.Clear();

                Console.WriteLine("Welcome to TicTacToe");
                Console.WriteLine($"----------------------");
                Console.WriteLine($"Local address: {_LocalAddress}:{_LocalPort}");
                //Console.WriteLine($"Public address: {_PublicAddress}:{_LocalPort}");
                Console.WriteLine($"----------------------");
                Console.WriteLine($"1. Host");
                Console.WriteLine($"2. Client");
                Console.WriteLine($"----------------------");
                Console.WriteLine($"Enter selection...");

                String resp = Console.ReadLine();

                if (resp == null) continue;

                switch (resp.ToLower())
                {
                    case "host":
                    case "1":
                        {
                            IsHost = true;
                            valid = true;
                            break;
                        }
                    case "client":
                    case "2":
                        {
                            IsHost = false;
                            valid = true;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine($"Invalid option...");
                            Console.WriteLine($"Press enter to try again.");
                            Console.ReadKey();
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Wait for an opponent, called by the host game
        /// </summary>
        public void WaitForClient()
        {
            Console.WriteLine($"Listening at {_LocalAddress} on port {_LocalPort}.");
            Console.WriteLine($"Waiting for opponent...");

            _ServerListener.Start();

            while (!Connected)
            {
                if (!_ServerListener.Pending()) continue;

                _Client = _ServerListener.AcceptTcpClient();

                if (!_Client.Connected) continue;

                Connected = true;
                Console.WriteLine($"Client {_Client.Client.RemoteEndPoint} has connected to the game.");
            }

            _MsgStream = _Client.GetStream();
        }

        /// <summary>
        /// Connect to a host, called by the client game
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public Boolean ConnectToHost(IPAddress hostAddress, Int32 port)
        {
            _Client = new TcpClient();

            while (!_Client.Connected)
            {
                try
                {
                    _Client.Connect(hostAddress, port);
                }
                catch (SocketException e)
                {
                    Console.Clear();
                    Console.WriteLine($"Host {hostAddress}:{port} not found: {e.Message}");
                }
                Thread.Sleep(10);
            }

            Connected = true;
            _MsgStream = _Client.GetStream();

            Console.WriteLine($"Connected to host game {_Client.Client.RemoteEndPoint}");

            return Connected;
        }

        /// <summary>
        /// Send a message packet
        /// </summary>
        /// <param name="packet"></param>
        public void SendPacket(Packet packet)
        {
            Byte[] jsonBuffer = Encoding.UTF8.GetBytes(packet.ToJson());

            // Int16 is 2 bytes big, all messages will be prefixed with message size so we know how much data to read from the stream
            // This is in case we have more than one waiting message
            Int16 size = (Int16)jsonBuffer.Length;
            Byte[] sizeBuffer = BitConverter.GetBytes(size);

            Byte[] packetBuffer = new Byte[sizeBuffer.Length + jsonBuffer.Length];

            sizeBuffer.CopyTo(packetBuffer, 0);
            jsonBuffer.CopyTo(packetBuffer, sizeBuffer.Length);

            _MsgStream.Write(packetBuffer, 0, packetBuffer.Length);
        }

        /// <summary>
        /// Waits until a packet is received and returns it
        /// </summary>
        /// <returns></returns>
        public Packet AwaitPacket()
        {
            Boolean waiting = true;
            Packet packet = new Packet();

            while (waiting)
            {
                if (_Client.Available <= 0) continue;

                // We have a message, get the size of the message from the first 2 bytes
                Byte[] sizeBuffer = new Byte[2];
                _MsgStream.Read(sizeBuffer, 0, sizeBuffer.Length);
                Int16 messageSize = BitConverter.ToInt16(sizeBuffer, 0);

                // Get that much data from the stream, that must be the message
                Byte[] jsonBuffer = new Byte[messageSize];
                _MsgStream.Read(jsonBuffer, 0, jsonBuffer.Length);

                String jsonString = Encoding.UTF8.GetString(jsonBuffer);
                packet = Packet.FromJson(jsonString);
                waiting = false;
            }

            return packet;
        }
    }
}
