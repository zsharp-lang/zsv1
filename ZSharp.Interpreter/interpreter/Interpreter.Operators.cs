using CommonZ.Utils;

namespace ZSharp.Interpreter
{
    partial class Interpreter
    {
        public OperatorTable Operators { get; private set; } = new();

        public ContextManager OperatorScope(out OperatorTable scopedTable)
        {
            scopedTable = Operators.CreateChild();

            var previous = Operators;
            Operators = scopedTable;

            return new ContextManager(() => Operators = previous);
        }
    }
}
