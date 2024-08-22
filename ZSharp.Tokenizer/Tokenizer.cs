using System.Runtime.CompilerServices;
using System.Text;
using ZSharp.Text;

namespace ZSharp.Tokenizer
{
    public static class Tokenizer
    {
        private const char StringQuotation = '\"';
        private const char CharQuotation = '\'';

        private const char BackSlash = '\\';
        private const char Slash = '/';

        private const char Asterisk = '*';

        private const char Underscore = '_';

        private const char LF = '\n';
        private const char CR  = '\r';

        private const string Operators = "./|+-=<>!@#$%^&*~?";

        private static bool IsAlpha(char c)
            => ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z');

        private static bool IsDigit(char c)
            => '0' <= c && c <= '9';

        private static bool IsHexDigit(char c)
            => IsDigit(c) || ('a' <= c && c <= 'f') || ('a' <= c && c <= 'f');

        private static bool IsAlphaNumeric(char c)
            => IsAlpha(c) || IsDigit(c);

        public static IEnumerable<Token> Tokenize(TextStream stream)
        {
            while (!stream.IsEOF)
            {
                Position start = stream.Position;

                Token SingleChar(TokenType type)
                    => new(type, stream.Read().ToString(), new(start, stream.Position));

                Token ReadString()
                {
                    char c = stream.Read();

                    StringBuilder sb = new();

                    while ((c = stream.Read()) != StringQuotation)
                    {
                        if (c == BackSlash)
                        {
                            var replace = stream.Peek() switch
                            {
                                'n' => LF,
                                'r' => CR,
                                StringQuotation => StringQuotation,
                                CharQuotation => CharQuotation,
                                BackSlash => stream.Read(),
                                _ => c
                            };
                            if (c != replace)
                                stream.Read();
                            c = replace;
                        }

                        sb.Append(c);
                    }

                    return new(TokenType.String, sb.ToString(), new(start, stream.Position));
                }

                Token ReadChar()
                {
                    char c = stream.Read(); // assert == CharQuotation

                    if ((c = stream.Read()) == BackSlash)
                    {
                        var replace = stream.Peek() switch
                        {
                            'n' => LF,
                            'r' => CR,
                            StringQuotation => StringQuotation,
                            CharQuotation => CharQuotation,
                            BackSlash => stream.Read(),
                            _ => c
                        };
                        if (c != replace)
                            stream.Read();
                        c = replace;
                    }

                    stream.Read(); // assert == CharQuotation

                    return new(TokenType.Char, c.ToString(), new(start, stream.Position));
                }

                Token ReadNumber()
                {
                    throw new NotImplementedException();
                }

                Token ReadIdentifier()
                {
                    char c = stream.Peek();

                    StringBuilder sb = new();

                    if (!IsAlpha(c) && c != Underscore) throw new NotImplementedException();
                    sb.Append(stream.Read());

                    while (IsAlphaNumeric(c = stream.Peek()) || c == Underscore)
                        sb.Append(stream.Read());

                    return new(TokenType.Identifier, sb.ToString(), new(start, stream.Position));
                }

                Token ReadToken()
                {
                    char c = stream.Peek();

                    StringBuilder sb = new();

                    if (c == Slash)
                    {
                        stream.Read();

                        if (stream.Peek() == Slash)
                            return new(TokenType.LineComment, stream.ReadLine(), new(start, stream.Position));
                        if (stream.Peek() == Asterisk)
                        {
                            stream.Read();

                            while (!stream.IsEOF)
                            {
                                while (stream.Peek() != Asterisk) stream.Read();
                                stream.Read();
                                if (stream.Peek() == Slash) break;
                            }

                            stream.Read();
                            return new(TokenType.BlockComment, string.Empty, new(start, stream.Position));
                        }

                        sb.Append(c);

                        while (Operators.Contains(stream.Peek()))
                            sb.Append(stream.Read());

                        return new(TokenType.Operator, sb.ToString(), new(start, stream.Position));
                    }

                    if (Operators.Contains(c))
                    {
                        while (Operators.Contains(stream.Peek()))
                            sb.Append(stream.Read());

                        return new(TokenType.Operator, sb.ToString(), new(start, stream.Position));
                    }

                    if (IsDigit(c)) return ReadNumber();

                    if (IsAlpha(c) || c == Underscore) return ReadIdentifier();

                    throw new Exception("Invalid character: " + c);
                }

                Token EOF()
                    => new(TokenType.EOF, string.Empty, new(start, stream.Position));

                char c = stream.Peek();

                Token token = c switch
                {
                    '\0' => EOF(),
                    '$' => SingleChar(TokenType.Breakpoint),
                    LF => SingleChar(TokenType.NewLine),
                    CR => SingleChar(TokenType.NewLine),
                    ' ' => SingleChar(TokenType.Space),
                    '\t' => SingleChar(TokenType.Tab),
                    '{' => SingleChar(TokenType.LCurly),
                    '}' => SingleChar(TokenType.RCurly),
                    '(' => SingleChar(TokenType.LParen),
                    ')' => SingleChar(TokenType.RParen),
                    '[' => SingleChar(TokenType.LBracket),
                    ']' => SingleChar(TokenType.RBracket),
                    ';' => SingleChar(TokenType.Semicolon),
                    ':' => SingleChar(TokenType.Colon),
                    ',' => SingleChar(TokenType.Comma),
                    StringQuotation => ReadString(),
                    CharQuotation => ReadChar(),
                    '0' =>  ReadNumber(),
                    _ => ReadToken()
                };

                yield return token;
            }
        }
    }
}
