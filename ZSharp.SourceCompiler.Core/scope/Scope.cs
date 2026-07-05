using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using MemberName = string;

namespace ZSharp.SourceCompiler
{
    public sealed class Scope()
        : IScope
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

        public bool Add(MemberName name, CompilerObject value, [NotNullWhen(false)] out Error? error)
        {
            Add(name, value);
            return (error = null) is null;
        }

        public CompilerObject? Get(MemberName name)
            => scope.Cache(name, out var value) ? value : null;

        public bool Get(
            string name,
            [NotNullWhen(true)] out CompilerObject? @object,
            [NotNullWhen(false)] out Error? error
        )
        {
            if (Get(name) is CompilerObject value)
                return ((error, @object) = (null, value)).@object is not null;

            return ((error, @object) = (new ErrorMessage($"Could not resolve name {name}"), null)).error is null;
        }

        public void Set(MemberName name, CompilerObject value)
            => scope.Cache(name, value, set: true);

        public bool Set(string name, CompilerObject @object, [NotNullWhen(false)] out Error? error)
        {
            Set(name, @object);
            return (error = null) is null;
        }
    }
}
