using System;
using TicTacToe.Data;
using TicTacToe.Interfaces;
using TicTacToe.Models;

namespace TicTacToe.MessageHandlers
{
    public class MoveRequestHandler : IMessageHandler
    {
        private readonly GameProgressData _GameProgressData;

        private readonly PlayerTurnData _PlayerTurnData;

        private readonly IMessageHandler _GameWonHandler;

        private readonly IMessageService _MessageService;

        public MoveRequestHandler(IMessageService messageService, GameProgressData gameProgressData, PlayerTurnData playerTurnData, IMessageHandler gameWonHandler)
        {
            _MessageService = messageService;
            _GameProgressData = gameProgressData;
            _PlayerTurnData = playerTurnData;
            _GameWonHandler = gameWonHandler;
        }

        #region Implementation of IMessageHandler

        public void HandleMessage(String message)
        {
            MoveModel move = MoveModel.FromString(message);

            if (_GameProgressData.IsMoveValid(move))
            {
                _GameProgressData.GameBoard[move.X, move.Y] = StaticGameData.SLAVE_CHAR;

                if (_GameProgressData.IsGameWon(StaticGameData.SLAVE_CHAR))
                {
                    _MessageService.SendPacket(new Packet(StaticGameData.Command.GAME_WON.ToString(), StaticGameData.SLAVE_CHAR.ToString()));
                    Packet wonResp = _MessageService.AwaitPacket();

                    if (!Enum.TryParse(wonResp.Command, out StaticGameData.Command wonRespCommand)) return;

                    if (wonRespCommand == StaticGameData.Command.PACKET_RECEIVED)
                    {
                        _GameWonHandler.HandleMessage(StaticGameData.SLAVE_CHAR.ToString());
                    }
                }
                else
                {
                    _MessageService.SendPacket(_GameProgressData.GameBoardAsPacket());
                }

                Packet resp = _MessageService.AwaitPacket();

                if (!Enum.TryParse(resp.Command, out StaticGameData.Command command)) return;

                if (command == StaticGameData.Command.PACKET_RECEIVED)
                {
                    _PlayerTurnData.WaitingForClient = false;
                }
            }
            else
            {
                _MessageService.SendPacket(new Packet(StaticGameData.Command.MOVE_DENY.ToString()));
            }
        }

        #endregion
    }
}
