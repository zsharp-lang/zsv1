using CommonZ.Utils;
using ZSharp.AST;

namespace ZSharp.SourceCompiler
{
    internal sealed class DocumentContext(Document document, Scope? globalScope = null)
        : IContext
    {
        private Scope currentScope = globalScope ?? new();
        internal readonly TaskManager tasks = new();

        Cache<Type, Func<IContext, Node, IResult<CompilerObject, Error>>> IContext.Overrides => throw new NotImplementedException();

        Scope IContext.CurrentScope => currentScope;

        void IContext.AddTask(Action task)
            => tasks.AddTask(task);

        CompilerObject IContext.CreateSlot()
        {
            throw new NotImplementedException();
        }

        void IContext.Define(CompilerObject definition)
        {
            throw new NotImplementedException();
        }

        void IContext.Emit(CompilerObject code)
            => document.Content.Add(code);

        ContextManager IContext.Scope(Scope scope)
        {
            (currentScope, scope) = (scope, currentScope);

            return new(() => currentScope = scope);
        }
    }
}
