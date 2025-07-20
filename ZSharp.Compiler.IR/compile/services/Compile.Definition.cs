namespace ZSharp.IRCompiler
{
    partial class Compile
    {
        public static Compiler.Result<T, Error> Definition<T>(CompilerObject @object, TargetPlatform? target)
            where T : IR.IRObject
        {
            throw new NotImplementedException();
        }

        public static void Definition<Owner>(CompilerObject @object, Owner owner, TargetPlatform? target)
            where Owner : IR.IRObject
        {
            throw new NotImplementedException();
        }
    }
}
