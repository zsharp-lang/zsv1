using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    public sealed class CompilerContext(CTScope ctGlobals, RTScope rtGlobals)
    {
        private DomainContext<CTType> _ctContext = new(ctGlobals);
        private DomainContext<RTType> _rtContext = new(rtGlobals);

        public DomainContext<CTType> CT => _ctContext!;

        public DomainContext<RTType> RT => _rtContext!;


        public ContextManager UseCTContext(DomainContext<CTType> ctContext)
        {
            (ctContext, _ctContext) = (_ctContext!, ctContext);

            return new(() =>
            {
                _ctContext = ctContext;
            });
        }

        public ContextManager UseRTContext(DomainContext<RTType> rtContext)
        {
            (rtContext, _rtContext) = (_rtContext!, rtContext);

            return new(() =>
            {
                _rtContext = rtContext;
            });
        }

        public ICTBinding? GetCT(RNode node)
            => CT.Scope.Cache(node);

        public T? GetCT<T>(RNode node)
            where T : class, ICTBinding
            => CT.Scope.Cache<T>(node);

        public IRTBinding? GetRT(RNode node)
            => RT.Scope.Cache(node);

        public T? GetRT<T>(RNode node)
            where T : class, IRTBinding
            => RT.Scope.Cache<T>(node);

        public T Set<T>(RNode node, T binding)
            where T : ICTBinding, IRTBinding
        {
            SetCT(node, binding);
            SetRT(node, binding);
            return binding;
        }

        public T SetCT<T>(RNode node, T binding)
            where T : ICTBinding
            => CT.Scope.Cache(node, binding);

        public T SetRT<T>(RNode node, T binding)
            where T : IRTBinding
            => RT.Scope.Cache(node, binding);
    }
}
