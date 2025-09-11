namespace ZSharp.Runtime.Loaders
{
    partial class ModuleLoader
    {
        private readonly TaskManager tasks;

        private void AddTask(Action task)
            => tasks.AddTask(task);

        private void RunUntilComplete()
            => tasks.RunUntilComplete();
    }
}
