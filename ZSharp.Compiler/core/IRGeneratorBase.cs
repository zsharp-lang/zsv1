using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal abstract class IRGeneratorBase<T, IR>(IRGenerator irGenerator, T @object)
        : ICompileHandler
        where T : CGObject
        where IR : ZSharp.IR.IRObject
    {
        protected IRGenerator IRGenerator { get; } = irGenerator;

        protected T Object { get; } = @object;

        public IR Run()
        {
            using (IRGenerator.CompileHandler(this))
                return Compile();
        }

        public abstract void CompileObject(CGObject @object);

        protected abstract IR Compile();
    }
}
