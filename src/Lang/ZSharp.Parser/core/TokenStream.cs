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

                var popped = tokenStream.lookAheads.Pop();
                System.Diagnostics.Debug.Assert(ReferenceEquals(popped, this));

                if (Committing)
                {
                    // The tokens were read under the enclosing look-ahead too, so it can
                    // still give them back.
                    if (tokenStream.CurrentLookAhead is { } outer)
                        foreach (var token in cache)
                            outer.cache.Enqueue(token);

                    return;
                }

                // The tokens go back before any the stream already holds, since they were
                // read before those.
                var remaining = tokenStream.cache.ToArray();

                tokenStream.cache.Clear();

                foreach (var token in cache.Concat(remaining))
                    tokenStream.cache.Enqueue(token);
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
