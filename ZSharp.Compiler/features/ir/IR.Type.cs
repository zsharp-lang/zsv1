using IRTypeResult = ZSharp.Compiler.Result<ZSharp.IR.IType, string>;

namespace ZSharp.Compiler
{
    public partial struct IR
    {
        public IRTypeResult CompileType(CompilerObject @object)
        {
            IRType? result = null;

            if (@object is ICompileIRType irType)
                result = irType.CompileIRType(compiler);

            if (result is not null)
                return IRTypeResult.Ok(result);
            return IRTypeResult.Error(
                "Object cannot be compiled to IR type"
            );
        }

        public Result<T, Error> CompileType<T>(CompilerObject @object)
            where T : class, IRType
        {
            T? result = null;

            if (@object is ICompileIRType<T> irType)
                result = irType.CompileIRType(compiler);

            if (result is not null)
                return Result<T, Error>.Ok(result);
            return Result<T, Error>.Error(
                "Object cannot be compiled to IR type"
            );
        }
    }
}
