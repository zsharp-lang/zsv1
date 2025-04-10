﻿using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed partial class Parser
    {
		public bool HasTokens => TokenStream.HasTokens;

		public Token Token => HasTokens ? TokenStream.PeekToken() : throw new ParseError();

		public Token Eat(TokenType type)
			=> Is(type) ? TokenStream.Advance() : throw new ParseError();

		public bool Eat(TokenType type, out Token token)
		{
			if (!Is(type)) throw new ParseError();
			token = TokenStream.Advance();
			return true;
		}

		public Token Eat(string value)
			=> Is(value) ? TokenStream.Advance() : throw new ParseError();

        public bool Eat(string value, out Token token)
        {
            if (!Is(value)) throw new ParseError();
			token = TokenStream.Advance();
            return true;
        }

        public bool Is(TokenType type, bool eat = false)
		{
			if (TokenStream.PeekToken().Type != type) return false;
			if (eat) TokenStream.Advance();
			return true;
		}

		public bool Is(TokenType type, out Token token, bool eat = false)
		{
			token = TokenStream.PeekToken();
			if (TokenStream.PeekToken().Type != type) return false;
			if (eat) TokenStream.Advance();
			return true;
		}

		public bool Is(string value, bool eat = false)
		{
            if (TokenStream.PeekToken().Value != value) return false;
            if (eat) TokenStream.Advance();
            return true;
        }

		public bool Is(string value, out Token token, bool eat = false)
        {
            token = TokenStream.PeekToken();
            if (TokenStream.PeekToken().Value != value) return false;
            if (eat) TokenStream.Advance();
            return true;
        }
    }
}
