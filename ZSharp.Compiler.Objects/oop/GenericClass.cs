using CommonZ.Utils;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public class GenericClass
        : CompilerObject
        , ICTGetMember<MemberName>
        , IRTGetMember<MemberName>
        , IReferencable
        , ICompileIRObject<IR.Class, IR.Module>
        , IEvaluable
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Base = 0b1,
            Interfaces = 0b10,
            Body = 0b100,
            Owner = 0b1000,
        }
        private readonly ObjectBuildState<BuildState> state = new();

        public IR.ConstructedClass? IR { get; set; }

        public string Name { get; set; }

        public Collection<GenericParameter> GenericParameters { get; set; } = [];

        public GenericClassInstance? Base { get; set; }

        public Mapping<CompilerObject, Implementation> Implementations { get; set; } = [];

        public CompilerObject Constructor { get; set; }

        public Collection<CompilerObject> Content { get; } = [];

        public Mapping<MemberName, CompilerObject> Members { get; } = [];

        public CompilerObject Member(Compiler.Compiler compiler, MemberName member)
            => Members[member];

        public CompilerObject CreateReference(Compiler.Compiler compiler, ReferenceContext context)
        {
            int currentErrors = compiler.Log.Logs.Count(l => l.Level == LogLevel.Error);

            foreach (var genericParameter in GenericParameters)
                if (!context.CompileTimeValues.ContainsKey(genericParameter))
                    compiler.Log.Error(
                        $"Missing generic argument for parameter {genericParameter.Name} in type {this.Name}",
                        this
                    );

            if (compiler.Log.Logs.Count(l => l.Level == LogLevel.Error) > currentErrors)
                throw new(); // TODO: Huh???

            Mapping<GenericParameter, CompilerObject> genericArguments = [];

            foreach (var genericParameter in GenericParameters)
                genericArguments[genericParameter] = context.CompileTimeValues[genericParameter];

            return new GenericClassInstance(this, genericArguments)
            {
                Context = context,
            };
        }

        IR.Class ICompileIRObject<IR.Class, IR.Module>.CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR.Class @class = IR?.Class ?? new(Name);

            IR ??= new(@class);

            if (owner is not null && !state.Get(BuildState.Owner))
            {
                owner.Types.Add(@class!);

                state.Set(BuildState.Owner);
            }

            if (Base is not null && !state.Get(BuildState.Base))
            {
                state.Set(BuildState.Base);

                @class.Base = compiler.CompileIRType<IR.ConstructedClass>(Base);
            }

            if (Content.Count > 0 && !state.Get(BuildState.Body))
            {
                state.Set(BuildState.Body);

                foreach (var item in Content)
                    compiler.CompileIRObject(item, @class);
            }

            return @class;
        }

        CompilerObject IEvaluable.Evaluate(Compiler.Compiler compiler)
        {
            if (GenericParameters.Count != 0)
                return this;

            return new GenericClassInstance(this, []);
        }

        CompilerObject IRTGetMember<string>.Member(Compiler.Compiler compiler, CompilerObject value, string member)
        {
            var memberObject = compiler.Member(this, member);

            if (memberObject is IRTBoundMember boundMember)
                return boundMember.Bind(compiler, value);

            return memberObject;
        }
    }
}
