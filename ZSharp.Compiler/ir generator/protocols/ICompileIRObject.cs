namespace ZSharp.Compiler
{
    public interface ICompileIRObject
    {
        public IR.IRObject CompileIRObject(Compiler compiler);
    }

    public interface ICompileIRObject<in Owner> : ICompileIRObject
        where Owner : class
    {
        public IR.IRObject CompileIRObject(Compiler compiler, Owner? owner);

        IR.IRObject ICompileIRObject.CompileIRObject(Compiler compiler)
            => CompileIRObject(compiler, null);
    }

    public interface ICompileIRObject<T, in Owner> : ICompileIRObject<Owner>
        where T : IR.IRObject
        where Owner : class
    {
        public new T CompileIRObject(Compiler compiler, Owner? owner);

        IR.IRObject ICompileIRObject<Owner>.CompileIRObject(Compiler compiler, Owner? owner)
            => CompileIRObject(compiler, owner);
    }
}
