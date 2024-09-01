namespace ZSharp.Text
{
    public readonly struct Position
    {
        public int Line { get; }
        public int Column { get; }

        public Position(int line, int column)
        {
            Line = line;
            Column = column;
        }

        public override readonly string ToString()
        {
            return $"{Line}:{Column}";
        }

        public static Position Min(Position left, Position right)
        {
            if (left.Line < right.Line) return left;
            if (right.Line < left.Line) return right;
            if (left.Column < right.Column) return left;
            return right;
        }

        public static Position Max(Position left, Position right)
        {
            if (left.Line > right.Line) return left;
            if (right.Line > left.Line) return right;
            if (left.Column > right.Column) return left;
            return right;
        }
    }
}
