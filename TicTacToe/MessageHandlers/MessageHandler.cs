using System;
using TicTacToe.Data;
using TicTacToe.FrontEnd;
using TicTacToe.Interfaces;

namespace TicTacToe.MessageHandlers
{
    public class MessageHandler : IMessageHandler
    {
        private readonly GameProgressData _GameProgressData;
        private readonly ScreenDrawer _ScreenDrawer;

        public MessageHandler(GameProgressData gameProgressData, ScreenDrawer screenDrawer)
        {
            _GameProgressData = gameProgressData;
            _ScreenDrawer = screenDrawer;
        }

        #region Implementation of IMessageHandler

        public IMessageService _MessageService { get; set; }

        public void HandleMessage(String message)
        {
            _ScreenDrawer.Draw(_GameProgressData);
            Console.WriteLine($"{message}");
        }

        #endregion
    }
}
