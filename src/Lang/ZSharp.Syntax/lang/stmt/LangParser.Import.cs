using ZSharp.Parser;
using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Syntax
{
    using Parser = global::ZSharp.Parser.Parser;

    public static partial class LangParser
    {
        public static ImportedName ParseImportedName(Parser parser)
        {
            ImportedName importedName = new()
            {
                Name = parser.Eat(Text.TokenType.Identifier).Value
            };

            if (parser.Is(Keywords.As, eat: true))
                importedName.Alias = parser.Eat(Text.TokenType.Identifier).Value;

            return importedName;
        }

        public static ImportStatement ParseImportStatement(Parser parser)
        {
            var importKeyword = parser.Eat(Keywords.Import);

            List<CallArgument>? arguments = null;
            if (parser.Is(TokenType.LParen, eat: true))
            {
                arguments = [];
                if (!parser.Is(TokenType.RParen))
                {
                    arguments.Add(ParseCallArgument(parser));

                    while (parser.Is(TokenType.Comma, eat: true))
                        arguments.Add(ParseCallArgument(parser));
                }

                parser.Eat(TokenType.RParen);
            }


            List<ImportedName>? importedNames = null;
            Expression source;

            if (parser.Is(TokenType.LCurly, eat: true))
            {
                importedNames = [];

                if (!parser.Is(TokenType.RCurly))
                {
                    importedNames.Add(ParseImportedName(parser));

                    while (parser.Is(TokenType.Comma, eat: true))
                        importedNames.Add(ParseImportedName(parser));
                }

                parser.Eat(TokenType.RCurly);

                parser.Eat(Keywords.From);
            } else if (parser.Is(Symbols.ImportAll))
            {
                throw new NotImplementedException("'import *' is not implemented yet");
            }

            source = parser.Parse<Expression>();

            var semicolon = parser.Eat(TokenType.Semicolon);

            return new(new()
            {
                ImportKeyword = importKeyword,
                Semicolon = semicolon
            })
            {
                Arguments = arguments,
                ImportedNames = importedNames,
                Source = source,
            };
        }
    }
}
