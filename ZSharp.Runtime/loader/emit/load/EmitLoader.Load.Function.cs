namespace ZSharp.Runtime.Loaders
{
    partial class EmitLoader
    {
        public IL.MethodInfo LoadStandaloneFunction(IR.Function function)
        {
            if (function.HasGenericParameters)
                return LoadStandaloneGenericFunction(function);
            return LoadStandaloneRegularFunction(function);
        }

        private IL.MethodInfo LoadStandaloneRegularFunction(IR.Function function)
        {
            var method = new Emit.DynamicMethod(
                function.Name ?? string.Empty,
                Runtime.ImportType(function.ReturnType),
                [
                    .. function.Signature
                    .GetParameters()
                    .Select(p => p.Type)
                    .Select(Runtime.ImportType)
                ],
                StandaloneModule
            );

            foreach (var (i, parameter) in function.Signature.GetParameters().Select((v, i) => (i, v)))
                method.DefineParameter(i + 1, IL.ParameterAttributes.None, parameter.Name);

            CompileFunctionCode(function, method.GetILGenerator());

            return method;
        }

        private IL.MethodInfo LoadStandaloneGenericFunction(IR.Function function)
        {
            var type = StandaloneModule.DefineType(
                $"<{function.Name ?? string.Empty}>{StandaloneModule.GetTypes().Length}",
                IL.TypeAttributes.Abstract | IL.TypeAttributes.Sealed
            );
            var method = type.DefineMethod(
                function.Name ?? "<AnonymousMethod>",
                IL.MethodAttributes.Public | IL.MethodAttributes.Static,
                Runtime.ImportType(function.ReturnType),
                [
                    .. function.Signature
                    .GetParameters()
                    .Select(p => p.Type)
                    .Select(Runtime.ImportType)
                ]
            );

            var genericParameters = method.DefineGenericParameters(
                [
                    .. function.GenericParameters
                    .Select(p => p.Name)
                ]
            );

            foreach (var (ir, il) in function.GenericParameters.Zip(genericParameters))
                Runtime.AddType(ir, il);

            foreach (var (i, parameter) in function.Signature.GetParameters().Select((v, i) => (i, v)))
                method.DefineParameter(i + 1, IL.ParameterAttributes.None, parameter.Name);

            CompileFunctionCode(function, method.GetILGenerator());

            foreach (var genericParameter in function.GenericParameters)
                Runtime.DelType(genericParameter);

            type.CreateType();

            return method;
        }

        private void CompileFunctionCode(IR.Function function, Emit.ILGenerator il)
        {
            var codeLoader = new CodeCompiler(FunctionCodeContext.From(Runtime, il, function));
            codeLoader.CompileCode(function.Body.Instructions);
        }
    }
}
