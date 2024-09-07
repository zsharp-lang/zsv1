using CommonZ.Utils;
using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
        : IObjectBuilder<NodeObject>
    {
        private readonly Queue<NodeObject> dependencyCollectionQueue = [];
        private ObjectBuilder<NodeObject>? objectBuilder = null;

        private void EnqueueForDependencyCollection(CGObject @object, RDefinition node)
            => EnqueueForDependencyCollection(new NodeObject(node, @object));

        private void EnqueueForDependencyCollection(NodeObject @object)
        {
            dependencyCollectionQueue.Enqueue(@object);
            nodes.Cache(@object.Object, @object);
        }

        private void Build()
        {
            objectBuilder = new(this);

            NodeObject @object;

            while (dependencyCollectionQueue.Count > 0)
                using (CollectingDependenciesFor(@object = dependencyCollectionQueue.Dequeue()))
                    Initialize(@object);

            objectBuilder.BuildInOrder();

            objectBuilder = null;
        }

        private void Initialize(NodeObject @object)
            => Dispatcher(
                Initialize,
                Initialize,
                Initialize,
                Unspecified
                )(@object);

        void IObjectBuilder<NodeObject>.Declare(NodeObject @object)
            => Dispatcher(
                Declare,
                Declare,
                Declare,
                Unspecified
                )(@object);

        void IObjectBuilder<NodeObject>.Define(NodeObject @object)
            => Dispatcher(
                Define,
                Define,
                Define,
                Unspecified
                )(@object);

        private static Action<NodeObject> Dispatcher(
            Action<RTFunction, RFunction> functionFn,
            Action<Global, RLetDefinition> letFn,
            Action<Global, RVarDefinition> varFn,
            Action<Module, RModule> moduleFn
        )
            => @object =>
            {
                switch (@object.Object)
                {
                    case RTFunction function: functionFn(function, (RFunction)@object.Node); break;
                    case Global global
                        when @object.Node is RLetDefinition let:
                            letFn(global, (RLetDefinition)@object.Node); break;
                    case Global global
                        when @object.Node is RVarDefinition var:
                            varFn(global, (RVarDefinition)@object.Node); break;
                    case Module module: moduleFn(module, (RModule)@object.Node); break;
                    default: throw new NotImplementedException();
                }
            };

        private static void Unspecified<R, CG>(CG _, R __)
            where R : RDefinition
            where CG : CGObject
        { }
    }
}
