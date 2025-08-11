namespace ZSharp.IRCompiler
{
    partial class Compiler
    {
        public ZSharp.Compiler.Result<T, Error> CompileDefinition<T>(CompilerObject @object, TargetPlatform? target)
            where T : IR.IRDefinition
        {
            throw new NotImplementedException();
        }

        public void CompileDefinition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
            where Owner : IR.IRDefinition
        {
            throw new NotImplementedException();
        }
    }
}
