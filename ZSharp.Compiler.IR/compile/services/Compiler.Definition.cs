namespace ZSharp.IRCompiler
{
    partial class Compiler
    {
        public ZSharp.Compiler.Result<T, Error> Definition<T>(CompilerObject @object, TargetPlatform? target)
            where T : IR.IRObject
        {
            throw new NotImplementedException();
        }

        public void Definition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
            where Owner : IR.IRObject
        {
            throw new NotImplementedException();
        }
    }
}
