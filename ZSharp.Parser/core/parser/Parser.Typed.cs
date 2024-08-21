using CommonZ.Utils;
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

        public T Parse<T>() where T : Node
            => (parsersFor[typeof(T)] as Parser<T>)!.Parse(this);
    }
}
