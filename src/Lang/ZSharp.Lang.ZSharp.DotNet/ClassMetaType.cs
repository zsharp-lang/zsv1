using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The meta-compiler of <c>DotNet_Class</c>, a class of the CLR's defined in Z#. A
    /// definition of it compiles to code that, run where the definition is, builds the
    /// class's HIR, a <see cref="Package.DotNet.Class"/>: its type parameters first, its
    /// bases, then each member, a <c>let</c> as a field, a <c>fun</c> as a method, a
    /// constructor as one, and any other definition as a nested one, and last a property
    /// for each name its accessors have, and the named constructors, in the CLR's terms.
    /// Only a type definition is one: a function can be given a meta-type too, so what
    /// isn't a type definition fails.
    /// </summary>
    public sealed class ClassMetaType(
        Language language,
        ILLoader il,
        ContainerCompiler container,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
        private readonly NamedConstructors namedConstructors = new(il);

        public IResult<CompiledDefinition, Error> Compile(ASTCompiler compiler, AST.Definition node, Code.Local self)
        {
            if (node is not AST.TypeDefinition definition)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a class.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                CodeBody.Construct(underlyingType, new Code.StringLiteral(definition.Name)),
                underlyingType,
                () => Fill(compiler, definition, self)
            ));
        }

        private IResult<Code.Block, Error> Fill(ASTCompiler compiler, AST.TypeDefinition definition, Code.Local self)
        {
            // A class's type parameters are bound before its members are compiled, which use
            // them, and built before anything else.
            if (body.TypeParameters(compiler, definition.GenericParameters, self, nameof(Package.DotNet.Class.GenericParameters)).When(out var parameters).Error(out var invalid))
                return Result<Code.Block>.Error(invalid);

            // In a class, a let is a field and a fun a method.
            var fork = body.WithFields(body.WithMethods(compiler), self, nameof(Package.DotNet.Class.Fields), bind: false);

            // A class is private, and so are its members unless they say otherwise.
            var filled = MemberDefaults.Within(compiler, Package.DotNet.Accessibility.Private, Package.DotNet.Accessibility.Private, () => container.Compile(
                fork,
                definition.Content?.Statements ?? [],
                (member, value) => new Code.ExpressionStatement(body.Register(self, ListOf(member), value))
            ));

            if (filled.When(out var block).Error(out var error))
                return Result<Code.Block>.Error(error);

            block!.Statements.InsertRange(0, parameters!);
            block.Statements.Insert(0, body.Accessible(self, Package.DotNet.Accessibility.Private));

            // A property is defined once its accessors are, as they are what it is.
            foreach (var property in (definition.Content?.Statements ?? [])
                .Select(statement => Language.Undecorate(statement, out _))
                .OfType<AST.DefinitionStatement>()
                .Select(statement => statement.Definition)
                .OfType<AST.Accessor>()
                .Select(accessor => accessor.Name)
                .Distinct())
                block.Statements.Add(new Code.ExpressionStatement(new Code.Call
                {
                    Callee = new Code.NamedMember { Target = new Code.Get(il.Load(typeof(Properties))), Name = nameof(Properties.Define) },
                    Arguments = [new() { Value = new Code.Get(self) }, new() { Value = new Code.StringLiteral(property) }]
                }));

            // A named constructor is made in the CLR's terms once its decorators are done.
            block.Statements.Add(new Code.ExpressionStatement(new Code.Call
            {
                Callee = new Code.NamedMember { Target = new Code.Get(il.Expose(namedConstructors)), Name = nameof(NamedConstructors.Define) },
                Arguments = [new() { Value = new Code.Get(self) }]
            }));

            return Bases(compiler, definition, self, block!) is { } failed ? Result<Code.Block>.Error(failed) : filled;
        }


        /// <summary>
        /// Adds code that gives the definition its bases, before anything else, as they are
        /// only known once they are built.
        /// </summary>
        public Error? Bases(ASTCompiler compiler, AST.TypeDefinition definition, Code.Local self, Code.Block block)
        {
            var index = 0;

            foreach (var @base in definition.Bases ?? [])
                if (compiler.CompileExpression(@base).When(out var type).Error(out var error))
                    return error;
                else
                    block.Statements.Insert(index++, new Code.ExpressionStatement(new Code.Call
                    {
                        Callee = new Code.NamedMember { Target = new Code.Get(il.Load(typeof(Inheritance))), Name = nameof(Inheritance.Inherit) },
                        Arguments = [new() { Value = new Code.Get(self) }, new() { Value = type! }]
                    }));

            return null;
        }

        private string ListOf(CompiledDefinition member)
        {
            if (ReferenceEquals(member.UnderlyingType, il.Load(typeof(Package.DotNet.Method))))
                return nameof(Package.DotNet.Class.Methods);

            return ReferenceEquals(member.UnderlyingType, il.Load(typeof(Package.DotNet.Constructor)))
                ? nameof(Package.DotNet.Class.Constructors)
                : nameof(Package.DotNet.Class.NestedDefinitions);
        }
    }
}
