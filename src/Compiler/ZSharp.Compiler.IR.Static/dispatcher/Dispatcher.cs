namespace ZSharp.Compiler.IRDispatchers.Static
{
    public sealed class Dispatcher(IR ir)
        : IIRDefinitionAsCompiler
        , IIRDefinitionInCompiler
        , IIRReferenceCompiler
    {
        private readonly IR ir = ir;

        private IR @base = null!;

        public void Apply()
        {
            @base = ir.Clone();

            ir.CompileExpression = CompileExpression;
            ir.CompileStatement = CompileStatement;
            ir.CompileType = CompileType;
            ir.DefinitionAsCompiler = this;
            ir.DefinitionInCompiler = this;
            ir.ReferenceCompiler = this;
        }

        public IResult<LIR.Expression, Error> CompileExpression(CompilerObject @object, object? target)
        {
            var result = @base.CompileExpression(@object, target);

            if (result.IsUnsupported() && @object.Is<ICompileIRExpression>(out var compile))
                result = compile.CompileIRExpression(target);

            return result;
        }

        public IResult<LIR.Statement, Error> CompileStatement(CompilerObject @object, object? target)
        {
            var result = @base.CompileStatement(@object, target);

            if (result.IsUnsupported() && @object.Is<ICompileIRStatement>(out var compile))
                result = compile.CompileIRStatement(target);

            // An expression can stand where a statement is expected.
            if (result.IsUnsupported() && @object.Is<ICompileIRExpression>(out var expression))
                result = expression.CompileIRExpression(target).When<LIR.Statement>(value => new LIR.Evaluate(value));

            return result;
        }

        public IResult<LIR.Type, Error> CompileType(CompilerObject @object, object? target)
        {
            var result = @base.CompileType(@object, target);

            if (result.IsUnsupported() && @object.Is<ICompileIRType>(out var compile))
                result = compile.CompileIRType(target);

            return result;
        }

        IResult<T, Error> IIRDefinitionAsCompiler.CompileDefinition<T>(CompilerObject @object, object? target)
        {
            var result = @base.CompileDefinition<T>(@object, target);

            if (result.IsUnsupported() && @object.Is<ICompileIRDefinitionAs<T>>(out var compile))
                result = compile.CompileIRDefinition(target);

            return result;
        }

        Error? IIRDefinitionInCompiler.CompileDefinition<Owner>(CompilerObject @object, Owner owner, object? target)
        {
            var error = @base.CompileDefinition(@object, owner, target);

            if (error.IsUnsupported() && @object.Is<ICompileIRDefinitionIn<Owner>>(out var compile))
                error = compile.CompileIRDefinition(owner, target);

            return error;
        }

        IResult<T, Error> IIRReferenceCompiler.CompileReference<T>(CompilerObject @object, object? target)
        {
            var result = @base.CompileReference<T>(@object, target);

            if (result.IsUnsupported() && @object.Is<ICompileIRReference<T>>(out var compile))
                result = compile.CompileIRReference(target);

            return result;
        }
    }
}
