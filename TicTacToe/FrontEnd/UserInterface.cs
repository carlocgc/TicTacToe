using System;
using System.Net;
using TicTacToe.Data;
using TicTacToe.Interfaces;
using TicTacToe.Models;

namespace TicTacToe.FrontEnd
{
    public class UserInterface : IUserInterface
    {
        private readonly GameProgressData _GameProgressData;

        public UserInterface(GameProgressData gameProgressData)
        {
            _GameProgressData = gameProgressData;
        }

        #region Implementation of IUserInterface

        public IPAddress GetHostIpAddress()
        {
            IPAddress ip = null;

            Boolean addressValid = false;

            while (!addressValid)
            {
                Console.WriteLine($"Enter host ip address...");
                addressValid = IPAddress.TryParse(Console.ReadLine() ?? "", out ip);
            }

            if (ip == null)
            {
                // TODO Handle no ip defined
                Console.WriteLine("IP was not defined, exiting...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            return ip;
        }

        public MoveModel GetMove()
        {
                       Boolean valid = false;

            Int32 x = 0;
            Int32 y = 0;

            String lastError = String.Empty;

            while (!valid)
            {
                Console.WriteLine();
                Console.WriteLine($"Enter a valid move in format \"X,X\" (0-2)");
                if (lastError != String.Empty)
                {
                    Console.WriteLine($"{lastError}");
                }

                String input = Console.ReadLine() ?? "";

                if (input.Length > 3)
                {
                    Console.WriteLine($"Too many characters, try again...");
                    continue;
                }

                if (input.Length < 3)
                {
                    Console.WriteLine($"Too few characters, try again...");
                    continue;
                }

                if (!input.Contains(","))
                {
                    Console.WriteLine($"Missing comma, try again...");
                    continue;
                }

                String[] parts = input.Split(',');

                if (!Int32.TryParse(parts[0], out Int32 tempX))
                {
                    Console.WriteLine($"{tempX} is not a number, try again...");
                    continue;
                }

                if (tempX < 0 || tempX > 2)
                {
                    Console.WriteLine($"{tempX} is out of bounds, must be between 0-2, try again...");
                    continue;
                }

                if (!Int32.TryParse(parts[1], out Int32 tempY))
                {
                    Console.WriteLine($"{tempY} is not a number, try again...");
                    continue;
                }

                if (tempY < 0 || tempY > 2)
                {
                    Console.WriteLine($"{tempY} is out of bounds, must be between 0-2, try again...");
                    continue;
                }

                if (!_GameProgressData.IsMoveValid(new MoveModel(tempX, tempY)))
                {
                    Console.WriteLine($"({tempX}, {tempY}) is already taken by \"{_GameProgressData.GameBoard[tempX, tempY]}\", try again...");
                    continue;
                }

                x = tempX;
                y = tempY;
                valid = true;
            }

            return new MoveModel(x, y);
        }

        #endregion
    }
}
