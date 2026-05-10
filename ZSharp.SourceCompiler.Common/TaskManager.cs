namespace ZSharp.SourceCompiler
{
    public sealed class TaskManager(Action? init = null)
    {
        private List<Action> thisTasks = [init ?? (() => {})], nextTasks = [];

        public void AddTask(Action task)
            => nextTasks.Add(task);

        public void Reset(bool reInit = false)
        {
            thisTasks.Clear();
            nextTasks.Clear();
            thisTasks.Add(reInit && init is not null ? init : () => { });
        }

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
