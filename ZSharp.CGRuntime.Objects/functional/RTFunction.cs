using ZSharp.Compiler;

namespace ZSharp.CGObjects
{
    public class RTFunction(string? name) : Function(name)
    {
        //public IR.Function? IR { get; set; }

        public Signature Signature { get; set; } = new();

        public CGCode? ReturnType { get; set; }

        public CGObject Call(IRCode[] arguments)
        {
            if (IR is null)
                throw new("Function not compiled.");

            IRCode code = [];

            foreach (var argument in arguments)
                code.AddRange(argument);

            code.Add(new IR.VM.Call(IR));

            throw new NotImplementedException();
            //return new Code(code);
        }
    }
}
