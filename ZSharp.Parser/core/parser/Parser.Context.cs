using CommonZ.Utils;
using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed partial class Parser
    {
        private readonly Mapping<Type, ParserBase> contextParsers = [];

        public void AddContextParser<T>(Parser<T> contextParser)
            where T : Node
        {
            contextParsers[typeof(T)] = contextParser;
        }

        public T Parse<T>() where T : Node
            => (contextParsers[typeof(T)] as Parser<T>)!.Parse(this);
    }
}
