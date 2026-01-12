namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ClassLoader
    {
        protected override void AddTask(Action task)
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

            base.AddTask(task);
        }
    }
}
