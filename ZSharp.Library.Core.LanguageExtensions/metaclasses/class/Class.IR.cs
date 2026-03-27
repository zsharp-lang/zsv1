using ZSharp.Compiler;
using ZSharp.IR;

using IRClassResult = ZSharp.Compiler.Result<ZSharp.IR.Class>;


namespace Core.LanguageExtensions.Objects
{
    partial class Class
        : ICompileIRDefinitionAs<ZSharp.IR.Class>
        , ICompileIRDefinitionIn<Module>
        , ICompileIRType<TypeReference<ZSharp.IR.Class>>
    {
        private ZSharp.IR.Class? IR { get; set; }

        private IRClassResult GetIR(Compiler compiler, object? target)
        {
            if (IR is not null) return IRClassResult.Ok(IR);

            IR = new(Name);

            AggregateError errors = new();

            if (Base is not null)
                if (
                    compiler.IR.CompileTypeAs<TypeReference<ZSharp.IR.Class>>(Base, target)
                    .When(out var @base)
                    .Error(out var error)
                ) errors.Append(error);
                else IR.Base = @base;
            //else if (target is IOOPRuntime runtime)
            //    IR.Base = new ClassReference(runtime.ObjectHierarchyRoot);

            foreach (var item in MembersByOrder)
            {
                if (
                    !compiler.IR.CompileDefinition(item, IR, target)
                ) errors.Append($"Could not compile member {item}");
            }

            if (errors.HasErrors)
                return IRClassResult.Error(errors);

            return IRClassResult.Ok(IR);
        }

        IResult<ZSharp.IR.Class, Error> ICompileIRDefinitionAs<ZSharp.IR.Class>.CompileIRDefinition(Compiler compiler, object? target)
            => GetIR(compiler, target);

        void ICompileIRDefinitionIn<Module>.CompileIRDefinition(Compiler compiler, Module owner, object? target)
        {
            var result = GetIR(compiler, target);

            if (result.Ok(out var ir))
                owner.Types.Add(ir);
        }

        IResult<TypeReference<ZSharp.IR.Class>, Error> ICompileIRType<TypeReference<ZSharp.IR.Class>>.CompileIRType(Compiler compiler, object? target)
        {
            if (
                GetIR(compiler, target)
                .When(out var ir)
                .Error(out var error)
            ) return Result<ClassReference>.Error(error);

            return Result<ClassReference>.Ok(
                new ClassReference(ir!)    
            );
        }
    }
}
