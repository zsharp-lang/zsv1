using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;
using Compiled = IResult<ZSharp.Compiler.CompiledDefinition, CommonZ.Error>;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR class meta-type. It compiles a <c>class</c> to code that builds a
    /// <see cref="Package.DotNet.Class"/> as HIR, and registers each member in it:
    /// a <c>let</c> becomes a field.
    /// </summary>
    public sealed class ClassMetaType(
        Language language,
        ASTCompiler compiler,
        ILLoader il,
        Func<Code.Expression, HIR.Instance, bool> isUnderlyingType
    )
    {
        private readonly Package.DotNet.TypeReference underlyingType = il.Load(typeof(Package.DotNet.Class));

        public void Install(MappedASTCompiler mapped)
            => mapped.Definitions.Register<AST.TypeDefinition>(Compile);

        private Compiled Compile(AST.TypeDefinition definition)
        {
            if (definition.Of is not null)
            {
                if (compiler.CompileExpression(definition.Of).When(out var of).Error(out var error))
                    return Fail(error);

                if (!isUnderlyingType(of!, underlyingType))
                    return Fail($"Class {definition.Name} has a meta-type that isn't supported yet.");
            }

            var self = new Code.Local(definition.Name, underlyingType);
            var build = new Code.BlockExpression { Result = new Code.Get(self) };
            build.Locals.Add(self);

            build.Statements.Add(Statement(new Code.Set
            {
                Target = new Code.Get(self),
                Value = Construct(underlyingType, new Code.StringLiteral(definition.Name))
            }));

            var outer = language.Scope;
            language.Scope = new(outer);

            try
            {
                // A definition always binds its name inside itself.
                if (definition.Name.Length > 0)
                    language.Scope.Add(definition.Name, new Code.Get(self));

                if (Base(definition, self) is { } @base)
                    if (@base.When(out var statement).Error(out var error))
                        return Fail(error);
                    else
                        build.Statements.Add(statement!);

                foreach (var member in definition.Content?.Statements ?? [])
                    if (Member(member, self).When(out var compiled).Error(out var failure))
                        return Fail(failure);
                    else
                        build.Statements.Add(compiled!);
            }
            finally
            {
                language.Scope = outer;
            }

            return Result<CompiledDefinition>.Ok(new(build, underlyingType, definition.Name));
        }

        private IResult<Code.Statement, Error>? Base(AST.TypeDefinition definition, Code.Local self)
        {
            if (definition.Bases is null or [])
                return null;

            if (definition.Bases is not [var @base])
                return Result<Code.Statement>.Error($"Class {definition.Name} has interfaces, which aren't supported yet.");

            return Described<Package.DotNet.ClassReference>(@base).When(out var type).Error(out var error)
                ? Result<Code.Statement>.Error(error)
                : Result<Code.Statement>.Ok(Statement(new Code.Set
                {
                    Target = new Code.NamedMember { Target = new Code.Get(self), Name = nameof(Package.DotNet.Class.Base) },
                    Value = type!
                }));
        }

        private IResult<Code.Statement, Error> Member(AST.Statement member, Code.Local self)
        {
            if (member is not AST.ExpressionStatement { Expression: AST.LetExpression let })
                return Result<Code.Statement>.Error($"A {member.GetType().Name} can't be a class member yet.");

            if (let.Type is null)
                return Result<Code.Statement>.Error($"Field {let.Name} needs a type.");

            if (let.Value is not null)
                return Result<Code.Statement>.Error($"Field {let.Name} has a value, which needs a constructor body.");

            return Described<HIR.Type>(let.Type).When(out var type).Error(out var error)
                ? Result<Code.Statement>.Error(error)
                : Result<Code.Statement>.Ok(Statement(new Code.Call
                {
                    Callee = new Code.NamedMember
                    {
                        Target = new Code.NamedMember { Target = new Code.Get(self), Name = nameof(Package.DotNet.Class.Fields) },
                        Name = "Add"
                    },
                    Arguments = [new() { Value = Construct(il.Load(typeof(Package.DotNet.Field)), new Code.StringLiteral(let.Name), type!) }]
                }));
        }

        /// <summary>
        /// Compiles a type and casts it to what describes it, as in <c>I32 as Type</c>
        /// where <c>Type</c> is the HIR marker. An existing type is reflected, and a
        /// definition being built already is its description.
        /// </summary>
        private IResult<Code.Expression, Error> Described<T>(AST.Expression type)
            => compiler.CompileExpression(type).When(out var compiled).Error(out var error)
                ? Result<Code.Expression>.Error(error)
                : Result<Code.Expression>.Ok(new Code.Cast { Value = compiled!, Type = new Code.Get(il.Load(typeof(T))) });

        private static Code.Call Construct(HIR.Instance type, params Code.Expression[] arguments)
            => new()
            {
                Callee = new Code.Get(type),
                Arguments = [.. arguments.Select(argument => new Code.Argument { Value = argument })]
            };

        private static Code.ExpressionStatement Statement(Code.Expression expression) => new(expression);

        private static Compiled Fail(Error error) => Result<CompiledDefinition>.Error(error);

        private static Compiled Fail(string message) => Result<CompiledDefinition>.Error(message);
    }
}
