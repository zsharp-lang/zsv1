namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Global
        : ITyped
    {
        private CompilerObject _type;

        public CompilerObject Type
        {
            get => _type;
            set {
                if (IsBuilt) throw new InvalidOperationException("Cannot change type after the global object is built.");

                if (value is null) throw new ArgumentNullException(nameof(value), "Type cannot be null.");

                _type = value;
            }
        }
    }
}
