using System.Diagnostics.CodeAnalysis;
using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A statement whose code says where it was written: it lowers as the statement it
    /// stands for does, and what it lowers into says the same of what it lowers to.
    /// Anything else asked of it is the statement's.
    /// </summary>
    internal sealed class Mapped(CompilerObject statement, SourceMapping.SourceMap map, LIR.Definition into)
        : CompilerObject
        , ICompileIRStatement
    {
        bool CompilerObject.Is<T>([NotNullWhen(true)] out T? found) where T : class
            => typeof(T) == typeof(ICompileIRStatement)
                ? (found = this as T) is not null
                : statement.Is(out found);

        public IResult<LIR.Statement, Error> CompileIRStatement(object? target)
        {
            var result = Scope.IR.CompileStatement(statement, target);

            if (result.Ok(out var compiled))
                SourceMapping.FunctionMaps.In(into)[compiled] ??= map;

            return result;
        }
    }
}
