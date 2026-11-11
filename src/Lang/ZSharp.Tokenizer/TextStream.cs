using ZSharp.Text;

namespace ZSharp.Tokenizer
{
    public sealed class TextStream(TextReader stream)
    {
        private readonly TextReader stream = stream;

        public int Index { get; private set; }

        public Position Position { get; private set; } = new(1, 1);

        public bool IsEOF { get; private set; }

        public TextStream(string content)
            : this(new StringReader(content)) { }

        public string ReadLine()
        {
            string line = stream.ReadLine()!;
            Position = new(Position.Line + 1, 1);
            return line;
        }

        public char Peek()
            => Peek(false);

        public char Read()
        {
            char c = Peek(true);

            Position = c switch
            {
                '\n' => new(Position.Line + 1, 1),
                '\r' => Position,

                _ => new(Position.Line, Position.Column + 1)
            };

            return c;
        }

        private char Peek(bool read)
        {
            int result = read ? stream.Read() : stream.Peek();

            if (result == -1)
            {
                IsEOF = true;
                return '\0';
            }

            return (char)result;
        }
    }
}
