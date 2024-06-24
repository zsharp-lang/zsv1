using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal abstract class CodeCompiler<T>(ZSCompiler compiler, IProcessor<T> processor)
        : CompilerBase(compiler)
        where T : class
    {
        private IDefinitionCompiler<T>? definitionCompiler;
        private IStatementCompiler<T>? statementCompiler;

        public abstract DomainContext<T> CodeContext { get; }

        public IProcessor<T> Processor { get; } = processor;

        public ContextManager UseDefinitionCompiler(IDefinitionCompiler<T> compiler)
        {
            (compiler, definitionCompiler) = (definitionCompiler!, compiler);

            return new(() =>
            {
                definitionCompiler = compiler;
            });
        }

        public ContextManager UseStatementCompiler(IStatementCompiler<T> compiler)
        {
            (compiler, statementCompiler) = (statementCompiler!, compiler);

            return new(() =>
            {
                statementCompiler = compiler;
            });
        }

        public abstract T Combine(params T[] items);

        public T Compile(RStatement statement)
        {
            return statement switch
            {
                RBlock block => Compile(block),
                RExpressionStatement expressionStatement => Compile(expressionStatement),
                _ => statementCompiler!.Compile(statement).Read(Compiler),
            };
        }

        #region RStatement

        private T Compile(RBlock block)
        {
            ContextManager scope = block.IsScoped ? CodeContext.UseScope(block) : ContextManager.Empty;

            T[] items = new T[block.Statements.Count];

            using (scope)
                for (int i = 0; i < block.Statements.Count; i++)
                    items[i] = Compile(block.Statements[i]);

            return Combine(items);
        }

        private T Compile(RExpressionStatement expressionStatement)
        {
            var binding = Bind(expressionStatement.Expression);

            // todo: add void as expression
            //binding = Processor.Cast(binding, Binder.Bind("void")); 

            return binding.Read(Compiler);
        }

        #endregion

        #region RExpression

        public IBinding<T> Bind(RExpression expression)
        {
            return expression switch
            {
                RCall call => Bind(call),
                RDefinition definition => Bind(definition),
                RId id => Bind(id),
                RLiteral literal => Bind(literal),
                _ => throw new NotImplementedException(),
            };
        }

        private IBinding<T> Bind(RCall call)
        {
            var callee = Bind(call.Callee);

            var args = call.Arguments.Select(arg => new Argument<T>(arg.Name, Bind(arg.Value))).ToArray();

            return Processor.Call(callee, args);
        }

        public IBinding<T> Bind(RDefinition definition)
            => definitionCompiler!.Compile(definition);

        private IBinding<T> Bind(RId id)
            => CodeContext.Scope.Cache(id) ?? throw new Exception();

        private IBinding<T> Bind(RLiteral literal)
            => Processor.Literal(
                literal.Value,
                literal.Type,
                literal.UnitType is null ? null : Bind(literal.UnitType));

        #endregion
    }
}
