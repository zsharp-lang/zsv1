using CommonZ.Utils;
using ZSharp.RAST;
using ZSharp.VM;

namespace ZSharp.CTRuntime
{
    public sealed class ZSCompiler
    {
        private readonly Interpreter _interpreter;

        private readonly CTScope ctGlobals = new();
        private readonly RTScope rtGlobals = new();

        private readonly ModuleCompiler moduleCompiler;

        private CompilerContext? _context;

        public CompilerContext Context => _context!;

        public Interpreter Interpreter => _interpreter;

        public ZSCompiler() : this(new(IR.RuntimeModule.Standard)) { }

        public ZSCompiler(Interpreter interpreter)
        {
            _interpreter = interpreter;

            moduleCompiler = new(this);
        }

        public ContextManager UseContext()
        {
            var context = new CompilerContext(ctGlobals, rtGlobals);
            (context, _context) = (_context!, context);

            return new(() =>
            {
                _context = context;
            });
        }

        public IR.Module Compile(RModule module)
        {
            return moduleCompiler.Compile(module);
        }

        public void DefineGlobal<T>(RNode node, T binding)
            where T : ICTBinding, IRTBinding
        {
            DefineCTGlobal(node, binding);
            DefineRTGlobal(node, binding);
        }

        public void DefineCTGlobal(RNode node, ICTBinding binding)
            => ctGlobals.Cache(node, binding);

        public void DefineRTGlobal(RNode node, IRTBinding binding)
            => rtGlobals.Cache(node, binding);
    }
}
