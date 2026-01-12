namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ModuleLoader
    {
        public void LoadFunction(IR.Function function)
        {
            var returnType = Loader.Runtime.ImportType(function.ReturnType);
            var parameterTypes = function
                .Signature
                .GetParameters()
                .Select(p => p.Type)
                .Select(Loader.Runtime.ImportType)
                .ToArray();

            var method = Globals.DefineMethod(
                function.Name ?? "<AnonymousFunction>", 
                IL.MethodAttributes.Static | IL.MethodAttributes.Public,
                returnType,
                parameterTypes
            );

            Loader.Runtime.AddFunction(function, method);

            foreach (var (i, parameter) in function.Signature.GetParameters().Select((v, i) => (i, v)))
                method.DefineParameter(i + 1, IL.ParameterAttributes.None, parameter.Name);

            AddTask(() =>
            {
                var codeLoader = new CodeCompiler(FunctionCodeContext.From(Loader.Runtime, method, function));
                codeLoader.CompileCode(function.Body.Instructions);
            });
        }
    }
}
