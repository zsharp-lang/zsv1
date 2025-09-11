namespace ZSharp.SourceCompiler.Module.Objects
{
    partial class Global
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (IsBuilt) throw new InvalidOperationException("Cannot change name after the global object is built.");

                if (value is null) throw new ArgumentNullException(nameof(value), "Name cannot be null.");

                _name = value;
            }
        }
    }
}
