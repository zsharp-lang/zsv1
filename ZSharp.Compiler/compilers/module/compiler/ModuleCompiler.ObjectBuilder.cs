using CommonZ.Utils;
using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ModuleCompiler
        : IObjectBuilder<NodeObject>
        , IObjectInitializer<NodeObject>
    {
        private readonly NonLinearContent objectBuilder;

        void IObjectInitializer<NodeObject>.Initialize(NodeObject @object)
            => Dispatcher(
                Initialize,
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
                Declare,
                Unspecified
                )(@object);

        void IObjectBuilder<NodeObject>.Define(NodeObject @object)
            => Dispatcher(
                Define,
                Define,
                Define,
                Define,
                Unspecified
                )(@object);

        private static Action<NodeObject> Dispatcher(
            Action<ModuleOOPObject, ROOPDefinition> oopFn,
            Action<RTFunction, RFunction> functionFn,
            Action<Global, RLetDefinition> letFn,
            Action<Global, RVarDefinition> varFn,
            Action<Module, RModule> moduleFn
        )
            => @object =>
            {
                switch (@object.Object)
                {
                    case ModuleOOPObject oop: oopFn(oop, (ROOPDefinition)@object.Node); break;
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
