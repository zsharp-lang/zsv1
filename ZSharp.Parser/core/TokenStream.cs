using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class TokenStream
    {
        public sealed class TokenStreamPosition(TokenStream tokenStream) : IDisposable
        {
            private readonly Queue<Token>? cache = tokenStream.cache;
            private readonly TokenStream tokenStream = tokenStream;

            public bool Committing { get; private set; } = true;

            public void Commit() => Committing = true;

            public void Restore() => Committing = false;

            public void RestoreImmediate()
            {
                Restore();
                SetCacheState();
            }

            public void Dispose()
                => SetCacheState();

            private void SetCacheState()
            {
                if (Committing) cache?.Clear();
                tokenStream.cache = cache;
            }
        }

        private readonly IEnumerator<Token> tokens;

        private Queue<Token>? cache = null;

        public bool HasTokens { get; private set; } = true;

        public bool SkipWhitespaces { get; set; } = true;

        public TokenStream(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.GetEnumerator();
            HasTokens = this.tokens.MoveNext();
        }

        public TokenStreamPosition LookAhead()
        {
            var position = new TokenStreamPosition(this);
            cache = [];
            return position;
        }

        public Token PeekToken()
            => tokens.Current;

        public Token Advance()
        {
            Token result = tokens.Current;
            cache?.Enqueue(result);

            if (SkipWhitespaces)
                while ((HasTokens = tokens.MoveNext()) && tokens.Current.Is(TokenCategory.WhiteSpace)) ;
            else
                HasTokens = tokens.MoveNext();

            return result;
        }
    }
}
