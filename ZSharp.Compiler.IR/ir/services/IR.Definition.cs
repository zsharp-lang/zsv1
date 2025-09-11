namespace ZSharp.Compiler
{
    partial class IR
    {
        public Result<T> CompileDefinition<T>(CompilerObject @object, TargetPlatform? target)
            where T : IRDefinition
        {
            if (@object.Is<ICompileIRDefinitionAs<T>>(out var compile))
                return compile.CompileIRDefinition(this, target);

            return Result<T>.Error(
                $"Cannot compile definition for object of type {@object}"
            );
        }

        public bool CompileDefinition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
            where Owner : IRDefinition
        {
            if (!@object.Is<ICompileIRDefinitionIn<Owner>>(out var compile))
                return false;


            compile.CompileIRDefinition(this, owner, target);
            return true;
        }
    }
}
