using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal sealed class Context
    {
        private readonly Stack<ContextCompiler> _stack = [];

        private readonly ExpressionCompiler _expressionCompiler;
        private readonly StatementCompiler _statementCompiler;

        public Context()
        {
            _expressionCompiler = new(this);
            _statementCompiler = new(this);

            _stack.Push(new ModuleCompiler(this));
        }

        public ContextCompiler CurrentCompiler => _stack.Peek();

        public CGObject Compile(RDefinition definition)
        {
            foreach (var compiler in _stack)
            {
                if (compiler.Compile(definition) is CGObject @object)
                    return @object;
            }

            throw new Exception($"Could not compile node of type {definition.GetType().Name}");
        }

        public CGCode Compile(RExpression expression)
            => _expressionCompiler.Compile(expression);

        public bool Compile(RStatement statement)
        {
            if (_statementCompiler.Compile(statement))
                return true;

            foreach (var compiler in _stack)
            {
                if (compiler.Compile(statement))
                    return true;
            }

            return false;
        }

        public void Emit(CGCode code)
            => CurrentCompiler.AddCode(code);

        public ContextManager Of(ContextCompiler compiler)
        {
            _stack.Push(compiler);

            return new(() =>
            {
                _stack.Pop();
            });
        }
    }
}
