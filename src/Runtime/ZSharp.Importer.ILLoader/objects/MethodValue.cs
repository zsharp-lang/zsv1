using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A method as a value: a delegate of a type, bound to the object the method was
    /// reached through, if any.
    /// </summary>
    internal sealed class MethodValue(ILLoader loader, MethodInfo method, CompilerObject? receiver, Type delegateType)
        : CompilerObject
        , ICompileIRExpression
        , ITyped
    {
        CompilerObject ITyped.Type => loader.Class(delegateType);

        public IResult<LIR.Expression, Error> CompileIRExpression(object? target)
        {
            LIR.Expression bound = new LIR.Constant(loader.Handle(typeof(object)), null);

            if (receiver is not null)
                if (Core.Compiler.ModuleScope.IR.CompileExpression(receiver, target).When(out var compiled).Error(out var error))
                    return Result<LIR.Expression>.Error(error);
                else
                    bound = compiled!;

            return Result<LIR.Expression>.Ok(new LIR.Call(loader.Delegate(delegateType, method), [bound]));
        }

        /// <summary>
        /// The delegate type a method is a value of when nothing asks for another: a
        /// <c>Func</c> or an <c>Action</c> of its signature.
        /// </summary>
        public static Type? Natural(MethodInfo method)
        {
            if (method.ContainsGenericParameters)
                return null;

            var types = method.GetParameters().Select(parameter => parameter.ParameterType).ToList();

            if (types.Any(type => type.IsByRef || type.IsPointer))
                return null;

            types.Add(method.ReturnType);

            return System.Linq.Expressions.Expression.GetDelegateType([.. types]);
        }

        /// <summary>
        /// Whether a method can be a delegate of a type: the delegate's parameters go to the
        /// method's, and what the method returns goes where the delegate's result does.
        /// </summary>
        public static bool Fits(MethodInfo method, Type delegateType)
            => Fits([.. method.GetParameters().Select(parameter => parameter.ParameterType)], method.ReturnType, delegateType);

        /// <summary>
        /// Whether a function of a signature can be a delegate of a type.
        /// </summary>
        public static bool Fits(Type[] parameters, Type returnType, Type delegateType)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType) || delegateType.GetMethod("Invoke") is not { } invoke)
                return false;

            var accepted = invoke.GetParameters();

            return parameters.Length == accepted.Length
                && parameters.Zip(accepted).All(pair => pair.First.IsAssignableFrom(pair.Second.ParameterType))
                && (invoke.ReturnType == typeof(void)
                    ? returnType == typeof(void)
                    : invoke.ReturnType.IsAssignableFrom(returnType));
        }
    }
}
