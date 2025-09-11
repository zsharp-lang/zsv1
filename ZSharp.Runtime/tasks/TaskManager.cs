namespace ZSharp.Runtime
{
    internal sealed class TaskManager(Action init)
    {
        private List<Action> thisTasks = [init], nextTasks = [];

        public void AddTask(Action task)
            => nextTasks.Add(task);

        public void RunUntilComplete()
        {
            while (thisTasks.Count > 0)
            {
                foreach (var task in thisTasks) task();
                (thisTasks, nextTasks) = (nextTasks, thisTasks);
                nextTasks.Clear();
            }
        }
    }
}
