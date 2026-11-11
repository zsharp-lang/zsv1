namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A reference to a variable of a type, as a parameter that takes one has it: a
    /// <c>ref</c> or <c>out</c> parameter in C#. A variable of exactly that type goes to it,
    /// passed by reference, so nothing marks the argument where it is passed.
    /// </summary>
    internal sealed class RefType(ILLoader loader, Type element, bool isOut)
        : CompilerObject
        , IILType
        , ICompileIRType
        , ICompileIRExpression
        , ITyped
        , ICTImplicitCastFrom
        , ICTImplicitCastTo
        , IAssignableTo
        , ICTCastTo
    {
        public Type Element => element;

        public bool IsOut => isOut;

        public Type GetILType() => element.MakeByRefType();

        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => Result<LIR.Type>.Ok(loader.Handle(GetILType()));

        /// <summary>
        /// A reference type used as a value is its type object, as a class is.
        /// </summary>
        CompilerObject ITyped.Type => loader.Class(typeof(Type));

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => Result<LIR.Expression>.Ok(new LIR.Call(loader.TypeOf(loader.Handle(GetILType())), []));

        /// <summary>
        /// A reference goes to a reference to the same type, whether it is <c>Ref</c> or
        /// <c>Out</c>, since the CLR has them as one type.
        /// </summary>
        public bool IsAssignableTo(CompilerObject target)
            => target is RefType other && other.Element == element;

        /// <summary>
        /// Casting a reference type to what describes it is reflection, as for a class: a
        /// reference to the type its target describes, so a signature written in Z# can
        /// take one.
        /// </summary>
        public IResult Cast(CompilerObject targetType)
        {
            var node = new Package.DotNet.RefType(loader.Load(element)) { IsOut = isOut };

            return ZSharp.Compiler.Current.TS.IsAssignableTo(targetType, loader.Class(node.GetType()))
                ? Result.Ok(new Literal(node, node.GetType(), loader))
                : Result.Error(new UnsupportedError($"A reference can't be cast to that type."));
        }

        /// <summary>
        /// A reference type goes where an HIR type is expected as what describes it.
        /// </summary>
        IResult ICTImplicitCastTo.ImplicitCast(CompilerObject targetType)
            => loader.Describes(targetType) ? Cast(targetType) : Result.Error(new UnsupportedError("A reference type isn't a description."));

        public IResult ImplicitCast(CompilerObject @object)
        {
            var typed = ZSharp.Compiler.Current.TS.TypeOf(@object).Ok(out var type);

            if (typed && type is RefType other && other.Element == element)
                return Result.Ok(@object);

            if (@object.Is<ICTSet>(out _) && typed && type!.Is<IILType>(out var known) && known.GetILType() == element)
                return Result.Ok(new ByAddress(loader, @object, element));

            return Result.Error(new UnsupportedError($"Only a variable of {element.Name} can be passed as {(isOut ? "Out" : "Ref")}[{element.Name}]."));
        }
    }
}
