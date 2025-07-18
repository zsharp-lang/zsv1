namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class Module
    {
        private ModuleLoader? _moduleLoader;

        public required ILLoader Loader { get; init; }

        internal ModuleLoader GlobalsLoader
        {
            get
            {
                if (_moduleLoader is not null)
                    return _moduleLoader;

                Interlocked.CompareExchange(ref _moduleLoader, new(Loader), null);
                return _moduleLoader;
            }
        }

        public CompilerObject? LoadMember(string name)
        {
            var members = Globals?.GetMember(name);

            if (members is not null && members.Length > 0)
            {
                foreach (var member in members)
                    AddMember(name, GlobalsLoader.LoadMember(member));

                return Members[name];
            }

            var type = IL.GetType(name);
            if (type is null) return null;
            if (!type.IsPublic) return null;
            return Loader.LoadType(type);
        }
    }
}
