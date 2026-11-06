using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR class meta-compiler. It compiles a <c>class</c> to code that builds a
    /// <see cref="Package.DotNet.Class"/> as HIR, and registers each member in it:
    /// a <c>let</c> becomes a field.
    /// </summary>
    public sealed class ClassMetaType(
        ASTCompiler compiler,
        ILLoader il,
        ContainerCompiler container,
        Package.DotNet.TypeReference underlyingType
    )
    {
        public IResult<CompiledDefinition, Error> Compile(AST.Definition node, Code.Local self)
        {
            if (node is not AST.TypeDefinition definition)
                return Result<CompiledDefinition>.Error($"A {node.GetType().Name} is not a class.");

            return Result<CompiledDefinition>.Ok(new(
                self,
                new Code.Call
                {
                    Callee = new Code.Get(underlyingType),
                    Arguments = [new() { Value = new Code.StringLiteral(definition.Name) }]
                },
                underlyingType,
                () => Fill(definition, self)
            ));
        }

        private IResult<Code.Block, Error> Fill(AST.TypeDefinition definition, Code.Local self)
        {
            var filled = container.Compile(
                definition.Content?.Statements ?? [],
                nested => new Code.ExpressionStatement(Add(self, nameof(Package.DotNet.Class.NestedDefinitions), nested)),
                member => Member(member, self)
            );

            if (filled.When(out var block).Error(out var error))
                return Result<Code.Block>.Error(error);

            if (definition.Bases is null or [])
                return filled;

            if (definition.Bases is not [var @base])
                return Result<Code.Block>.Error($"Class {definition.Name} has interfaces, which aren't supported yet.");

            if (Described<Package.DotNet.ClassReference>(@base).When(out var type).Error(out error))
                return Result<Code.Block>.Error(error);

            block!.Statements.Insert(0, new Code.ExpressionStatement(new Code.Set
            {
                Target = new Code.NamedMember { Target = new Code.Get(self), Name = nameof(Package.DotNet.Class.Base) },
                Value = type!
            }));

            return filled;
        }

        private IResult<Code.Statement, Error>? Member(AST.Statement member, Code.Local self)
        {
            if (member is not AST.ExpressionStatement { Expression: AST.LetExpression let })
                return null;

            if (let.Type is null)
                return Result<Code.Statement>.Error($"Field {let.Name} needs a type.");

            if (let.Value is not null)
                return Result<Code.Statement>.Error($"Field {let.Name} has a value, which needs a constructor body.");

            if (Described<HIR.Type>(let.Type).When(out var type).Error(out var error))
                return Result<Code.Statement>.Error(error);

            return Result<Code.Statement>.Ok(new Code.ExpressionStatement(Add(
                self,
                nameof(Package.DotNet.Class.Fields),
                new Code.Call
                {
                    Callee = new Code.Get(il.Load(typeof(Package.DotNet.Field))),
                    Arguments = [
                        new() { Value = new Code.StringLiteral(let.Name) },
                        new() { Value = type! }
                    ]
                }
            )));
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

        private static Code.Call Add(Code.Local self, string list, Code.Expression value)
            => new()
            {
                Callee = new Code.NamedMember
                {
                    Target = new Code.NamedMember { Target = new Code.Get(self), Name = list },
                    Name = "Add"
                },
                Arguments = [new() { Value = value }]
            };
    }
}
