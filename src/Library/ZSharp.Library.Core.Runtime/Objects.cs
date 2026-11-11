using CommonZ;
using ZSharp.Compiler;

using CompilerObject = ZSharp.MIR.CompilerObject;
using Scope = Core.Compiler.ModuleScope;

namespace Core.Runtime
{
    /// <summary>
    /// The running process's own conversion: a value of a type it realizes as a CLR class,
    /// as a Z# class's objects, goes where an <c>Object</c> is expected, as every object of
    /// the process is one. It is the runtime's, not the language's: a class has no base,
    /// and code with this conversion compiles only for the runtime.
    /// </summary>
    public static class Objects
    {
        /// <summary>
        /// Makes the implicit casts the code generator does have this one, after what they
        /// had before.
        /// </summary>
        public static void Install(ZSharp.Platform.Runtime.Runtime runtime, ZSharp.Importer.ILLoader.ILLoader loader)
        {
            var previous = Scope.CG.ImplicitCast;

            Scope.CG.ImplicitCast = (value, type) =>
            {
                var result = previous(value, type);

                if (result.IsOk || !IsObject(type, loader) || !IsObjectOfClass(value, runtime))
                    return result;

                return Result<CompilerObject>.Ok(new AsObject(value, type, runtime));
            };
        }

        /// <summary>
        /// Whether a type is <c>Object</c>: one it goes to and that goes to it.
        /// </summary>
        private static bool IsObject(CompilerObject type, ZSharp.Importer.ILLoader.ILLoader loader)
            => Scope.HIRLoader.LoadType(loader.Load(typeof(object))).Ok(out var @object)
                && Scope.TS.IsAssignableTo(type, @object!)
                && Scope.TS.IsAssignableTo(@object!, type);

        /// <summary>
        /// Whether a value is of a type the runtime realizes as a class: a structure it
        /// makes polymorphic, or a class definition that isn't a value type.
        /// </summary>
        private static bool IsObjectOfClass(CompilerObject value, ZSharp.Platform.Runtime.Runtime runtime)
            => Scope.TS.TypeOf(value).Ok(out var type)
                && Scope.IR.CompileType(type!, runtime).Ok(out var lowered)
                && lowered switch
                {
                    ZSharp.LIR.Structure structure => structure.Metadata.Any(mark => mark is ZSharp.LIR.CLR.Polymorphic),
                    _ => false,
                };

        /// <summary>
        /// A value as an <c>Object</c>, which it already is in the running process.
        /// </summary>
        private sealed class AsObject(CompilerObject value, CompilerObject type, ZSharp.Platform.Runtime.Runtime runtime)
            : CompilerObject
            , ICompileIRExpression
            , ITyped
        {
            CompilerObject ITyped.Type => type;

            public IResult<ZSharp.LIR.Expression, Error> CompileIRExpression(object? target)
                => target == runtime
                    ? Scope.IR.CompileExpression(value, target)
                    : Result<ZSharp.LIR.Expression>.Error("A Z# object is an Object only in the running process.");
        }
    }
}
