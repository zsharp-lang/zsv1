using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    internal sealed class Class(ILLoader loader, Type il, LIR.TypeHandle handle)
        : CompilerObject
        , ICompileIRType
        , ICTGetMember<MemberName>
    {
        public IResult<LIR.Type, Error> CompileIRType(object? target)
            => Result<LIR.Type>.Ok(handle);

        public IResult Member(MemberName member)
        {
            var candidates = il
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => (method.GetCustomAttribute<AliasAttribute>()?.Name ?? method.Name) == member)
                .ToArray();

            return candidates switch
            {
                [] => Result.Error(new UnsupportedError($"{il.Name} has no static method named {member}.")),
                [var method] => loader.LoadMethod(loader.Load(method)),
                _ => Result.Error($"{il.Name}.{member} is overloaded, which is not supported yet.")
            };
        }
    }
}
