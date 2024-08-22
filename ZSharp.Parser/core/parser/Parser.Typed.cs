using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed partial class Parser
    {
        private readonly Mapping<Type, ParserBase> parsersFor = [];

        public void AddParserFor<T>(Parser<T> contextParser)
            where T : Node
        {
            parsersFor[typeof(T)] = contextParser;
        }

        public Parser<T>? GetParserFor<T>()
            where T : Node
            => parsersFor.TryGetValue(typeof(T), out var parser) ? parser as Parser<T> : null;

        public bool GetParserFor<T>([NotNullWhen(true)] out Parser<T>? parser)
            where T : Node
            => (parser = GetParserFor<T>()) is not null;

        public T Parse<T>() where T : Node
            => (parsersFor[typeof(T)] as Parser<T>)!.Parse(this);
    }
}
