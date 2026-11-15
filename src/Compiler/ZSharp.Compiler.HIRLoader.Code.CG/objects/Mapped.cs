using System.Diagnostics.CodeAnalysis;
using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Compiler.Objects
{
    /// <summary>
    /// A statement that says where it was written: it lowers as the statement it stands for
    /// does, and gives what it lowers to its map. Anything else asked of it is the
    /// statement's.
    /// </summary>
    internal sealed class Mapped(CompilerObject statement, SourceMapping.SourceMap map)
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

            if (result.Ok(out var compiled) && SourceMapping.SourceMaps.Of(compiled) is null)
                SourceMapping.SourceMaps.Set(compiled, map);

            return result;
        }
    }
}
