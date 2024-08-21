using ZSharp.Text;

namespace ZSharp.Parser
{
    public sealed class TokenStream
    {
        public sealed class TokenStreamPosition(TokenStream tokenStream) : IDisposable
        {
            private readonly Queue<Token>? cache = tokenStream.cache;
            private readonly TokenStream tokenStream = tokenStream;
            private readonly bool cacheEnabled = tokenStream.cacheEnabled;

            private bool alive = true;

            public bool Committing { get; private set; } = false;

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
                Queue<Token>? temp = cache;

                if (!Committing)
                    foreach (var token in tokenStream.cache ?? [])
                        (temp ??= []).Enqueue(token);
                tokenStream.cache = temp;
                tokenStream.cacheEnabled = cacheEnabled;
            }
        }

        private readonly IEnumerator<Token> tokens;

        private Queue<Token>? cache = null;
        private bool cacheEnabled = false;

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
            cacheEnabled = true;
            cache = [];
            return position;
        }

        public Token PeekToken()
            => (cache?.Count ?? 0) > 0 ? cache!.Peek() : tokens.Current;

        public Token Advance()
        {
            if ((cache?.Count ?? 0) > 0)
                return cache!.Dequeue();

            Token result = tokens.Current;

            if (cacheEnabled)
                cache?.Enqueue(result);

            if (SkipWhitespaces)
                while ((HasTokens = tokens.MoveNext()) && tokens.Current.Is(TokenCategory.WhiteSpace)) ;
            else
                HasTokens = tokens.MoveNext();

            return result;
        }
    }
}
