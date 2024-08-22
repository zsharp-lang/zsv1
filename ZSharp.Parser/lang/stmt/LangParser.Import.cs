using ZSharp.AST;

namespace ZSharp.Parser
{
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

            List<ImportedName>? importedNames = null;
            Expression source;

            if (parser.Is(Text.TokenType.LCurly, eat: true))
            {
                importedNames = [];

                while (!parser.Is(Text.TokenType.RCurly, eat: true))
                {
                    importedNames.Add(ParseImportedName(parser));

                    while (!parser.Is(Text.TokenType.RCurly) && parser.Is(Text.TokenType.Comma, eat: true))
                        importedNames.Add(ParseImportedName(parser));
                }

                parser.Eat(Keywords.From);
            } else if (parser.Is(Symbols.ImportAll))
            {
                throw new NotImplementedException("'import *' is not implemented yet");
            }

            source = parser.Parse<Expression>();

            string? alias = null;

            if (parser.Is(Keywords.As, eat: true))
                alias = parser.Eat(Text.TokenType.Identifier).Value;

            var semicolon = parser.Eat(Text.TokenType.Semicolon);

            return new(new()
            {
                ImportKeyword = importKeyword,
                Semicolon = semicolon
            })
            {
                Alias = alias,
                ImportedNames = importedNames,
                Source = source,
            };
        }
    }
}
