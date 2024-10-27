using System.Net;
using TicTacToe.Models;

namespace TicTacToe.Interfaces
{
    public interface IUserInterface
    {
        IPAddress GetHostIpAddress();

        MoveModel GetMove();
    }
}
