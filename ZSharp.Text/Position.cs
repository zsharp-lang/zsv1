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
    }
}
