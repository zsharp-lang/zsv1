namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Module
    {
        private CompilerObject? initializer;

        public CompilerObject? Initializer
        {
            get => initializer;
            set {
                if (state[BuildState.Initializer])
                    throw new InvalidOperationException($"Cannot set initializer on an already built module");
                initializer = value;
            }
        }
    }
}
