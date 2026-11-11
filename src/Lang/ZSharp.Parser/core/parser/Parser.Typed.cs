using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed partial class Parser
    {
        public void AddParserFor<T>(ParserFunction<T> parser)
            where T : Node
            => AddParserFor(new FunctionalParser<T>(parser));

        public void AddParserFor<T>(Parser<T> contextParser)
            where T : Node
            => contextParsers[typeof(T)] = contextParser;

        public Parser<T>? GetParserFor<T>()
            where T : Node
            => contextParsers.TryGetValue(typeof(T), out var parser) ? parser as Parser<T> : null;

        public bool GetParserFor<T>([NotNullWhen(true)] out Parser<T>? parser)
            where T : Node
            => (parser = GetParserFor<T>()) is not null;

        public T Parse<T>() where T : Node
        {
            if (contextParsers.TryGetValue(typeof(T), out var parser))
                return ((Parser<T>)parser).Parse(this);

            throw new ParseError();
        }
    }
}
