using System;
using TicTacToe.Interfaces;

namespace TicTacToe.MessageHandlers
{
    public class MoveDenyHandler : IMessageHandler
    {
        #region Implementation of IMessageHandler

        public IMessageService _MessageService { get; set; }

        public void HandleMessage(String message)
        {
            Console.WriteLine($"Move was denied by host...");

            // TODO Get another move from the client
        }

        #endregion
    }
}
