using ZSharp.Compiler;
using ZSharp.IR;

using MethodIRResult = ZSharp.Compiler.Result<ZSharp.IR.Method>;

namespace Core.LanguageExtensions.Objects
{
    partial class Method
        : ICompileIRDefinitionAs<ZSharp.IR.Method>
        , ICompileIRDefinitionIn<ZSharp.IR.Class>
        , ICompileIRReference<MethodReference>
    {
        private ZSharp.IR.Method? IR { get; set; }

        private IResult<ZSharp.IR.Method, Error> GetIR(Compiler compiler, object? target)
        {
            if (IR is not null) return MethodIRResult.Ok(IR);

            if (
                compiler.IR.CompileDefinition<Function>(UnderlyingFunction, target)
                .When(out var underlyingFunction)
                .Error(out var error)
            ) return MethodIRResult.Error(error);

            IR = new(underlyingFunction!)
            {
                Name = Name
            };

            return MethodIRResult.Ok(IR);
        }

        IResult<ZSharp.IR.Method, Error> ICompileIRDefinitionAs<ZSharp.IR.Method>.CompileIRDefinition(Compiler compiler, object? target)
            => GetIR(compiler, target);

        void ICompileIRDefinitionIn<ZSharp.IR.Class>.CompileIRDefinition(Compiler compiler, ZSharp.IR.Class owner, object? target)
        {
            if (
                GetIR(compiler, target)
                .Ok(out var ir)
            ) owner.Methods.Add(ir);
        }

        IResult<MethodReference, Error> ICompileIRReference<MethodReference>.CompileIRReference(Compiler compiler, object? target)
            => GetIR(compiler, target).When(ir => new MethodReference(ir) { OwningType = new ClassReference((ZSharp.IR.Class)IR.Owner) });
    }
}
