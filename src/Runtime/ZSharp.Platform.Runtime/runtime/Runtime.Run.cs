using System.Reflection;

namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        /// <summary>
        /// Runs a statement on its own, in a module made for it. The statement can use the
        /// given parameters, which take the given arguments.
        /// </summary>
        public void Execute(LIR.Statement statement, params (LIR.Parameter Parameter, object? Argument)[] arguments)
            => Invoke(Signature(null, arguments), statement, arguments);

        /// <summary>
        /// Evaluates an expression on its own, in a module made for it. The expression
        /// can use the given parameters, which take the given arguments.
        /// </summary>
        public object? Evaluate(LIR.Expression expression, params (LIR.Parameter Parameter, object? Argument)[] arguments)
            => CodeEmitter.TypeOf(expression) is { } type
                ? Invoke(Signature(type, arguments), new LIR.Return(expression), arguments)
                : Invoke(Signature(null, arguments), new LIR.Evaluate(expression), arguments);

        private static LIR.Signature Signature(LIR.Type? returnType, (LIR.Parameter Parameter, object? Argument)[] arguments)
        {
            var signature = new LIR.Signature { ReturnType = returnType };

            signature.Parameters.AddRange(arguments.Select(argument => argument.Parameter));

            return signature;
        }

        private object? Invoke(LIR.Signature signature, LIR.Statement statement, (LIR.Parameter Parameter, object? Argument)[] arguments)
        {
            var code = new LIR.Code();
            code.Block.Statements.Add(statement);

            var function = new LIR.Function(signature) { Body = code };

            var module = new LIR.Module();
            module.Functions.Add(function);

            Load(module);

            return GetMethod(function).Invoke(
                null,
                BindingFlags.DoNotWrapExceptions,
                null,
                [.. arguments.Select(argument => argument.Argument)],
                null
            );
        }
    }
}
