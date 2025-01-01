using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class ZSharpParser
    {
        public DocumentParser Document { get; } = new();

        public ModuleParser Module { get; } = new();

        public ClassParser Class { get; } = new();

        public FunctionParser Function { get; } = new();

        public ExpressionParser Expression { get; } = new();

        public CodeParser Statement { get; } = new();

        public CodeParser Loop { get; } = new();

        public Document Parse(Parser parser)
            => Document.Parse(parser);

        public void RegisterParsers(Parser parser)
        {
            parser.AddParserFor(Expression);
            parser.AddParserFor(Statement);

            parser.AddParserFor(ClassBody.Content, Class);
            parser.AddParserFor(MethodBody.Content, Class.Method);

            parser.AddParserFor(FunctionBody.Content, Function);

            parser.AddParserFor(LoopBody.Content, Loop);
        }

        public ZSharpParser(bool initialize = true)
        {
            if (initialize)
                Initialize();
        }

        private void Initialize()
        {
            InitializeModuleLevelParser(Document);
            InitializeModuleLevelParser(Module);

            InitializeStatementParser();

            InitializeLoopParser();
        }

        private void InitializeModuleLevelParser<T>(ContextParser<T, Statement> parser)
            where T : Node
        {
            parser.AddKeywordParser(
                LangParser.Keywords.Class,
                Utils.DefinitionStatement(Class)
            );
            parser.AddKeywordParser(
                LangParser.Keywords.Function, 
                Utils.DefinitionStatement(Function)
            );
            parser.AddKeywordParser(
                LangParser.Keywords.Module, 
                Utils.DefinitionStatement(Module)
            );

            parser.AddKeywordParser(
                LangParser.Keywords.Import, 
                LangParser.ParseImportStatement
            );
            parser.AddKeywordParser(
                LangParser.Keywords.Let,
                Utils.ExpressionStatement(LangParser.ParseLetExpression)
            );
            parser.AddKeywordParser(
                LangParser.Keywords.Var,
                Utils.ExpressionStatement(LangParser.ParseVarExpression)
            );
        }

        private void InitializeStatementParser()
        {
            
        }

        private void InitializeLoopParser()
        {
            Loop.DefaultContextItemParser = Statement;

            Loop.AddKeywordParser(
                LangParser.Keywords.Break,
                LangParser.ParseBreakStatement
            );
        }
    }
}
