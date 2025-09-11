using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class ArrayLiteral(IEnumerable<CompilerObject>? items = null)
        : CompilerObject
        , ICTTypeCast
        , IImplicitCastToType
    {
        public List<CompilerObject> Items { get; } = new(items ?? []);

        CompilerObject ICTTypeCast.Cast(Compiler.Compiler compiler, IType targetType)
        {
            var instance = compiler.Call(targetType, []);

            var code = new IRCode(compiler.CompileIRCode(instance).Instructions);

            var append = compiler.Member(targetType, "append");

            foreach (var item in Items)
                code.Instructions.AddRange(
                    compiler.CompileIRCode(
                        compiler.Call(append, [
                            new(new RawCode(new([
                                    new IR.VM.Dup()
                                ])
                                {
                                    Types = [targetType]
                                })
                            ),
                            new(item)
                        ])
                    ).Instructions
                );

            code.Types.Clear();
            code.Types.Add(targetType);

            return new RawCode(code);
        }

        CompilerObject IImplicitCastToType.ImplicitCastToType(Compiler.Compiler compiler, IType type)
            => compiler.Cast(this, type);
    }
}
