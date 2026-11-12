using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A generic CLR method given types that aren't CLR types yet, as a class being loaded
    /// with the code that uses it. It takes and gives the types it is given in place of
    /// its type parameters, and lowers to an LIR instance of the method, which the
    /// platform makes as it emits the code.
    /// </summary>
    internal sealed class MethodInstantiation(ILLoader loader, MethodInfo definition, CompilerObject[] arguments, CompilerObject? receiver)
        : CompilerObject
        , ICTCallable
    {
        public CompilerObject TypeOf(Type type)
            => loader.Substitute(type, parameter => parameter.DeclaringMethod is not null ? arguments[parameter.GenericParameterPosition] : null);

        private static LIR.Type Lower(CompilerObject type, object? target)
            => ZSharp.Compiler.Current.IR.CompileType(type, target).Unwrap();

        /// <summary>
        /// The LIR type of a parameter: a reference to a variable of its type, if it takes
        /// one, as the target lowers its type.
        /// </summary>
        public LIR.Type ParameterType(Type type, object? target)
            => type.IsByRef ? new LIR.CLR.ByRefType(Lower(TypeOf(type.GetElementType()!), target)) : Lower(TypeOf(type), target);

        /// <summary>
        /// The instance of the method, as a target lowers its type arguments.
        /// </summary>
        private IResult<LIR.ICallable, Error> Lowered(object? target)
        {
            var signature = new LIR.Signature();

            if (!definition.IsStatic)
                signature.Parameters.Add(new(loader.Handle(definition.DeclaringType!)) { Name = "this" });

            foreach (var parameter in definition.GetParameters())
                signature.Parameters.Add(new(ParameterType(parameter.ParameterType, target)) { Name = parameter.Name });

            if (definition.ReturnType != typeof(void))
                signature.ReturnType = Lower(TypeOf(definition.ReturnType), target);

            return Result<LIR.ICallable>.Ok(new LIR.MethodInstance(loader.Handle(definition), arguments.Select(argument => Lower(argument, target)), signature));
        }

        public IResult Call(Argument[] given)
        {
            var parameters = definition.GetParameters().Select(parameter => (CompilerObject)new MethodInstantiationParameter(this, parameter, loader));

            if (Arguments.Match(parameters, given) is not { } values)
                return Result.Error($"{definition.Name} can't be called with these arguments.");

            Func<CompilerObject>? returnType = definition.ReturnType == typeof(void) ? null : () => TypeOf(definition.ReturnType);

            return Result.Ok(new Call(Lowered, [.. (receiver is null ? [] : new[] { receiver }), .. values], returnType));
        }
    }

    internal sealed class MethodInstantiationParameter(MethodInstantiation owner, ParameterInfo parameter, ILLoader loader)
        : CompilerObject
        , ICTParameter
    {
        public IResult Match(ArgumentStream arguments)
        {
            if (!parameter.ParameterType.IsByRef)
                return Arguments.Take(
                    parameter.Name ?? string.Empty,
                    owner.TypeOf(parameter.ParameterType),
                    parameter.HasDefaultValue ? () => new Literal(parameter.DefaultValue, parameter.ParameterType, loader) : null,
                    arguments
                );

            // A reference parameter takes a variable of exactly its type, by its address.
            var element = owner.TypeOf(parameter.ParameterType.GetElementType()!);

            if ((arguments.Pop(parameter.Name ?? string.Empty) ?? arguments.Pop()) is not { } variable)
                return Result.Error($"No argument is given for parameter {parameter.Name}.");

            return variable.Is<ICTSet>(out _)
                && ZSharp.Compiler.Current.TS.TypeOf(variable).Ok(out var type)
                && ZSharp.Compiler.Current.TS.IsAssignableTo(type!, element)
                && ZSharp.Compiler.Current.TS.IsAssignableTo(element, type!)
                ? Result.Ok(new VariableAddress(variable, element, target => owner.ParameterType(parameter.ParameterType, target)))
                : Result.Error(new UnsupportedError($"Only a variable of the type can be passed to parameter {parameter.Name}."));
        }
    }

    /// <summary>
    /// A variable passed by its address, as a reference parameter takes it.
    /// </summary>
    internal sealed class VariableAddress(CompilerObject variable, CompilerObject type, Func<object?, LIR.Type> addressType)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => type;

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            if (variable.Is<IHasAddress>(out var addressable) && addressable.AddressOf(target) is { } own)
                return own;

            if (ZSharp.Compiler.Current.IR.CompileExpression(variable, target).When(out var compiled).Error(out var error))
                return Result<LIR.Expression>.Error(error);

            return LIR.CLR.AddressOf.Has(compiled!)
                ? Result<LIR.Expression>.Ok(new LIR.CLR.AddressOf(compiled!, addressType(target)))
                : Result<LIR.Expression>.Error("This variable has no address, so it can't be passed by reference here.");
        }
    }
}
