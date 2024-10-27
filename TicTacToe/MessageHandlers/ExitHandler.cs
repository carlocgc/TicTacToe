using System;
using TicTacToe.Interfaces;

namespace TicTacToe.MessageHandlers
{
    public class ExitHandler : IMessageHandler
    {
        #region Implementation of IMessageHandler

        public IMessageService _MessageService { get; set; }

        public void HandleMessage(String message)
        {
            Console.WriteLine($"Game exiting...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        #endregion
    }
}
