using ZSharp.AST;
using ZSharp.Text;

namespace ZSharp.Parser
{
    public static partial class LangParser
    {
        public static WhileExpression<TElse> ParseWhileExpression<TElse>(Parser parser)
            where TElse : Node
        {
            parser.Eat(Keywords.While);

            string name = string.Empty;
            if (parser.Is(TokenType.Identifier))
                name = parser.Eat(TokenType.Identifier).Value;

            Expression condition;

#if !ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(TokenType.LParen);
#endif

            condition = parser.Parse<Expression>();

#if !ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(TokenType.RParen);
#endif

#if ZSHARP_MINIMAL_PARENTHESIS
            parser.Eat(Symbols.ThenDo);
#endif

            Statement @while;
            using (parser.Stack(LoopBody.Content))
                @while = parser.Parse<Statement>();

            TElse? @else = null;
            if (parser.Is(Keywords.Else, eat: true))
                @else = parser.Parse<TElse>();

            return new()
            {
                Body = @while,
                Condition = condition,
                Else = @else,
                Name = name
            };
        }
    }
}
