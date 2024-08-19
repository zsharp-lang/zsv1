namespace ZSharp.Compiler
{
    public class RTFunction(string? name) : Function(name)
    {
        public IR.Function? IR { get; set; }

        public CGObject Call(IRCode[] arguments)
        {
            if (IR is null)
                throw new("Function not compiled.");

            IRCode code = [];

            foreach (var argument in arguments)
                code.AddRange(argument);

            code.Add(new IR.VM.Call(IR));

            return new Code(code);
        }
    }
}
