namespace ZSharp.Runtime.Loaders
{
    partial class EnumClassLoader
    {
        public required TaskManager Tasks { get; init; }

        private void AddTask(Action task)
        {
            Tasks.AddTask(task);
        }
    }
}
