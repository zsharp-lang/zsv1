using LateConstructor = System.Action;

namespace ZSharp.SourceCompiler
{
    public struct TwoPassPrepared
    {
        internal List<(HIR.Node Node, LateConstructor LateConstructor)> Tasks { get; init; }

        public readonly List<HIR.Node> Perform()
        {
            Tasks.ForEach(task => task.LateConstructor());

            return [.. Tasks.Select(task => task.Node)];
        }
    }
}
