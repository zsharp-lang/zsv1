namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ClassLoader
    {
        private void LoadConstructor(IR.Constructor constructor)
        {
            var result = ILType.DefineConstructor(
                IL.MethodAttributes.Public,
                IL.CallingConventions.HasThis,
                [.. constructor.Method.Signature.GetParameters().Skip(1).Select(p => Loader.Runtime.ImportType(p.Type))]
            );

            Loader.Runtime.AddFunction(constructor.Method.UnderlyingFunction, result);

            var context = FunctionCodeContext.From(Loader.Runtime, result, constructor.Method.UnderlyingFunction);

            var codeLoader = new CodeCompiler(context);

            AddTask(() => codeLoader.CompileCode(constructor.Method.Body.Instructions));
        }
    }
}
