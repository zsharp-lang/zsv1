namespace ZSharp.Interpreter
{
    partial class Interpreter
    {
        public Compiler.ILLoader.ILLoader ILLoader { get; }

        public CompilerObject ImportILModule(System.Reflection.Module module)
            => ILLoader.LoadModule(module);
    }
}
