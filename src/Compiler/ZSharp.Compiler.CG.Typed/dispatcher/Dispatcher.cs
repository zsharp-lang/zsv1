namespace ZSharp.Compiler.CGDispatchers.Typed
{
    public partial class Dispatcher(CG cg, TS ts)
    {
        private readonly CG cg = cg;
        private readonly TS ts = ts;
        private CG @base = null!;

        public void Apply()
        {
            @base = cg.Clone();

            cg.Call = Call;
            cg.Cast = Cast;
            cg.Get = Get;
            cg.GetIndex = Index;
            cg.GetMemberByIndex = Member;
            cg.GetMemberByName = Member;
            cg.ImplicitCast = ImplicitCast;
            cg.Match = Match;
            cg.Set = Set;
            cg.SetIndex = Index;
            cg.SetMemberByIndex = Member;
            cg.SetMemberByName = Member;
        }
    }
}
