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

        //private void CompileWithClosure(CGObject @object)
        //{
        //    using (ClosureOf(@object))
        //        Compile(@object);
        //}

		//private IR.IRObject Compile(CGObject @object)
		//	=> @object switch
		//	{
  //              Function function => Compile(function),
  //              Global global => Compile(global),
		//		Module module => Compile(module),
		//		_ => throw new NotImplementedException(),
		//	};

		//private IR.Function Compile(Function function)
  //          => Compile(function,
  //              Initialize,
  //              Declare,
  //              Define,
  //              @object => @object.IR!
  //          );

  //      private IR.Global Compile(Global global)
  //          => Compile(global,
  //              Initialize,
  //              Declare,
  //              Define,
  //              @object => @object.IR!
  //          );

  //      private IR.Module Compile(Module module)
  //          => Compile(module,
  //              Initialize,
  //              Declare,
  //              Define,
  //              @object => @object.IR!
  //          );

  //      private TIR Compile<T, TIR>(
  //          T @object,
  //          Action<T> initialize,
  //          Action<T> declare,
  //          Action<T> define,
  //          Func<T, TIR> getIR
  //      )
  //          where T : CGObject
  //          where TIR : IR.IRObject
  //      {
  //          var objectState = GetObjectState(@object);

  //          if (objectState == ObjectState.Uninitialized)
  //              using (Initializing(@object))
  //                  initialize(@object);
  //          else if (objectState == ObjectState.Initialized)
  //              using (Declaring(@object))
  //                  declare(@object);
  //          else if (objectState == ObjectState.Declared)
  //              using (Defining(@object))
  //                  define(@object);

  //          return getIR(@object);
  //      }
    }
}
