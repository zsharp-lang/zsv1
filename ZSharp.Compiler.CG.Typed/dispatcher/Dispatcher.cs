namespace ZSharp.Compiler.CGDispatchers.Typed
{
    public partial class Dispatcher(Compiler compiler)
    {
        private readonly Compiler compiler = compiler;
        private CG @base;

        public void Apply()
        {
            @base = compiler.CG;

            ref var cg = ref compiler.CG;

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
