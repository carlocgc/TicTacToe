using System;
using System.Net;
using TicTacToe.Data;
using TicTacToe.Models;

namespace TicTacToe.Interfaces
{
    public interface IMessageService
    {
        Boolean Connected { get; }

        Boolean IsHost { get; }

        Packet AwaitPacket();

        Boolean ConnectToHost(IPAddress hostAddress, Int32 port);

        void Initialise();

        void SendPacket(Packet packet);

        void WaitForClient();
    }
}