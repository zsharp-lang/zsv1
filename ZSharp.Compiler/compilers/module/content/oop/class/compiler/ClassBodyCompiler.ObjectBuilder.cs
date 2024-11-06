using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class ClassSpecCompiler
        : IObjectBuilder<NodeObject>
        , IObjectInitializer<NodeObject>
    {
        private readonly NonLinearContent objectBuilder;

        void IObjectInitializer<NodeObject>.Initialize(NodeObject @object)
            => Dispatcher(
                //Initialize,
                Initialize,
                Initialize
                //Initialize,
                )(@object);

        void IObjectBuilder<NodeObject>.Declare(NodeObject @object)
            => Dispatcher(
                //Declare,
                Declare,
                Declare
                //Declare,
                )(@object);

        void IObjectBuilder<NodeObject>.Define(NodeObject @object)
            => Dispatcher(
                //Define,
                Define,
                Define
                //Define,
                )(@object);

        private static Action<NodeObject> Dispatcher(
            //Action<Class, ROOPDefinition> classFn,
            Action<Method, RFunction> methodFn,
            Action<Field, RLetDefinition> letFn
            //Action<Global, RVarDefinition> varFn,
        )
            => @object =>
            {
                switch (@object.Object)
                {
                    //case Class @class: classFn(@class, (ROOPDefinition)@object.Node); break;
                    case Method method: methodFn(method, (RFunction)@object.Node); break;
                    case Field field
                        when @object.Node is RLetDefinition let:
                        letFn(field, (RLetDefinition)@object.Node); break;
                    //case Field field
                    //    when @object.Node is RVarDefinition var:
                    //    varFn(field, (RVarDefinition)@object.Node); break;
                    default: throw new NotImplementedException();
                }
            };

        private static void Unspecified<R, CG>(CG _, R __)
            where R : RDefinition
            where CG : CGObject
        { }
    }
}
