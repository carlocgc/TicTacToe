using System;
using TicTacToe.Data;
using TicTacToe.Interfaces;

namespace TicTacToe.MessageHandlers
{
    public class MoveConfirmHandler : IMessageHandler
    {
        private readonly PlayerTurnData _PlayerTurnData;

        public MoveConfirmHandler(PlayerTurnData playerTurnData)
        {
            _PlayerTurnData = playerTurnData;
        }

        #region Implementation of IMessageHandler

        public IMessageService _MessageService { get; set; }

        public void HandleMessage(String message)
        {
            _PlayerTurnData.WaitingForHost = false;
        }

        #endregion
    }
}
