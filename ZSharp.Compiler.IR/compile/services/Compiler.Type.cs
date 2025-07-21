namespace ZSharp.IRCompiler
{
    partial class Compiler
    {
        public ZSharp.Compiler.Result<IR.IType, Error> CompileType(CompilerObject @object)
        {
            throw new NotImplementedException();
        }

        public ZSharp.Compiler.Result<T, Error> CompileType<T>(CompilerObject @object)
            where T : class, IR.IType
        {
            throw new NotImplementedException();
        }
    }
}
