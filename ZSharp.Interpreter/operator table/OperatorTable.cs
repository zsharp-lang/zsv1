global using Operator = string;

using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Interpreter
{
    public sealed class OperatorTable()
    {
        private readonly Cache<Operator, CompilerObject> operators = new();

        internal OperatorTable(OperatorTable? parent = null)
            : this()
        {
            operators = new()
            {
                Parent = parent?.operators
            };
        }

        public void Op(Operator op, CompilerObject obj)
            => operators.Cache(op, obj); // TODO: on add

        public CompilerObject? Op(Operator op)
            => operators.Cache(op);

        public bool Op(Operator op, [NotNullWhen(true)] out CompilerObject? obj)
            => (obj = Op(op)) is not null;

        public OperatorTable CreateChild() => new(this);
    }
}
