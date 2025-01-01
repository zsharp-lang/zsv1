using System.Diagnostics.CodeAnalysis;

namespace ZSharp.IR
{
    public interface ICallable
    {
        public Signature Signature { get; }

        [MemberNotNullWhen(true, nameof(Body))]
        public bool HasBody { get; }

        public ICallableBody? Body => null;
    }
}
