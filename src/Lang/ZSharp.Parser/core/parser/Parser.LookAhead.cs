namespace ZSharp.Parser
{
    public sealed partial class Parser
    {
        public TokenStream.TokenStreamPosition LookAhead(bool commit = false) 
            => TokenStream.LookAhead(commit);
    }
}
