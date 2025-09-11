namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Function
    {
        public string _name;

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
