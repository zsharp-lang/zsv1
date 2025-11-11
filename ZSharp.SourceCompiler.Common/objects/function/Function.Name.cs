namespace ZSharp.SourceCompiler.Objects
{
    partial class Function
        : IHasName
    {
        public string _name = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                if (IsBuilt) throw new InvalidOperationException("Cannot change name after the function object is built.");
                if (value is null) throw new ArgumentNullException(nameof(value), "Name cannot be null.");
                _name = value;
            }
        }
    }
}
