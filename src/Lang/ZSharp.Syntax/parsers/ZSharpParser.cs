using ZSharp.Parser;
using ZSharp.AST;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

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
            parser.AddParserFor(ConstructorBody.Content, Class.Constructor);

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
            InitializeDefinitionBodyParser(Class);

            // A module is defined in a document.
            Document.AddKeywordParser(
                LangParser.Keywords.Module,
                Utils.DefinitionStatement(Module)
            );

            InitializeStatementParser();

            InitializeLoopParser();
        }

        private void InitializeModuleLevelParser<T>(ContextParser<T, Statement> parser)
            where T : Node
        {
            parser.AddKeywordParser(
                LangParser.Keywords.Def,
                Utils.DefinitionStatement(Class)
            );
            parser.AddKeywordParser(
                LangParser.Keywords.Function, 
                Utils.DefinitionStatement(Function)
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

        /// <summary>
        /// A definition's body holds definitions as a module's does: what each is, and
        /// whether the definition takes it, is its meta-type's to say.
        /// </summary>
        private void InitializeDefinitionBodyParser<T>(ContextParser<T, Statement> parser)
            where T : Node
        {
            parser.AddKeywordParser(
                LangParser.Keywords.Def,
                Utils.DefinitionStatement(Class)
            );
            parser.AddKeywordParser(
                LangParser.Keywords.Import,
                LangParser.ParseImportStatement
            );
        }

        private void InitializeStatementParser()
        {
            
        }

        private void InitializeLoopParser()
        {
            // A loop's body has its own statements, break and continue, and what isn't one
            // is for what the loop is in to parse, as return is a function's: what isn't a
            // loop's statement fails here, and the stack goes on to the parser below.
            Loop.DefaultContextItemParser = new FunctionalParser<AST.Statement>(_ => throw new ParseError());

            Loop.AddKeywordParser(
                LangParser.Keywords.Break,
                LangParser.ParseBreakStatement
            );
            Loop.AddKeywordParser(
                LangParser.Keywords.Continue,
                LangParser.ParseContinueStatement
            );
        }
    }
}
