using System;
using System.Linq;
using TicTacToe.Models;
using static TicTacToe.Data.StaticGameData;

namespace TicTacToe.Data
{
    public class GameProgressData
    {
        public Int32 ClientScore { get; set; }

        public Int32 HostScore { get; set; }

        public Char[,] GameBoard { get; set; }

        public Char PlayerSymbol { get; set; }

        public Boolean GameWon { get; set; }

        public GameProgressData()
        {
            ResetGame();
        }

        public Boolean IsGameWon(char symbol)
        {
            if (GameBoard[0, 0] == symbol && GameBoard[0, 1] == symbol && GameBoard[0, 2] == symbol) return true;
            if (GameBoard[1, 0] == symbol && GameBoard[1, 1] == symbol && GameBoard[1, 2] == symbol) return true;
            if (GameBoard[2, 0] == symbol && GameBoard[2, 1] == symbol && GameBoard[2, 2] == symbol) return true;
            if (GameBoard[0, 0] == symbol && GameBoard[1, 1] == symbol && GameBoard[2, 2] == symbol) return true;
            if (GameBoard[0, 2] == symbol && GameBoard[1, 1] == symbol && GameBoard[2, 0] == symbol) return true;
            if (GameBoard[0, 0] == symbol && GameBoard[1, 0] == symbol && GameBoard[2, 0] == symbol) return true;
            if (GameBoard[0, 1] == symbol && GameBoard[1, 1] == symbol && GameBoard[2, 1] == symbol) return true;
            if (GameBoard[0, 2] == symbol && GameBoard[1, 2] == symbol && GameBoard[2, 2] == symbol) return true;
            return false;
        }

        /// <summary>
        /// returns whether all the spaces on the board are taken
        /// </summary>
        /// <returns></returns>
        public Boolean IsGameDrawn()
        {
            return GameBoard.Cast<Char>().All(c => c != '-');
        }

        public Boolean IsMoveValid(MoveModel move)
        {
            return GameBoard[move.X, move.Y] == '-';
        }

        public Packet GameBoardAsPacket()
        {
            String gameBoardString = $"{GameBoard[0, 0]}:{GameBoard[0, 1]}:{GameBoard[0, 2]}:" +
                $"{GameBoard[1, 0]}:{GameBoard[1, 1]}:{GameBoard[1, 2]}:" +
                $"{GameBoard[2, 0]}:{GameBoard[2, 1]}:{GameBoard[2, 2]}";

            return new Packet(Command.BOARD_STATE.ToString(), gameBoardString);
        }

        public void ResetGame()
        {
            GameBoard = new[,] { { '-', '-', '-' }, { '-', '-', '-' }, { '-', '-', '-' } };

            GameWon = false;
        }
    }
}

