namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// Emits the body of one function.
    /// </summary>
    internal sealed partial class CodeEmitter(Runtime runtime, Emit.ILGenerator il, LIR.Function function)
    {
        private readonly Dictionary<LIR.Local, Emit.LocalBuilder> locals = [];
        private readonly Dictionary<LIR.Loop, (Emit.Label Start, Emit.Label End)> loops = [];

        private LIR.Signature Signature => function.Signature;

        public void EmitBody()
        {
            if (function.Body is not { } code)
                throw new InvalidOperationException($"Function {function.Name} has no body.");

            Statement(code.Block);

            // Reaching the end is valid only when there is nothing to return.
            if (Signature.ReturnType is null)
                il.Emit(Emit.OpCodes.Ret);
            else
            {
                il.Emit(Emit.OpCodes.Ldnull);
                il.Emit(Emit.OpCodes.Throw);
            }
        }

        private short ParameterIndex(LIR.Parameter parameter)
        {
            var index = Signature.Parameters.IndexOf(parameter);

            if (index < 0)
                throw new InvalidOperationException($"Parameter {parameter.Name} is not in function {function.Name}.");

            return (short)index;
        }

        private Emit.LocalBuilder Local(LIR.Variable variable)
            => variable is LIR.Local local && locals.TryGetValue(local, out var builder)
                ? builder
                : throw new InvalidOperationException($"A local is not in function {function.Name}.");
    }
}
