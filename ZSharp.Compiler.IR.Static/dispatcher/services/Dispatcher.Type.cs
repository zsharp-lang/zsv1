namespace ZSharp.Compiler.IRDispatchers.Static
{
    partial class Dispatcher
        : IIRTypeCompiler
    {
        public IResult<ZSharp.IR.IType, Error> CompileType(CompilerObject @object)
        {
            var result = @base.CompileType(@object);

            if (result.IsError && @object.Is<ICompileIRType>(out var compile))
                result = compile.CompileIRType(compiler);

            return result;
        }

        IResult<T, Error> IIRTypeCompiler.CompileType<T>(CompilerObject @object)
        {
            var result = @base.CompileTypeAs<T>(@object);

            if (result.IsError && @object.Is<ICompileIRType<T>>(out var compile))
                result = compile.CompileIRType(compiler);

            return result;
        }
    }
}
