using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class Scope(Scope? parent)
    {
        private readonly Mapping<string, CompilerObject> scope = [];
        private readonly Mapping<CompilerObject, List<Constraint>> constraints = [];

        public Scope? Parent { get; } = parent;

        public CompilerObject? Get(string name, bool lookupParent = true)
        {
            if (scope.TryGetValue(name, out var result))
                return result;

            if (lookupParent && Parent is not null)
                return Parent.Get(name);

            return null;
        }

        public bool Get(string name, [NotNullWhen(true)] out CompilerObject? result, bool lookupParent = true)
            => (result = Get(name, lookupParent: lookupParent)) is not null;

        public void Set(string name, CompilerObject value, bool @override = false)
        {
            if (scope.ContainsKey(name) && !@override)
                throw new(); // TODO: Throw a proper exception of NameAlreadyExists

            scope[name] = value;
        }

        public IEnumerable<Constraint> Constraints(string name, bool lookupParent = true)
            => Get(name, lookupParent: lookupParent) is CompilerObject @object ? Constraints(@object, lookupParent: lookupParent) : [];

        public IEnumerable<Constraint> Constraints(CompilerObject @object, bool lookupParent = true)
        {
            if (constraints.TryGetValue(@object, out var result))
                return result;

            if (lookupParent && Parent is not null)
                return Parent.Constraints(@object);

            return [];
        }

        public void Constriant(string name, Constraint constraint, bool lookupParent = true)
        {
            if (Get(name, lookupParent: lookupParent) is CompilerObject @object)
                Constriant(@object, constraint);
            else throw new(); // TODO: Throw a proper exception of UnresolvedName
        }

        public void Constriant(CompilerObject @object, Constraint constraint)
        {
            if (constraints.TryGetValue(@object, out var result))
                result.Add(constraint);
            else constraints[@object] = [constraint];
        }

        public void Constraints(string name, Constraint[] constraints, bool lookupParent = true)
        {
            if (Get(name, lookupParent: lookupParent) is CompilerObject @object)
                Constraints(@object, constraints);
            else throw new(); // TODO: Throw a proper exception of UnresolvedName
        }

        public void Constraints(CompilerObject @object, Constraint[] constraints)
        {
            if (this.constraints.TryGetValue(@object, out var result))
                result.AddRange(constraints);
            else this.constraints[@object] = constraints.ToList();
        }
    }
}
