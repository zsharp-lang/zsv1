using System.Diagnostics.CodeAnalysis;

namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class Namespace
    {
        public Namespace? Parent { get; private set; }

        [MemberNotNullWhen(true, nameof(Parent))]
        public bool HasParent => Parent is not null;

        public string FullName =>
            Parent is null ? Name : $"{Parent.FullName}.{Name}";
    }
}
