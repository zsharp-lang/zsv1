namespace ZSharp.Platform.Runtime
{
    partial class CodeEmitter
    {
        /// <summary>
        /// Gets the type of an expression, or <see langword="null"/> if it produces
        /// no value.
        /// </summary>
        private static LIR.Type? TypeOf(LIR.Expression expression)
            => expression switch
            {
                LIR.Constant node => node.Type,
                LIR.GetLocal node => node.Variable.Type,
                LIR.GetGlobal node => node.Global.Type,
                LIR.GetField node => node.Field.Type,
                LIR.GetFunction node => new LIR.FunctionType(node.Callable.Signature),
                LIR.Call node => node.Callable.Signature.ReturnType,
                LIR.CallIndirect node => node.Signature.ReturnType,
                LIR.New node => node.Structure,
                LIR.Allocate node => new LIR.Reference(TypeOf(node.Value) ?? NoValue(node)),
                LIR.Load node => TargetOf(node.Reference),
                LIR.Sequence node => TypeOf(node.Result),
                _ => throw new NotSupportedException($"Can't type a {expression.GetType().Name}.")
            };

        private static LIR.Type TargetOf(LIR.Expression reference)
            => TypeOf(reference) is LIR.Reference type
                ? type.Target
                : throw new InvalidOperationException("Expected a reference.");

        private static LIR.Structure StructureOf(LIR.Expression target)
            => (TypeOf(target) is LIR.Reference reference ? reference.Target : TypeOf(target)) as LIR.Structure
                ?? throw new InvalidOperationException("Expected a structure or a reference to one.");

        private static LIR.Type NoValue(LIR.Expression expression)
            => throw new InvalidOperationException($"A {expression.GetType().Name} needs a value.");
    }
}
