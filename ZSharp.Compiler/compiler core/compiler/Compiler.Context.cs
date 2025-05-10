using CommonZ.Utils;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        public IContext CurrentContext { get; private set; } = new EmptyContext();

        public Action UseContext(IContext context)
        {
            (CurrentContext, context) = (context, CurrentContext);
            CurrentContext.Parent = context;

            return () => CurrentContext = context;
        }

        public ContextManager ContextScope(IContext context)
            => new(UseContext(context));
    }
}
