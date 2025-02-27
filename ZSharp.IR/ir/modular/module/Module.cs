using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Module(string? name) : ModuleMember
    {
        private Function? _initializer;
        private ModuleCollection<ImportedModule>? _importedModules;
        private ModuleCollection<Function>? _functions;
        private GlobalCollection? _globals;
        private ModuleCollection<Module>? _submodules;
        private ModuleCollection<OOPType>? _types;

        public string? Name { get; set; } = name;

        public ModuleAttributes Attributes { get; set; } = ModuleAttributes.None;

        public Function? Initializer
        {
            get => _initializer;
            set
            {
                if (value is not null)
                {
                    if (value.Owner is null)
                        Functions.Add(value);
                    else if (value.Owner != this)
                        throw new InvalidOperationException("Module initializer cannot reside in a different module.");
                    _initializer = value;
                }
            }
        }

        public Collection<ImportedModule> ImportedModules
        {
            get
            {
                if (_importedModules is not null)
                    return _importedModules;

                Interlocked.CompareExchange(ref _importedModules, new(this), null);
                return _importedModules;
            }
        }

        public bool HasImportedModules => !_importedModules.IsNullOrEmpty();

        public Collection<Function> Functions
        {
            get
            {
                if (_functions is not null)
                    return _functions;

                Interlocked.CompareExchange(ref _functions, new(this), null);
                return _functions;
            }
        }

        public bool HasFunctions => !_functions.IsNullOrEmpty();

        public Collection<Global> Globals
        {
            get
            {
                if (_globals is not null)
                    return _globals;

                Interlocked.CompareExchange(ref _globals, new(this), null);
                return _globals;
            }
        }

        public bool HasGlobals => !_globals.IsNullOrEmpty();

        public Collection<Module> Submodules
        {
            get
            {
                if (_submodules is not null)
                    return _submodules;

                Interlocked.CompareExchange(ref _submodules, new(this), null);
                return _submodules;
            }
        }

        public bool HasSubmodules => !_submodules.IsNullOrEmpty();

        public Collection<OOPType> Types
        {
            get
            {
                if (_types is not null)
                    return _types;

                Interlocked.CompareExchange(ref _types, new(this), null);
                return _types;
            }
        }

        public bool HasTypes => !_types.IsNullOrEmpty();
    }
}
