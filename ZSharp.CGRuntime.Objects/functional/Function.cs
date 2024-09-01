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
        private CGCode? _body;

        public IR.Function? IR { get; set; }

        public string? Name { get; set; } = name;

        public CGCode Body
        {
            get
            {
                if (_body is not null)
                    return _body;

                Interlocked.CompareExchange(ref _body, [], null);
                return _body;
            }
        }

        public bool HasBody => _body is not null;

        public abstract CGObject Call(ICompiler compiler, CGRuntime.Argument[] arguments);

        public virtual CGRuntime.Argument[]? Match(ICompiler compiler, CGRuntime.Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);
            return Match(compiler, args, kwargs);
        }
    }
}
