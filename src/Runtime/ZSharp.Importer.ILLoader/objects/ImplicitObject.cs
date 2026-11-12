using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// The object a method or a constructor of a class defined in Z# runs on, as its
    /// code reaches it implicitly, through its <c>this</c> node, which is how the code
    /// reaches its class's members by their names. A member's name is resolved as C#
    /// resolves a simple name: the object's member, or else the class's, as a static
    /// member has no object. As a value, it is the object, which the receiver gives: the
    /// method's first parameter, or the cell it is in once a function captures it.
    /// </summary>
    internal sealed class ImplicitObject(CompilerObject receiver, DefinedClass @class)
        : CompilerObject
        , ICompileIRExpression
        , ICompileIRReference<LIR.Expression>
        , ICTGetMember<MemberName>
        , ITyped
    {
        public CompilerObject Receiver { get; } = receiver;

        CompilerObject ITyped.Type => @class;

        /// <summary>
        /// The same object, given by another receiver, as a cell it is copied into.
        /// </summary>
        public ImplicitObject With(CompilerObject given)
            => new(given, @class);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Scope.IR.CompileExpression(Receiver, target);

        public IResult<LIR.Expression, Error> CompileIRReference(object? target)
            => Scope.IR.CompileReference<LIR.Expression>(Receiver, target);

        public IResult Member(MemberName member)
            => Scope.CG.Member(Receiver, member) is var found && found.IsUnsupported()
                ? Scope.CG.Member(@class, member)
                : found;
    }
}
