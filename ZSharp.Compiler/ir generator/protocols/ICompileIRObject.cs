namespace ZSharp.Compiler
{
    public interface ICompileIRObject
    {
        public ZSharp.IR.IRDefinition CompileIRObject(Compiler compiler);
    }

    public interface ICompileIRObject<in Owner> : ICompileIRObject
        where Owner : class
    {
        public ZSharp.IR.IRDefinition CompileIRObject(Compiler compiler, Owner? owner);

        ZSharp.IR.IRDefinition ICompileIRObject.CompileIRObject(Compiler compiler)
            => CompileIRObject(compiler, null);
    }

    public interface ICompileIRObject<T, in Owner> : ICompileIRObject<Owner>
        where T : ZSharp.IR.IRDefinition
        where Owner : class
    {
        public new T CompileIRObject(Compiler compiler, Owner? owner);

        ZSharp.IR.IRDefinition ICompileIRObject<Owner>.CompileIRObject(Compiler compiler, Owner? owner)
            => CompileIRObject(compiler, owner);
    }
}
