using IRDefinitionResult = ZSharp.Compiler.Result<ZSharp.IR.IRObject, string>;

namespace ZSharp.Compiler
{
    public partial struct IR
    {
        public IRDefinitionResult CompileDefinition(CompilerObject @object)
        {
            ZSharp.IR.IRObject? result = null;

            if (@object is ICompileIRObject irObject)
                result = irObject.CompileIRObject(compiler);

            if (result is not null)
                return IRDefinitionResult.Ok(result);
            return IRDefinitionResult.Error(
                "Object cannot be compiled to IR definition"
            );
        }

        public IRDefinitionResult CompileDefinition<Owner>(CompilerObject @object, Owner? owner)
            where Owner : ZSharp.IR.IRObject
        {
            ZSharp.IR.IRObject? result = null;

            if (@object is ICompileIRObject<Owner> irObject)
                result = irObject.CompileIRObject(compiler, owner);

            if (result is not null)
                return IRDefinitionResult.Ok(result);
            return IRDefinitionResult.Error(
                "Object cannot be compiled to IR definition"
            );
        }

        public Result<T, Error> CompileDefinition<T, Owner>(CompilerObject @object, Owner? owner)
            where T : ZSharp.IR.IRObject
            where Owner : class
        {
            T? result = null;

            if (@object is ICompileIRObject<T, Owner> irObject)
                result = irObject.CompileIRObject(compiler, owner);

            if (result is not null)
                return Result<T, Error>.Ok(result);
            return Result<T, Error>.Error(
                "Object cannot be compiled to IR definition"
            );
        }
    }
}
