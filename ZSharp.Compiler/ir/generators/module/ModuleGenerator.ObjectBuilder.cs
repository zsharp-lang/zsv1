using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleGenerator
        : IObjectBuilder
    {
        private Queue<CGObject> dependencyCollectionQueue = [];
        private ObjectBuilder objectBuilder;

        private void EnqueueForDependencyCollection(CGObject cgObject)
            => dependencyCollectionQueue.Enqueue(cgObject);

        private void Build()
        {
            objectBuilder = new(this); // TODO: move this to constructor and make field readonly

            CGObject dependent;

            while (dependencyCollectionQueue.Count > 0)
                using (CollectingDependenciesFor(dependent = dependencyCollectionQueue.Dequeue()))
                    Initialize(dependent);

            objectBuilder.BuildInOrder();
        }

        private void Initialize(CGObject @object)
            => Switch(
                Initialize,
                Unspecified,
                Initialize
            )(@object);

        void IObjectBuilder.Declare(CGObject @object)
            => Switch(
                Declare,
                Unspecified,
                Declare
            )(@object);

        void IObjectBuilder.Define(CGObject @object)
            => Switch(
                Define,
                Unspecified,
                Define
            )(@object);

        private static Action<CGObject> Switch(
            Action<Global> globalFn,
            Action<Module> moduleFn,
            Action<RTFunction> functionFn
        )
            => @object =>
            {
                switch (@object)
                {
                    case Global global: globalFn(global); break;
                    case Module module: moduleFn(module); break;
                    case RTFunction function: functionFn(function); break;
                    default: throw new NotImplementedException();
                }
            };

        private static void Unspecified<T>(T _) { }
    }
}
