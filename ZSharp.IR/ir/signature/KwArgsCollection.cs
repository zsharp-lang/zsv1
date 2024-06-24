using CommonZ.Utils;

namespace ZSharp.IR
{
    internal sealed class KwArgsCollection(Signature signature)
        : Mapping<string, Parameter>
    {
        public Signature Signature { get; } = signature;

        public override void OnAdd(string name, Parameter item)
        {
            AssertUnOwned(item);

            item.Signature = Signature;
            item.Index = Count + Signature.TotalNumArgs;
        }

        public override void OnRemove(string name)
        {
            Parameter parameter = this[name];

            foreach (var param in Values)
            {
                if (param.Index > parameter.Index)
                    param.Index--;
            }

            parameter.Signature = null;
            parameter.Index = -1;
        }

        private static void AssertUnOwned(Parameter item)
        {
            if (item.Signature is not null)
                throw new InvalidOperationException();
        }
    }
}
