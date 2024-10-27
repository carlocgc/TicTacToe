using System;

namespace TicTacToe.Models
{
    public class MoveModel
    {
        public Int32 X { get; }

        public Int32 Y { get; }

        public MoveModel(Int32 x, Int32 y)
        {
            X = x;
            Y = y;
        }

        public override String ToString()
        {
            return $"{X},{Y}";
        }

        public static MoveModel FromString(String moveString)
        {
            String[] parts = moveString.Split(',');

            if (!Int32.TryParse(parts[0], out Int32 x))
            {
                String error = $"Error parsing string as a valid move...";
                throw new ArgumentException(error);
            }
            if (!Int32.TryParse(parts[1], out Int32 y))
            {
                String error = $"Error parsing string as a valid move...";
                throw new ArgumentException(error);
            }

            return new MoveModel(x, y);
        }
    }
}
