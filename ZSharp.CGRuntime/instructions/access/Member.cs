namespace ZSharp.CGRuntime.HLVM
{
    public sealed class Member<T>(T member, AccessMode accessMode) : Instruction
    {
        public T MemberPosition { get; set; } = member;

        public AccessMode AccessMode { get; set; } = accessMode;

        public static Member<T> Get(T member)
            => new(member, AccessMode.Get);

        public static Member<T> Set(T member)
            => new(member, AccessMode.Set);

        public static Member<T> Del(T member)
            => new(member, AccessMode.Del);

        public override string ToString()
            => MemberPosition is string name
            ? $"{AccessMode}Member(\"{MemberPosition}\")"
            : $"{AccessMode}Member({MemberPosition})"
            ;
    }
}
