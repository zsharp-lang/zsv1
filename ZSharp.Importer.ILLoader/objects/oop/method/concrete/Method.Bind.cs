namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Method
        : IBindable
    {
        IResult IBindable.Bind(Compiler.Compiler compiler, CompilerObject target)
        {
            if (IL.IsStatic) return Result.Ok(this);

            if (!compiler.TS.IsTyped(target, out var targetType))
                return Result.Error(
                        $"Cannot bind method {IL.Name} to untyped object {target}"
                    );

            return Result.Ok(new BoundMethod()
            {
                Method = this,
                Object = target
            });
        }
    }
}
