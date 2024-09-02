using CommonZ.Utils;
using ZSharp.IR;

namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        internal readonly Mapping<IRObject, ZSObject> irMap = [];

        public ZSObject LoadIR(IRObject ir)
            => LoadIRWithCaching(ir);

        //public ZSObject ReloadIR(IRObject ir)
        //    => irMap[ir] = LoadIRWithoutCaching(ir);

        //public void UnloadIR(IRObject ir)
        //    => irMap.Remove(ir);

        private ZSObject LoadIRWithCaching(IRObject ir)
            => irMap.TryGetValue(ir, out var result)
            ? result
            : irMap[ir] = LoadIRWithoutCaching(ir);

        private ZSObject LoadIRWithoutCaching(IRObject ir)
            => ir switch
            {
                Function function => LoadIRInternal(function),
                Module module => LoadIRInternal(module),
                _ => throw new NotImplementedException()
            };
    }
}
