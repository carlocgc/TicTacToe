using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Interfaces;

namespace TicTacToe.MessageHandlers
{
    public class GameDrawHandler : IMessageHandler
    {
        #region Implementation of IMessageHandler

        public IMessageService _MessageService { get; set; }

        public void HandleMessage(String message)
        {

        }

        #endregion
    }
}
