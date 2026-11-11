namespace ZSharp.Compiler
{
    public delegate IResult GetMember<T>(CompilerObject @object, T member);

    public delegate IResult SetMember<T>(CompilerObject @object, T member, CompilerObject value);

    partial class CG
    {
        public GetMember<MemberIndex> GetMemberByIndex { get; set; } = Dispatcher.Member;

        public SetMember<MemberIndex> SetMemberByIndex { get; set; } = Dispatcher.Member;

        public GetMember<MemberName> GetMemberByName { get; set; } = Dispatcher.Member;

        public SetMember<MemberName> SetMemberByName { get; set; } = Dispatcher.Member;

        /// <summary>
        /// The get member (.) operator.
        /// </summary>
        public IResult Member(CompilerObject @object, MemberIndex index)
            => GetMemberByIndex(@object, index);

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        public IResult Member(CompilerObject @object, MemberIndex index, CompilerObject value)
            => SetMemberByIndex(@object, index, value);

        /// <summary>
        /// The get member (.) operator.
        /// </summary>
        public IResult Member(CompilerObject @object, MemberName name)
            => GetMemberByName(@object, name);

        /// <summary>
        /// The set member (.=) operator.
        /// </summary>
        public IResult Member(CompilerObject @object, MemberName name, CompilerObject value)
            => SetMemberByName(@object, name, value);
    }
}
