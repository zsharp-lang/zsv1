namespace ZSharp.Compiler.CGDispatchers.External
{
    /// <summary>
    /// Members an object has from outside, found when it has none of its own by the name.
    /// Its own members always come first, as instance methods do before extension methods
    /// in C#. What every provider gives is one overload group, so members of one name from
    /// different providers compete as overloads.
    /// </summary>
    /// <remarks>
    /// A dispatcher belongs to the whole toolchain, so what is added is seen by everything
    /// compiled after it, not only by a scope.
    /// </remarks>
    public sealed class Dispatcher(CG cg)
    {
        private readonly CG cg = cg;
        private readonly List<IExternalMembers> providers = [];
        private CG @base = null!;

        public void Apply()
        {
            @base = cg.Clone();

            cg.GetMemberByName = Member;
        }

        public void Add(IExternalMembers provider)
            => providers.Add(provider);

        public IResult Member(CompilerObject @object, MemberName name)
        {
            var result = @base.GetMemberByName(@object, name);

            if (!result.IsUnsupported())
                return result;

            var members = providers.SelectMany(provider => provider.Members(@object, name)).ToArray();

            return members.Length == 0 ? result : Result.Ok(OverloadGroup.Of(members));
        }
    }
}
