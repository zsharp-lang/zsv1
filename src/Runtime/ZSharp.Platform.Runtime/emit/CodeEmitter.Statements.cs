namespace ZSharp.Platform.Runtime
{
    partial class CodeEmitter
    {
        private void Statement(LIR.Statement statement)
        {
            switch (statement)
            {
                case LIR.Block node:
                    foreach (var item in node.Statements)
                        Statement(item);
                    break;
                case LIR.Evaluate node: Evaluate(node); break;
                case LIR.SetLocal node: SetLocal(node); break;
                case LIR.SetGlobal node:
                    Expression(node.Value);
                    il.Emit(Emit.OpCodes.Stsfld, runtime.ImportGlobal(node.Global));
                    break;
                case LIR.SetField node:
                    FieldOwnerAddress(node.Target);
                    Expression(node.Value);
                    il.Emit(Emit.OpCodes.Stfld, runtime.ImportField(StructureOf(node.Target), node.Field));
                    break;
                case LIR.Store node:
                    Expression(node.Reference);
                    Expression(node.Value);
                    il.Emit(Emit.OpCodes.Stfld, Runtime.BoxValue(runtime.ImportType(TypeOf(node.Reference))));
                    break;
                case LIR.If node: If(node); break;
                case LIR.Loop node: Loop(node); break;
                case LIR.Break: il.Emit(Emit.OpCodes.Br, CurrentLoop.End); break;
                case LIR.Continue: il.Emit(Emit.OpCodes.Br, CurrentLoop.Start); break;
                case LIR.Return node:
                    if (node.Value is not null)
                        Expression(node.Value);
                    il.Emit(Emit.OpCodes.Ret);
                    break;
                default:
                    throw new NotSupportedException($"Can't emit a {statement.GetType().Name}.");
            }
        }

        private (Emit.Label Start, Emit.Label End) CurrentLoop
            => loops.TryPeek(out var loop)
                ? loop
                : throw new InvalidOperationException("Break or continue outside of a loop.");

        private void Evaluate(LIR.Evaluate node)
        {
            Expression(node.Expression);

            if (TypeOf(node.Expression) is not null)
                il.Emit(Emit.OpCodes.Pop);
        }

        private void SetLocal(LIR.SetLocal node)
        {
            Expression(node.Value);

            if (node.Variable is LIR.Parameter parameter)
                il.Emit(Emit.OpCodes.Starg, ParameterIndex(parameter));
            else
                il.Emit(Emit.OpCodes.Stloc, Local(node.Variable));
        }

        private void If(LIR.If node)
        {
            var otherwise = il.DefineLabel();
            var end = il.DefineLabel();

            Expression(node.Condition);
            il.Emit(Emit.OpCodes.Brfalse, otherwise);

            Statement(node.Then);
            il.Emit(Emit.OpCodes.Br, end);

            il.MarkLabel(otherwise);

            if (node.Else is not null)
                Statement(node.Else);

            il.MarkLabel(end);
        }

        private void Loop(LIR.Loop node)
        {
            var start = il.DefineLabel();
            var end = il.DefineLabel();

            il.MarkLabel(start);
            loops.Push((start, end));

            Statement(node.Body);

            loops.Pop();
            il.Emit(Emit.OpCodes.Br, start);
            il.MarkLabel(end);
        }
    }
}
