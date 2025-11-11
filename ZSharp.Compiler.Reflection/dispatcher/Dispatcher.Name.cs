namespace ZSharp.Compiler
{
    partial class Dispatcher
    {
        public static string GetName(CompilerObject @object)
            => @object.Is<IHasName>(out var hasName)
                ? hasName.Name
                : string.Empty;
    }
}
