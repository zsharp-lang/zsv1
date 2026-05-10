using CommonZ.Utils;

using MemberName = string;

namespace ZSharp.SourceCompiler
{
    public sealed class Scope()
    {
        private readonly Cache<MemberName, CompilerObject> scope = [];

        private Scope(Scope parent) 
            : this()
        { 
            scope = new()
            {
                Parent = (Parent = parent).scope
            };
        }

        public Scope? Parent { get; private init; }

        public Scope CreateChildScope()
            => new(this);

        public void Add(MemberName name, CompilerObject value)
            => scope.Cache(name, value);

        public CompilerObject? Get(MemberName name)
            => scope.Cache(name, out var value) ? value : null;

        public void Set(MemberName name, CompilerObject value)
            => scope.Cache(name, value, set: true);
    }
}
