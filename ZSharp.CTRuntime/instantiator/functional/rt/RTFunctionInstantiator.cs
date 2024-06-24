//using ZSharp.RAST;

//namespace ZSharp.CTRuntime
//{
//    internal sealed class RTFunctionInstantiator(Instantiator instantiator) 
//        : InstantiatorBase(instantiator)
//    {
//        public IR.Function? Function { get; private set; }

//        public RFunction? FunctionNode { get; private set; }

//        public IR.Function Instantiate(RFunction function)
//        {
//            IR.Function result = new()
//            {
//                Name = function.Name,
//            };

//            (function, FunctionNode) = (FunctionNode!, function);
//            (result, Function) = (Function!, result);

//            var unusedScope = Context.UseScope(FunctionNode);

//            //{
//            //    Instantiate(FunctionNode.Signature);

//            //    if (FunctionNode.Body is not null)
//            //        Instantiate(FunctionNode.Body);
//            //}

//            unusedScope();

//            FunctionNode = function;
//            (Function, result) = (result, Function);

//            return result;
//        }

//        private void Instantiate(RSignature signature)
//        {
//            if (signature.Args is not null)
//                foreach (var parameter in signature.Args)
//                    Function!.Signature.Args.Parameters.Add(Context.Scope.Cache(parameter.Id, Instantiate(parameter)).Parameter);

//            if (signature.VarArgs is not null)
//                Function!.Signature.Args.Var = Context.Scope.Cache(signature.VarArgs.Id, Instantiate(signature.VarArgs)).Parameter;

//            if (signature.KwArgs is not null)
//                foreach (var parameter in signature.KwArgs)
//                    Function!.Signature.KwArgs.Parameters.Add(parameter.Name!, Context.Scope.Cache(parameter.Id, Instantiate(parameter)).Parameter);

//            if (signature.VarKwArgs is not null)
//                Function!.Signature.KwArgs.Var = Context.Scope.Cache(signature.VarKwArgs.Id, Instantiate(signature.VarKwArgs)).Parameter;
//        }

//        private ParameterBinding Instantiate(RParameter parameter)
//        {
//            return new ParameterBinding(new(parameter.Name!, null), null);
//        }
//    }
//}
