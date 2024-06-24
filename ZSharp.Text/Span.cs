namespace ZSharp.Text
{
    public readonly struct Span
    {
        public Position Start { get; }
        public Position End { get; }

        public Span(Position start, Position end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"{Start} - {End}";
        }

        public static Span FromBounds(int startLine, int startColumn, int endLine, int endColumn)
        {
            return new Span(new Position(startLine, startColumn), new Position(endLine, endColumn));
        }

        public static Span FromBounds(Position start, Position end)
        {
            return new Span(start, end);
        }

        public static Span FromBounds(Position start, int endLine, int endColumn)
        {
            return new Span(start, new Position(endLine, endColumn));
        }

        public static Span FromBounds(int startLine, int startColumn, Position end)
        {
            return new Span(new Position(startLine, startColumn), end);
        }

        public static Span Combine(params Span[] spans)
        {
            if (spans.Length == 0)
            {
                throw new ArgumentException("At least one span must be provided");
            }

            var start = new Position(int.MaxValue, int.MaxValue);
            var end = new Position(int.MinValue, int.MinValue);

            foreach (var span in spans)
            {
                start = new Position(Math.Min(start.Line, span.Start.Line), Math.Min(start.Column, span.Start.Column));
                end = new Position(Math.Max(end.Line, span.End.Line), Math.Max(end.Column, span.End.Column));
            }

            return new Span(start, end);
        }
    }
}
