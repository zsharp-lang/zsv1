using CommonZ.Utils;
using ZSharp.CGObjects;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
        //: ICodeInjector
        //, ICompiler
    {
        public IRType EvaluateType(CGCode instructions)
            => EvaluateType(runtime.Run(instructions));

        public IRType EvaluateType(CGObject @object)
        {
            if (@object is RawType raw)
                return raw.AsType(this); // this is a placeholder for a compile-type protocol

            return EvaluateType(Read(@object));
        }

        public IRType EvaluateType(IEnumerable<CGObject> objects)
        {
            var objectList = objects.ToArray();
            if (objectList.Length == 1) return EvaluateType(objectList[0]);
            return EvaluateType(Read(objectList));
        }

        public IRType EvaluateType(Code code)
        {
            var value = EvaluateRT(code);

            // TODO: implement the `compile-type` protocol and use it here.
            // for now, we'll just return the type `string` for all values.

            return RuntimeModule.TypeSystem.String;
        }
    }
}
