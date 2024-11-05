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

            Context.GlobalScope.Cache("i32", TypeSystem.Int32);

            Context.GlobalScope.Cache("f32", TypeSystem.Float32);

            foreach (var type in new CGObject[] {
                    TypeSystem.String,
                    TypeSystem.Type,
                    TypeSystem.Void,
                    TypeSystem.Int32,
                    TypeSystem.Float32
                }
            )
                if (CompileIRType(type) is IR.IRObject irObject)
                    ObjectCache.IR(type, irObject);
        }
    }
}
