//using ZSharp.RAST;

//namespace ZSharp.CTRuntime
//{
//    internal class GenericBinder<T>(CodeCompiler<T> compiler) : IBinder<T>
//    {
//        public CodeCompiler<T> Compiler { get; } = compiler;

//        public DomainContext<T> Context => Compiler.Context;

//        public IProcessor<T> Processor => Compiler.Processor;

//        public IBinding<T> Bind(RExpression expression)
//        {
//            return expression switch
//            {
//                RCall call => Bind(call),
//                RDefinition definition => Compiler.Bind(definition),
//                RId id => Bind(id),
//                RLiteral literal => Bind(literal),
//                _ => throw new NotImplementedException(),
//            };
//        }

//        private IBinding<T> Bind(RCall call)
//        {
//            var callee = Bind(call.Callee);

//            var args = call.Arguments.Select(arg => new Argument<T>(arg.Name, Bind(arg.Value))).ToArray();

//            return Processor.Call(callee, args);
//        }

//        private IBinding<T> Bind(RId id) 
//            => Context.Scope.Cache(id) ?? throw new Exception();

//        private IBinding<T> Bind(RLiteral literal)
//            => Processor.Literal(
//                literal.Value, 
//                literal.Type, 
//                literal.UnitType is null ? null : Bind(literal.UnitType));
//    }
//}
