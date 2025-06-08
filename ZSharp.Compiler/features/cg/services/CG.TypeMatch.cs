namespace ZSharp.Compiler
{
    public partial struct CG
    {
        public Result<TypeMatch, Error> TypeMatch(CompilerObject @object, IType type)
        {
            var result = Result<TypeMatch, Error>.Error(
                "Object cannot be type matched"
            );

            if (@object is ICTTypeMatch ctTypeMatch)
                result = ctTypeMatch.Match(compiler, type);

            if (result.IsOk) return result;

            if (
                RuntimeDescriptor(@object, out var runtimeDescriptor)
                && runtimeDescriptor is IRTTypeMatch rtTypeMatch
            )
                result = rtTypeMatch.Match(compiler, @object, type);

            if (result.IsOk) return result;

            return result;
        }
    }
}
