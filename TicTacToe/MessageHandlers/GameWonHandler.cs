using System;
using TicTacToe.Data;
using TicTacToe.FrontEnd;
using TicTacToe.Interfaces;
using TicTacToe.Models;
using static TicTacToe.Data.StaticGameData;

namespace TicTacToe.MessageHandlers
{
    public class GameWonHandler : IMessageHandler
    {
        private readonly ScreenDrawer _ScreenDrawer;

        private readonly GameProgressData _GameProgressData;

        private readonly PlayerTurnData _PlayerTurnData;

        private readonly IMessageService _MessageService;

        public GameWonHandler(IMessageService messageService, ScreenDrawer screenDrawer, GameProgressData gameProgressData, PlayerTurnData playerTurnData)
        {
            _MessageService = messageService;
            _ScreenDrawer = screenDrawer;
            _GameProgressData = gameProgressData;
            _PlayerTurnData = playerTurnData;
        }

        #region Implementation of IMessageHandler

        public void HandleMessage(String message)
        {
            if (!Char.TryParse(message, out Char winner)) return;

            if (_MessageService.IsHost)
            {
                _MessageService.SendPacket(new Packet(Command.GAME_WON.ToString(), MASTER_CHAR.ToString()));
                Packet packet = _MessageService.AwaitPacket();
                if (Enum.TryParse(packet.Command, out Command command))
                {
                    if (command == Command.PACKET_RECEIVED)
                    {
                        _MessageService.SendPacket(_GameProgressData.GameBoardAsPacket());
                    }
                }
            }
            else
            {
                _MessageService.SendPacket(new Packet(Command.PACKET_RECEIVED.ToString()));
                Packet packet = _MessageService.AwaitPacket();
                //HandlePacket(packet);
                // TODO Handle this specific packet
            }

            _ScreenDrawer.Draw(_GameProgressData);
            Console.WriteLine(winner == _GameProgressData.PlayerSymbol ? $"Congratulations, you won!" : $"Unlucky, you lost!");

            if (message == MASTER_CHAR.ToString())
            {
                _GameProgressData.HostScore++;
            }
            else
            {
                _GameProgressData.ClientScore++;
            }

            _GameProgressData.GameWon = true;
            _PlayerTurnData.WaitingForHost = false;
        }

        #endregion
    }
}
