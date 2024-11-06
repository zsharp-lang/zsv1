using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        internal ObjectCache ObjectCache { get; } = new();

        public CGObject ImportIR(IRType type)
            => type is IR.IRObject irObject
            ? ImportIR(irObject)
            : new RawType(type); // TODO: other default implementation for raw type

        public CGObject ImportIR(IR.IRObject ir)
            => ObjectCache.CG(ir, out var result)
            ? result
            : ObjectCache.CG(ir, ir switch
            {
                IR.Class @class => LoadIR(@class),
                IR.Function function => LoadIR(function),
                IR.Module module => LoadIR(module),
                _ => throw new NotImplementedException(),
            });

        public Module ImportIR(IR.Module module)
            => ObjectCache.CG<Module>(module, out var result)
            ? result
            : ObjectCache.CG(module, LoadIR(module));

        public RTFunction ImportIR(IR.Function function)
            => ObjectCache.CG<RTFunction>(function, out var result)
            ? result
            : ObjectCache.CG(function, LoadIR(function));

        public Class ImportIR(IR.Class @class)
            => ObjectCache.CG<Class>(@class, out var result)
            ? result
            : ObjectCache.CG(@class, LoadIR(@class));
    }
}

