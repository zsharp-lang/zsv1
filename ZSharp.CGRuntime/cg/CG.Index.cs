namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Index GetIndex(int argumentCount)
            => HLVM.Index.Get(argumentCount);

        public static HLVM.Index SetIndex(int argumentCount)
            => HLVM.Index.Set(argumentCount);

        public static HLVM.Index DelIndex(int argumentCount)
            => HLVM.Index.Del(argumentCount);
    }
}
