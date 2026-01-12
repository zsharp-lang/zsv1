namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Module
    {
        private string _name = string.Empty;

        public string Name         {
            get => _name;
            set
            {
                if (IsBuilt) throw new InvalidOperationException("Cannot change name after the module object is built.");
                if (value is null) throw new ArgumentNullException(nameof(value), "Name cannot be null.");
                _name = value;
            }
        }
    }
}
