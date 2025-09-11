using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public sealed class Parameter(string name) 
        : CompilerObject
        , ICTReadable
        , ICompileIRObject<IR.Parameter, IR.Signature>
        , IReferencable<Parameter>
        , IParameter
    {
        public IR.Parameter? IR { get; set; }

        public string Name { get; } = name;

        public IType? Type { get; set; }

        public CompilerObject? Initializer { get; set; }

        public CompilerObject? Default
        {
            get => Initializer;
            set => Initializer = value;
        }

        public IR.Parameter CompileIRObject(Compiler.Compiler compiler, IR.Signature? owner)
        {
            if (Type is null)
                throw new("Parameter type not set.");

            IR ??= new(Name, compiler.RuntimeModule.TypeSystem.Void);
            if (IR.Type == compiler.RuntimeModule.TypeSystem.Void)
                IR.Type = compiler.CompileIRType(Type);

            return IR;
        }

        public IRCode Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetArgument(IR ?? compiler.CompileIRObject<IR.Parameter, IR.Signature>(this, null)),
            ])
            {
                MaxStackSize = 1,
                Types = [Type ?? throw new()]
            };

        Parameter IReferencable<Parameter>.CreateReference(Referencing @ref, ReferenceContext context)
        {
            return new(Name)
            {
                Initializer = Initializer is null ? null : @ref.CreateReference(Initializer, context),
                Type = Type is null ? null : @ref.CreateReference<IType>(Type, context),
            };
        }

        CompilerObjectResult IParameter.MatchArgument(Compiler.Compiler compiler, CompilerObject argument)
        {
            if (Type is null)
                throw new PartiallyCompiledObjectException(this, $"Parameter's {Name} type is not defined");

            return compiler.TypeSystem.ImplicitCast(argument, Type);
        }
    }
}
