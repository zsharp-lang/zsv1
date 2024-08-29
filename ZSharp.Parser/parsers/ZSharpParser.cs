﻿using ZSharp.AST;

namespace ZSharp.Parser
{
    public sealed class ZSharpParser
    {
        public DocumentParser Document { get; } = new();

        public ModuleParser Module { get; } = new();

        public ExpressionParser Expression { get; } = new();

        public Document Parse(Parser parser)
            => Document.Parse(parser);

        public void RegisterParsers(Parser parser)
        {
            parser.AddParserFor(Expression);
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
        }

        private void InitializeModuleLevelParser<T>(ContextParser<T, Statement> parser)
            where T : Node
        {
            parser.AddKeywordParser(
                LangParser.Keywords.Function, 
                Utils.DefinitionStatement(LangParser.ParseFunctionExpression)
            );
            parser.AddKeywordParser(
                LangParser.Keywords.Module, 
                Utils.DefinitionStatement(Module)
            );

            parser.AddKeywordParser(
                LangParser.Keywords.Import, 
                LangParser.ParseImportStatement
            );
            //parser.AddKeywordParser(
            //    LangParser.Keywords.Let, 
            //    LangParser.ParseLetStatement
            //);
        }

        private void InitializeExpressionParser()
        {
            
        }
    }
}
