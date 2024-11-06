using ZSharp.CGObjects;
using ZSharp.RAST;

namespace ZSharp.Compiler
{
    internal sealed partial class MethodBodyCompiler
        : IStatementCompiler
    {
        CGObject? IStatementCompiler.Compile(RStatement statement)
            => statement switch
            {
                RReturn @return => Compile(@return),
                _ => null
            };

        private CGObject Compile(RReturn @return)
        {
            CGObject? value = 
                @return.Value is null ? null : Compiler.CompileNode(@return.Value);

            var code = value is null ? null : Compiler.CompileIRCode(value);

            return new RawCode(new([
                ..code?.Instructions ?? [],
                new IR.VM.Return(),
                ])
            {
                MaxStackSize = code?.MaxStackSize ?? 0,
            });
        }
    }
}
