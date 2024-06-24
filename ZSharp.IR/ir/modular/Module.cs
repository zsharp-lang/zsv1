using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class Module(string? name) : IRObject
    {
        private ModuleCollection<Function>? _functions;
        private GlobalCollection? _globals;
        private ModuleCollection<Module>? _submodules;
        private ModuleCollection<OOPType>? _types;

        public string? Name { get; set; } = name;

        public ModuleAttributes Attributes { get; set; } = ModuleAttributes.None;

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
    }
}
