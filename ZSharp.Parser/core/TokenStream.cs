using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class TokenStream
    {
        public sealed class TokenStreamPosition(TokenStream tokenStream, bool commit = false) : IDisposable
        {
            internal readonly Queue<Token> cache = [];
            private readonly TokenStream tokenStream = tokenStream;

            private bool alive = true;

            public bool Committing { get; private set; } = commit;

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
                if (!alive) return;

                alive = false;

                if (!Committing)
                    foreach (var token in cache)
                        tokenStream.cache.Enqueue(token);

                System.Diagnostics.Debug.Assert(ReferenceEquals(tokenStream.lookAheads.Pop(), this));
            }
        }

        private readonly IEnumerator<Token> tokens;

        private readonly Queue<Token> cache = [];
        private readonly Stack<TokenStreamPosition> lookAheads = [];

        private TokenStreamPosition? CurrentLookAhead => lookAheads.Count > 0 ? lookAheads.Peek() : null;

        public bool HasTokens { get; private set; } = true;

        public bool SkipWhitespaces { get; set; } = true;

        public TokenStream(IEnumerable<Token> tokens)
        {
            this.tokens = tokens.GetEnumerator();

            if (SkipWhitespaces)
                while ((HasTokens = this.tokens.MoveNext()) && this.tokens.Current.Is(TokenCategory.WhiteSpace)) ;
        }

        public TokenStreamPosition LookAhead(bool commit)
        {
            lookAheads.Push(new(this, commit));
            return lookAheads.Peek();
        }

        public Token PeekToken()
            => cache.Count > 0 ? cache.Peek() : tokens.Current;

        public Token Advance()
        {
            Token result;

            if (cache.Count > 0)
            {
                result = cache.Dequeue();
                CurrentLookAhead?.cache.Enqueue(result);

                return result;
            }
            else result = tokens.Current;

            CurrentLookAhead?.cache.Enqueue(result);

            if (SkipWhitespaces)
                while ((HasTokens = tokens.MoveNext()) && tokens.Current.Is(TokenCategory.WhiteSpace)) ;
            else
                HasTokens = tokens.MoveNext();

            return result;
        }
    }
}
