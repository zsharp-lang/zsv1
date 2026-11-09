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
        , IInferable
        , ICTGetIndex
        , ICompileIRExpression
        , ITyped
        , ICTImplicitCastTo
        , ICTCastTo
    {
        // A method is a value only if it has a delegate type: a constructor or an open
        // generic method isn't.
        bool CompilerObject.Is<T>([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? result) where T : class
            => (result = (typeof(T) == typeof(ITyped) || typeof(T) == typeof(ICompileIRExpression)) && Natural() is null ? null : this as T) is not null;

        CompilerObject ITyped.Type => loader.Class(Natural()!);

        /// <summary>
        /// A method used as a value is a delegate of its own signature.
        /// </summary>
        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
            => new MethodValue(loader, (MethodInfo)method, receiver, Natural()!).CompileIRExpression(target);

        /// <summary>
        /// A method goes where a delegate of a type is expected if its signature fits.
        /// </summary>
        public IResult ImplicitCast(CompilerObject type)
            => method is MethodInfo info && type.Is<IILType>(out var known) && MethodValue.Fits(info, known.GetILType())
                ? Result.Ok(new MethodValue(loader, info, receiver, known.GetILType()))
                : Result.Error(new UnsupportedError($"{method.Name} isn't a delegate of that type."));

        public IResult Cast(CompilerObject targetType)
            => ImplicitCast(targetType);

        private Type? Natural()
            => method is MethodInfo info ? MethodValue.Natural(info) : null;

        /// <summary>
        /// A generic method given types that aren't CLR types yet, as a class being loaded
        /// with the code, if each is a type and there is one for each type parameter.
        /// </summary>
        private MethodInstantiation? Instantiated(MethodInfo generic, Argument[] arguments)
        {
            if (arguments.Length != generic.GetGenericArguments().Length || arguments.Any(argument => argument.Name is not null))
                return null;

            if (arguments.Any(argument => !argument.Object.Is<ICompileIRType>(out _)))
                return null;

            return new MethodInstantiation(loader, generic, [.. arguments.Select(argument => argument.Object)], receiver);
        }

        /// <summary>
        /// Giving a generic method its arguments, as in <c>f[Int32]</c>, instantiates it.
        /// </summary>
        public IResult Index(Argument[] arguments)
        {
            if (method is not MethodInfo { IsGenericMethodDefinition: true } generic)
                return Result.Error(new UnsupportedError($"{method.Name} is not generic."));

            if (loader.Types(arguments, generic.GetGenericArguments()) is not { } types)
                return Instantiated(generic, arguments) is { } instantiation
                    ? Result.Ok(instantiation)
                    : Result.Error($"{method.Name} takes {generic.GetGenericArguments().Length} type arguments.");

            try
            {
                return Result.Ok(new Method(loader, generic.MakeGenericMethod(types), receiver));
            }
            catch (ArgumentException exception)
            {
                return Result.Error(exception.Message);
            }
        }

        /// <summary>
        /// A generic method called without its type arguments infers them from the call's
        /// arguments, as in C#.
        /// </summary>
        public CompilerObject? Infer(Argument[] arguments)
        {
            if (method is not MethodInfo { IsGenericMethodDefinition: true } generic || Inference.Infer(generic, arguments) is not { } types)
                return null;

            try
            {
                return new Method(loader, generic.MakeGenericMethod(types), receiver);
            }
            catch (ArgumentException)
            {
                // The inferred types don't meet the method's constraints.
                return null;
            }
        }

        public IResult Call(Argument[] arguments)
        {
            if (method is MethodInfo { IsGenericMethodDefinition: true })
                return Infer(arguments) is { } inferred
                    ? Core.Compiler.ModuleScope.CG.Call(inferred, arguments)
                    : Result.Error($"The type arguments of {method.Name} can't be inferred from these arguments.");

            if (Arguments.Match(Matching(), arguments) is not { } values)
                return Result.Error($"{method.Name} can't be called with these arguments.");

            CompilerObject[] bound = [];

            if (receiver is not null)
                if (method.DeclaringType is { IsValueType: false } owner
                    && Core.Compiler.ModuleScope.CG.ImplicitCast(receiver, loader.Class(owner)).Ok(out var converted))
                    bound = [converted];
                else
                    bound = [receiver];

            return Result.Ok(new Call(
                loader.Handle(method),
                [.. bound, .. values],
                ILLoader.ReturnType(method) is { } type ? () => loader.Class(type) : null
            ));
        }

        public IResult<LIR.ICallable, Error> CompileIRReference(object? target)
            => Result<LIR.ICallable>.Ok(loader.Handle(method));

        /// <summary>
        /// A method is as specific as another if the other takes whatever it takes. Between
        /// two that take the same, one that isn't generic is the more specific, as in C#.
        /// </summary>
        public bool IsAsSpecificAs(CompilerObject other)
            => ZSharp.Compiler.Parameters.AreAsSpecificAs(Parameters(), other)
            && !(method.IsGenericMethod && other is Method { IsGeneric: false } plain && plain.TakesWhat(this));

        private bool IsGeneric => method.IsGenericMethod;

        private bool TakesWhat(Method other)
            => ZSharp.Compiler.Parameters.AreAsSpecificAs(Parameters(), other);

        private CompilerObject[] Parameters()
            => [.. method.GetParameters().Select(loader.ParameterType)];

        /// <summary>
        /// The parameters as they match arguments, as its metadata has them.
        /// </summary>
        private CompilerObject[] Matching()
            => [.. method.GetParameters().Select(parameter => new ClrParameter(parameter, loader))];
    }
}
