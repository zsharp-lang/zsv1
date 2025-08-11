using ZSharp.Objects;

namespace ZSharp.Compiler
{
    public sealed partial class IRCompiler
    {
        public Result<IRCode, Error> CompileIRCode(CompilerObject @object)
            => Result<IRCode, Error>.Ok(
                Compiler.CompileIRCode(@object)
            );

        public Result<ZSharp.IR.IRDefinition, Error> CompileIRObject(CompilerObject @object)
            => Result<ZSharp.IR.IRDefinition, Error>.Ok(
                Compiler.CompileIRObject(@object)
            );

        public Result<ZSharp.IR.IRDefinition, Error> CompileIRObject<Owner>(CompilerObject @object, Owner? owner)
            where Owner : ZSharp.IR.IRDefinition
            => Result<ZSharp.IR.IRDefinition, Error>.Ok(
                Compiler.CompileIRObject<Owner>(@object, owner)
            );

        public Result<T, Error> CompileIRObject<T, Owner>(CompilerObject @object, Owner? owner)
            where T : ZSharp.IR.IRDefinition
            where Owner : class
            => Result<T, Error>.Ok(
                Compiler.CompileIRObject<T, Owner>(@object, owner)
            );

        public Result<ZSharp.IR.IType, Error> CompileIRType(CompilerObject @object)
            => Result<ZSharp.IR.IType, Error>.Ok(
                Compiler.CompileIRType(@object)
            );

        public Result<T, Error> CompileIRType<T>(CompilerObject @object)
            where T : class, ZSharp.IR.IType
            => Result<T, Error>.Ok(
                Compiler.CompileIRType<T>(@object)
            );

        public Result<T, Error> CompileIRReference<T>(CompilerObject @object)
            where T : class
            => Result<T, Error>.Ok(
                Compiler.CompileIRReference<T>(@object)
            );
    }
}
