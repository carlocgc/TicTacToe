using System;
using TicTacToe.Data;
using TicTacToe.Interfaces;

namespace TicTacToe.FrontEnd
{
    public class ScreenDrawer
    {
        private readonly IMessageService _MessageService;

        public ScreenDrawer(IMessageService messageService)
        {
            _MessageService = messageService;
        }

        public void Draw(GameProgressData model)
        {

            Console.Clear();
            Console.WriteLine($"_____________________________");
            Console.WriteLine($"          TicTacToe.");
            Console.WriteLine();
            Console.WriteLine($"     Your Symbol: \"{model.PlayerSymbol}\"");
            Console.WriteLine();
            Console.WriteLine($"        Host Score: {model.HostScore}");
            Console.WriteLine($"     Client Score: {model.ClientScore}");
            Console.WriteLine($"_____________________________");
            Console.WriteLine();
            Console.WriteLine($"      {model.GameBoard[0, 0]}   |   {model.GameBoard[0, 1]}   |   {model.GameBoard[0, 2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {model.GameBoard[1, 0]}   |   {model.GameBoard[1, 1]}   |   {model.GameBoard[1, 2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {model.GameBoard[2, 0]}   |   {model.GameBoard[2, 1]}   |   {model.GameBoard[2, 2]}   ");
            Console.WriteLine($"_____________________________");
        }
    }
}
