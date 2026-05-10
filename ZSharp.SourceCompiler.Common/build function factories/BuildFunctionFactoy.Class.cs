using ZSharp.SourceCompiler.Objects;

namespace ZSharp.SourceCompiler
{
    partial class BuildFunctionFactoy
    {
        public static IResult<object, Error> Class(AST.TypeDefinition node, LateConstructor lateConstructor)
        {
            var build = (Action<ClassSpecification> construct) =>
            {
                lateConstructor.ActualLateConstructor = () =>
                {

                };
            };

            return Result<object>.Ok(build);
        }
    }
}
