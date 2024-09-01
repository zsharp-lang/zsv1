using ZSharp.CGObjects;
using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
		: ICompileHandler
    {
        private ICompileHandler compileHandler;

		public void CompileObject(CGObject @object)
		    => compileHandler.CompileObject(@object);
    }
}
