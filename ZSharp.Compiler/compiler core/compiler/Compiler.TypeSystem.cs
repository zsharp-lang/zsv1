namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public TypeSystem TypeSystem { get; }

        private void InitializeTypeSystem()
        {
            Context.GlobalScope.Cache("string", TypeSystem.String);
            Context.GlobalScope.Cache("type", TypeSystem.Type);
            Context.GlobalScope.Cache("void", TypeSystem.Void);
        }
    }
}
