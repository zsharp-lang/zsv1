using ZSharp.Compiler;
using ZSharp.IR;

using ConstructorIRResult = ZSharp.Compiler.Result<ZSharp.IR.Constructor>;

namespace Core.LanguageExtensions.Objects
{
    partial class Constructor
        : ICompileIRDefinitionAs<ZSharp.IR.Constructor>
        , ICompileIRDefinitionIn<ZSharp.IR.Class>
        , ICompileIRReference<ConstructorReference>
    {
        private ZSharp.IR.Constructor? IR { get; set; }

        private IResult<ZSharp.IR.Constructor, Error> GetIR(Compiler compiler, object? target)
        {
            if (IR is not null) return ConstructorIRResult.Ok(IR);

            if (
                compiler.IR.CompileDefinition<Function>(UnderlyingObject, target)
                .When(out var underlyingFunction)
                .Error(out var error)
            ) return ConstructorIRResult.Error(error);

            IR = new(Name)
            {
                Method = new(underlyingFunction!)
                {
                    Name = Name
                },
            };

            return ConstructorIRResult.Ok(IR);
        }

        IResult<ZSharp.IR.Constructor, Error> ICompileIRDefinitionAs<ZSharp.IR.Constructor>.CompileIRDefinition(Compiler compiler, object? target)
            => GetIR(compiler, target);

        void ICompileIRDefinitionIn<ZSharp.IR.Class>.CompileIRDefinition(Compiler compiler, ZSharp.IR.Class owner, object? target)
        {
            if (
                GetIR(compiler, target)
                .Ok(out var ir)
            ) owner.Constructors.Add(ir);
        }

        IResult<ConstructorReference, Error> ICompileIRReference<ConstructorReference>.CompileIRReference(Compiler compiler, object? target)
            => GetIR(compiler, target).When(ir => new ConstructorReference(ir) { OwningType = new ClassReference((ZSharp.IR.Class)IR.Method.Owner) });
    }
}
