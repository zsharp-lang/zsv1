using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    /// <summary>
    /// Defines the base structure for a function.
    /// 
    /// A function must have a signature.
    /// The signature is used to be able to choose an overload at CG.
    /// 
    /// This is abstract because the behavior for calling a function is
    /// different for each type of function.
    /// </summary>
    public abstract class Function(string? name) 
        : CGObject
        , ICTCallable
    {
        public IR.Function? IR { get; set; }

        public string? Name { get; set; } = name;

        public CGObject? Body { get; set; }

        public abstract CGObject Call(ICompiler compiler, Argument[] arguments);

        public virtual Argument[]? Match(ICompiler compiler, Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);
            return Match(compiler, args, kwargs);
        }

        public abstract Argument[]? Match(ICompiler compiler, Args args, KwArgs kwArgs);
    }
}
