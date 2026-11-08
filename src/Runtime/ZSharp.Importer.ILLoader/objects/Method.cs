using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A CLR method or constructor, optionally bound to the object it is called on. Methods
    /// that share a name are an <see cref="OverloadGroup"/> of these.
    /// </summary>
    internal sealed class Method(ILLoader loader, MethodBase method, CompilerObject? receiver)
        : CompilerObject
        , ICTCallable
        , ICompileIRReference<LIR.ICallable>
        , ISpecific
    {
        public IResult Call(Argument[] arguments)
        {
            if (Arguments.Match(Matching(), arguments) is not { } values)
                return Result.Error($"{method.Name} can't be called with these arguments.");

            return Result.Ok(new Call(
                loader.Handle(method),
                [.. (receiver is null ? [] : new[] { receiver }), .. values],
                ILLoader.ReturnType(method) is { } type ? () => loader.Class(type) : null
            ));
        }

        public IResult<LIR.ICallable, Error> CompileIRReference(object? target)
            => Result<LIR.ICallable>.Ok(loader.Handle(method));

        public bool IsAsSpecificAs(CompilerObject other)
            => ZSharp.Compiler.Parameters.AreAsSpecificAs(Parameters(), other);

        private CompilerObject[] Parameters()
            => [.. method.GetParameters().Select(parameter => loader.Class(parameter.ParameterType))];

        /// <summary>
        /// The parameters as they match arguments. A method defined from HIR has them as
        /// written, so its default values are code. Any other has those of its metadata.
        /// </summary>
        private CompilerObject[] Matching()
            => loader.DefinedParameters(method) is { } defined && Arguments.Parameters(defined) is { } written
                ? written
                : [.. method.GetParameters().Select(parameter => new ClrParameter(parameter, loader))];
    }
}
