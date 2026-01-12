namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Module
    {
        private CompilerObject? entryPoint;

        public CompilerObject? EntryPoint
        {
            get => entryPoint;
            set {
                if (state[BuildState.EntryPoint])
                    throw new InvalidOperationException($"Cannot set entrypoint on an already built module");
                entryPoint = value;
            }
        }
    }
}
