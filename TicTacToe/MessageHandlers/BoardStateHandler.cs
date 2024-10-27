using System;
using TicTacToe.Data;
using TicTacToe.Interfaces;

namespace TicTacToe.MessageHandlers
{
    public class BoardStateHandler : IMessageHandler
    {
        private readonly GameProgressData _GameProgressData;

        private readonly PlayerTurnData _PlayerTurnData;

        public BoardStateHandler(GameProgressData gameProgressData, PlayerTurnData playerTurnData)
        {
            _GameProgressData = gameProgressData;
            _PlayerTurnData = playerTurnData;
        }

        #region Implementation of IMessageHandler

        public IMessageService _MessageService { get; set; }

        public void HandleMessage(String message)
        {
            String[] parts = message.Split(':');
            Int32 count = 0;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    _GameProgressData.GameBoard[x, y] = Char.Parse(parts[count]);
                    count++;
                }
            }

            _PlayerTurnData.WaitingForHost = false;
        }

        #endregion
    }
}
