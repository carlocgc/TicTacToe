using System;

namespace TicTacToe.Data
{
    /// <summary> Global static game data </summary>
    public static class StaticGameData
    {
        /// <summary> Port the game communicates on </summary>
        public const Int32 GAME_PORT = 6600;
        /// <summary> Symbol that represents the master </summary>
        public const Char MASTER_CHAR = 'X';
        /// <summary> Symbol that represents the slave </summary>
        public const Char SLAVE_CHAR = 'O';

        /// <summary>
        /// Command type, determines how a message is to be processed
        /// </summary>
        public enum Command
        {
            MESSAGE,
            PACKET_RECEIVED,
            MOVE_REQUEST,
            MOVE_CONFIRM,
            MOVE_DENY,
            BOARD_STATE,
            GAME_WON,
            GAME_DRAW,
            EXIT,
        }
    }
}
