namespace ZSharp.Text
{
    public enum TokenCategory : ushort
    {
        WhiteSpace = 1 << (8 + 0),
        Miscellaneous = 1 << (8 + 1),
        Symbol = 1 << (8 + 2),
        Literal = 1 << (8 + 3),
        Name = 1 << (8 + 4),
    }

    public enum TokenType : ushort
    {
        Mask = 0xFF00,

        Space = TokenCategory.WhiteSpace | 1,
        Tab = TokenCategory.WhiteSpace | 2,
        NewLine = TokenCategory.WhiteSpace | 3,
        LineComment = TokenCategory.WhiteSpace | 4,
        BlockComment = TokenCategory.WhiteSpace | 5,

        EOF = TokenCategory.Miscellaneous | 1,
        Error = TokenCategory.Miscellaneous | 2,
        Breakpoint = TokenCategory.Miscellaneous | 3,
        Documentation = TokenCategory.Miscellaneous | 4,

        Identifier = TokenCategory.Name | 1,
        Operator = TokenCategory.Name | 2,

        LCurly = TokenCategory.Symbol | 1,
        RCurly = TokenCategory.Symbol | 2,
        LParen = TokenCategory.Symbol | 3,
        RParen = TokenCategory.Symbol | 4,
        LBracket = TokenCategory.Symbol | 5,
        RBracket = TokenCategory.Symbol | 6,
        Comma = TokenCategory.Symbol | 7,
        Semicolon = TokenCategory.Symbol | 8,
        Colon = TokenCategory.Symbol | 9,
        Dot = TokenCategory.Symbol | 10,

        String = TokenCategory.Literal | 1,
        Number = TokenCategory.Literal | 2,
        True = TokenCategory.Literal | 3,
        False = TokenCategory.Literal | 4,
        Null = TokenCategory.Literal | 5,
        Unit = TokenCategory.Literal | 6,
        Char = TokenCategory.Literal | 7,
        Hex = TokenCategory.Literal | 8,
        Decimal = TokenCategory.Literal | 9,
        Real = TokenCategory.Literal | 10,
    }
}
