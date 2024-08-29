namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
    {
        private Queue<CGObject> dependencyCollectionQueue = [];

        private void EnqueueForDependencyCollection(CGObject cgObject)
            => dependencyCollectionQueue.Enqueue(cgObject);

        private void Build()
        {

        }
    }
}
