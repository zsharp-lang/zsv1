using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public sealed partial class Context
    {
        private readonly Stack<ContextCompiler> compilerStack = [];
        private readonly Mapping<Type, ContextCompiler> compilerMapping = [];

        internal Stack<ContextCompiler> CompilerStack => compilerStack;

        public ContextManager Of(ContextCompiler compiler)
        {
            compilerStack.Push(compiler);

            return new(() => compilerStack.Pop());
        }

        public ContextManager Of<CG>()
            where CG : CGObject
            => Of(GetCompilerFor<CG>());

        public void AddCompilerFor<CG>(ContextCompiler compiler)
            where CG : CGObject
            => compilerMapping.Add(typeof(CG), compiler);

        public void AddCompilerFor<R, CG>(ContextCompiler<R, CG> compiler)
            where R : RDefinition
            where CG : CGObject
            => compilerMapping.Add(typeof(CG), compiler);

        public ContextCompiler GetCompilerFor<CG>()
            where CG : CGObject
            => compilerMapping[typeof(CG)];

        public ContextCompiler<R, CG> GetCompilerFor<R, CG>()
            where R : RDefinition
            where CG : CGObject
            => (ContextCompiler<R, CG>)compilerMapping[typeof(CG)];
    }
}
