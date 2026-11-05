using System.Reflection;

namespace ZSharp.Platform.Runtime
{
    partial class Runtime
    {
        /// <summary>
        /// Runs a statement on its own, in a module made for it.
        /// </summary>
        public void Execute(LIR.Statement statement)
            => Invoke(new LIR.Signature(), statement);

        /// <summary>
        /// Evaluates an expression on its own, in a module made for it.
        /// </summary>
        public object? Evaluate(LIR.Expression expression)
            => CodeEmitter.TypeOf(expression) is { } type
                ? Invoke(new LIR.Signature { ReturnType = type }, new LIR.Return(expression))
                : Invoke(new LIR.Signature(), new LIR.Evaluate(expression));

        private object? Invoke(LIR.Signature signature, LIR.Statement statement)
        {
            var code = new LIR.Code();
            code.Block.Statements.Add(statement);

            var function = new LIR.Function(signature) { Body = code };

            var module = new LIR.Module();
            module.Functions.Add(function);

            Load(module);

            return GetMethod(function).Invoke(null, BindingFlags.DoNotWrapExceptions, null, null, null);
        }
    }
}
