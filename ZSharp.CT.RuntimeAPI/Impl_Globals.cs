using ZSharp.Runtime.NET.IL2IR;

namespace ZS.RuntimeAPI
{
    [ModuleGlobals]
    public static class Impl_Globals
    {
        public static ZSharp.Objects.CompilerObject InfoOf(object obj)
        {
            if (obj is ZSharp.Runtime.NET.ICompileTime ct)
                return ct.GetCO();

            throw new();
        }

        public static object GetObject(Type type)
            => Fields_Globals.runtime.GetObject(type);
    }
}
