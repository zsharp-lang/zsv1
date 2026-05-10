namespace ZSharp.SourceCompiler
{
    public sealed partial class CodeBuilder
    {
        private readonly Function fn = new();

        public CompilerObject CreateCode()
            => fn;

        public CompilerObject CreateLocal()
        {
            Local local = new();

            fn.Locals.Add(local);

            return local;
        }

        public void Emit(CompilerObject code)
            => fn.Body.Add(code);
    }
}
