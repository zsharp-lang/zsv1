namespace ZSharp.Compiler.IRDispatchers.Static
{
    partial class Dispatcher
        : IIRTypeCompiler
    {
        public IResult<ZSharp.IR.IType, Error> CompileType(CompilerObject @object, object? target)
        {
            var result = @base.CompileType(@object, target);

            if (result.IsError && @object.Is<ICompileIRType>(out var compile))
                result = compile.CompileIRType(compiler, target);

            return result;
        }

        IResult<T, Error> IIRTypeCompiler.CompileType<T>(CompilerObject @object, object? target)
        {
            var result = @base.CompileTypeAs<T>(@object, target);

            if (result.IsError && @object.Is<ICompileIRType<T>>(out var compile))
                result = compile.CompileIRType(compiler, target);

            return result;
        }
    }
}
