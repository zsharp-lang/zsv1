namespace ZSharp.CGRuntime
{
    public static partial class CG
    {
        public static HLVM.Member<MemberName> GetMember(MemberName name)
            => HLVM.Member<MemberName>.Get(name);

        public static HLVM.Member<MemberName> SetMember(MemberName name)
            => HLVM.Member<MemberName>.Set(name);

        public static HLVM.Member<MemberIndex> GetMember(MemberIndex index)
            => HLVM.Member<MemberIndex>.Del(index);

        public static HLVM.Member<MemberIndex> SetMember(MemberIndex index)
            => HLVM.Member<MemberIndex>.Set(index);
    }
}
