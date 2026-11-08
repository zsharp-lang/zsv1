namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// An extension method reached through a value: calling it passes the value as its
    /// first argument. A generic one infers its type arguments from the value too.
    /// </summary>
    internal sealed class Extension(CompilerObject method, CompilerObject receiver)
        : CompilerObject
        , ICTCallable
        , IInferable
        , ISpecific
    {
        private CompilerObject Method => method;

        private Argument[] With(Argument[] arguments)
            => [new(receiver), .. arguments];

        public IResult Call(Argument[] arguments)
            => Core.Compiler.ModuleScope.CG.Call(method, With(arguments));

        public CompilerObject? Infer(Argument[] arguments)
            => method.Is<IInferable>(out var inferable) && inferable.Infer(With(arguments)) is { } inferred
                ? new Extension(inferred, receiver)
                : null;

        /// <summary>
        /// Extension methods compare as the methods they are, with the value as an argument
        /// like any other.
        /// </summary>
        public bool IsAsSpecificAs(CompilerObject other)
            => other is Extension extension && method.Is<ISpecific>(out var specific) && specific.IsAsSpecificAs(extension.Method);
    }
}
