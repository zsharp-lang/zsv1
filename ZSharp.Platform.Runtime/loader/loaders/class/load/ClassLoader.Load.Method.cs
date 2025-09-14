namespace ZSharp.Platform.Runtime.Loaders
{
    partial class ClassLoader
    {
        private void LoadMethod(IR.Method method)
        {
            var attributes = IL.MethodAttributes.Public;

            if (method.IsStatic)
                attributes |= IL.MethodAttributes.Static;
            if (method.IsVirtual)
                attributes |= IL.MethodAttributes.Virtual | IL.MethodAttributes.NewSlot;

            var result = ILType.DefineMethod(
                method.Name ?? string.Empty,
                attributes,
                Loader.Runtime.ImportType(method.ReturnType),
                [.. (
                        method.IsInstance || method.IsVirtual
                        ? method.Signature.GetParameters().Skip(1)
                        : method.Signature.GetParameters()
                    ).Select(p => Loader.Runtime.ImportType(p.Type))
                ]
            );

            Loader.Runtime.AddFunction(method.UnderlyingFunction, result);

            var context = FunctionCodeContext.From(Loader.Runtime, result, method.UnderlyingFunction);

            foreach (var local in method.Body.Locals)
                result.GetILGenerator().DeclareLocal(Loader.Runtime.ImportType(local.Type));

            var codeLoader = new CodeCompiler(context);

            AddTask(() => codeLoader.CompileCode(method.Body.Instructions));
        }
    }
}
