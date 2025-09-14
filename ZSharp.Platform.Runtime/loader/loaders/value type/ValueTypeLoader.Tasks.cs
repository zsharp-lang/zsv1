namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ValueTypeLoader
    {
        public required TaskManager Tasks { get; init; }

        private void AddTask(Action task)
        {
            if (genericTypeContext is not null)
            {
                var originalTask = task;
                task = () =>
                {
                    using (Loader.Runtime.TypeContext(genericTypeContext))
                        originalTask();
                };
            }

            Tasks.AddTask(task);
        }
    }
}
