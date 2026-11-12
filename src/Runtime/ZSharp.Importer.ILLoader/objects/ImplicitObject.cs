using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// The object a method or a constructor of a class defined in Z# runs on, as its
    /// code reaches it implicitly, through its <c>this</c> node, which is how the code
    /// reaches its class's members by their names. A member's name is resolved as C#
    /// resolves a simple name: the object's member, or else the class's, as a static
    /// member has no object. As a value, it is the object.
    /// </summary>
    internal sealed class ImplicitObject(Variable receiver, DefinedClass @class)
        : CompilerObject
        , ICompileIRExpression
        , ICTGetMember<MemberName>
        , ITyped
    {
        CompilerObject ITyped.Type => @class;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => receiver.CompileIRExpression(target);

        public IResult Member(MemberName member)
            => Scope.CG.Member(receiver, member) is var found && found.IsUnsupported()
                ? Scope.CG.Member(@class, member)
                : found;
    }
}
