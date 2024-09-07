using CommonZ.Utils;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public abstract class ContextCompiler(Compiler compiler)
        : CompilerBase(compiler)
    {
        protected internal abstract CGObject? Compile(RDefinition definition);
    }

    public abstract class ContextCompiler<R, CG>(Compiler compiler)
        : ContextCompiler(compiler)
        where R : RDefinition
        where CG : CGObject
    {
        public R Node { get; private set; } = null!;

        public CG Result { get; private set; } = null!;

        protected internal override CGObject? Compile(RDefinition definition)
        {
            if (definition is R node)
                return Compile(node);

            return CompileContextItem(definition);
        }

        protected virtual CGObject? CompileContextItem(RDefinition definition) => null;

        protected abstract CG Create();

        protected abstract void Compile();

        protected virtual CG Register(CG result)
        {
            if (Node.Name is not null && Node.Name != string.Empty)
                return Context.CurrentScope.Cache(Node.Name, result);

            return result;
        }

        internal CG Compile(R definition, CG? result = null)
        {
            (Node, definition) = (definition, Node);

            result ??= Register(Create());

            (Result, result) = (result, Result);

            using (new ContextManager(() =>
            {
                Node = definition;
                Result = result;
            }))
            {
                using (Context.For(Result))
                using (Context.Of(this))
                    Compile();

                return Result;
            }
        }
    }
}
