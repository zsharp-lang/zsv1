namespace ZSharp.Runtime.NET.IR2IL
{
    internal sealed class RootModuleLoader(IRLoader loader, IR.Module @in)
    {
        public IRLoader Loader { get; } = loader;

        public IR.Module Input { get; } = @in;

        public IL.Emit.AssemblyBuilder AssemblyBuilder { get; } =
            IL.Emit.AssemblyBuilder.DefineDynamicAssembly(
                new(@in.Name ?? Constants.AnonymousModule),
                IL.Emit.AssemblyBuilderAccess.RunAndCollect
            );

        private List<Action> thisPass = [], nextPass = [], cleanUp = [];

        public void AddToNextPass(Action action)
            => nextPass.Add(action);

        public void AddToCleanUp(Action action)
            => cleanUp.Add(action);

        public IL.Module Load()
        {
            var result = new ModuleLoader(this, Input).Load();

            RunUntilComplete();

            return result;
        }

        private void RunUntilComplete()
        {
            while (nextPass.Count > 0)
            {
                (thisPass, nextPass) = (nextPass, thisPass);
                nextPass.Clear();

                foreach (var action in thisPass)
                    action();
            }

            foreach (var action in cleanUp)
                action();
        }
    }
}
