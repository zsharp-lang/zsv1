using ZSharp.Parser;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    /// <summary>
    /// Where source code isn't Z#, and why.
    /// </summary>
    public sealed class Problem(string message, Span span)
    {
        public string Message { get; } = message;

        public Span Span { get; } = span;
    }

    /// <summary>
    /// What was read of source code: the statements that parsed, and what didn't.
    /// </summary>
    public sealed class Parsed
    {
        public List<AST.Statement> Statements { get; } = [];

        public List<Problem> Problems { get; } = [];
    }

    /// <summary>
    /// Reads source code that may be wrong, as an editor's is: a statement that doesn't parse
    /// is a problem and the next one is read, where a document stops at the first.
    /// </summary>
    /// <remarks>
    /// Z# code can't catch an exception, so it can't do this itself.
    /// </remarks>
    public static class Recovery
    {
        public static Parsed Parse(string source, string? path = null)
        {
            var result = new Parsed();
            Parser.Parser? parser = null;

            try
            {
                parser = new Parser.Parser(Tokenizer.Tokenizer.Tokenize(new(new StringReader(source)))) { Source = path };
                var syntax = Grammar.Install(parser);

                while (parser.HasTokens && !parser.Is(TokenType.EOF))
                {
                    var start = parser.Start;

                    try
                    {
                        result.Statements.Add(syntax.Document.ParseStatement(parser));
                    }
                    catch (ParseError error)
                    {
                        var span = error is UnexpectedTokenError unexpected
                            ? unexpected.GotToken.Span
                            : new Span(start, start);
                        result.Problems.Add(new(error.Message, span));

                        Skip(parser);
                    }
                }
            }
            catch (Exception error) when (error is not OutOfMemoryException)
            {
                // The tokenizer failed, as it does on a string that never ends.
                var where = parser is { HasTokens: true } ? parser.Start : new Position(1, 1);
                result.Problems.Add(new(error.Message, new Span(where, where)));
            }

            return result;
        }

        /// <summary>
        /// Skips what is left of a statement that didn't parse: up to its `;`, or the `}` that
        /// closes what it opened, so the statement after it can be read.
        /// </summary>
        private static void Skip(Parser.Parser parser)
        {
            var depth = 0;

            while (parser.HasTokens && !parser.Is(TokenType.EOF))
            {
                var token = parser.TokenStream.Advance();

                if (token.Is(TokenType.LCurly))
                    depth++;
                else if (token.Is(TokenType.RCurly))
                {
                    depth--;
                    if (depth <= 0) return;
                }
                else if (token.Is(TokenType.Semicolon) && depth <= 0)
                    return;
            }
        }
    }
}
