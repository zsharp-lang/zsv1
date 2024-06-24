namespace ZSharp.RAST
{
    public class RBlock : RStatement
    {
        public bool IsScoped { get; set; } = true;

        public List<RStatement> Statements { get; }

        public RBlock(List<RStatement> statements)
        {
            Statements = statements;
        }
    }
}
