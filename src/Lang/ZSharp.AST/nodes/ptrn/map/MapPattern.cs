namespace ZSharp.AST
{
    public sealed partial class MapPattern : Pattern
    {
        public RestPattern? Rest { get; set; }
    }
}
