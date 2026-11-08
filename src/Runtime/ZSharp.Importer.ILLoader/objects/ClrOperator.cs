using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// The operators the CLR types of the operands define, found when called: the static
    /// methods of those types that have the operator's name, chosen between as overloads.
    /// </summary>
    internal sealed class ClrOperator(ILLoader loader, string name)
        : CompilerObject
        , ICTCallable
    {
        public IResult Call(Argument[] arguments)
        {
            var methods = arguments
                .Select(argument => Core.Compiler.ModuleScope.TS.TypeOf(argument.Object).Ok(out var type) && type.Is<IILType>(out var known) ? known.GetILType() : null)
                .OfType<Type>()
                .Distinct()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(method => method.Name == name))
                .Distinct()
                .Select(method => (CompilerObject)new Method(loader, method, null))
                .ToArray();

            return methods.Length == 0
                ? Result.Error(new UnsupportedError($"The operands define no {name}."))
                : Core.Compiler.ModuleScope.CG.Call(OverloadGroup.Of(methods), arguments);
        }
    }
}
