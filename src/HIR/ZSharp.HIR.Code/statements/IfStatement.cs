namespace ZSharp.HIR.Code
{
    public sealed class IfStatement
        : Statement
    {
        public required Expression Condition { get; set; }

        public required Statement Then { get; set; }

        public Statement? Else { get; set; }
    }
}
