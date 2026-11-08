using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The CLR class meta-compiler. It compiles a <c>class</c> to code that builds a
    /// <see cref="Package.DotNet.Class"/> as HIR, and registers each member in it:
    /// a <c>let</c> becomes a field, and the rest of its members are definitions.
    /// </summary>
    public sealed class ClassMetaType(
        ILLoader il,
        ContainerCompiler container,
        CodeBody body,
        Package.DotNet.TypeReference underlyingType
    )
    {
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
            var filled = container.Compile(
                compiler,
                definition.Content?.Statements ?? [],
                (member, value) => new Code.ExpressionStatement(CodeBody.Add(self, ListOf(member), value)),
                member => Member(compiler, member, self)
            );

            if (filled.When(out var block).Error(out var error))
                return Result<Code.Block>.Error(error);

            if (definition.Bases is null or [])
                return filled;

            if (definition.Bases is not [var @base])
                return Result<Code.Block>.Error($"Class {definition.Name} has interfaces, which aren't supported yet.");

            if (body.Described<Package.DotNet.ClassReference>(compiler, @base).When(out var type).Error(out error))
                return Result<Code.Block>.Error(error);

            block!.Statements.Insert(0, new Code.ExpressionStatement(new Code.Set
            {
                Target = CodeBody.Member(self, nameof(Package.DotNet.Class.Base)),
                Value = type!
            }));

            return filled;
        }

        private string ListOf(CompiledDefinition member)
        {
            if (ReferenceEquals(member.UnderlyingType, il.Load(typeof(Package.DotNet.Method))))
                return nameof(Package.DotNet.Class.Methods);

            return ReferenceEquals(member.UnderlyingType, il.Load(typeof(Package.DotNet.Constructor)))
                ? nameof(Package.DotNet.Class.Constructors)
                : nameof(Package.DotNet.Class.NestedDefinitions);
        }

        private IResult<Code.Statement, Error>? Member(ASTCompiler compiler, AST.Statement member, Code.Local self)
        {
            if (member is not AST.ExpressionStatement { Expression: AST.LetExpression let })
                return null;

            if (let.Type is null)
                return Result<Code.Statement>.Error($"Field {let.Name} needs a type.");

            if (body.Described<HIR.Type>(compiler, let.Type).When(out var type).Error(out var error))
                return Result<Code.Statement>.Error(error);

            var fieldType = il.Load(typeof(Package.DotNet.Field));
            var field = new Code.Local(let.Name, fieldType);
            var block = new Code.Block();

            block.Locals.Add(field);
            block.Statements.Add(CodeBody.Assign(field, CodeBody.Construct(fieldType, new Code.StringLiteral(let.Name), type!)));

            if (let.Value is not null)
            {
                if (body.Reified(compiler, block, let.Value).When(out var value).Error(out error))
                    return Result<Code.Statement>.Error(error);

                block.Statements.Add(new Code.ExpressionStatement(new Code.Set
                {
                    Target = CodeBody.Member(field, nameof(Package.DotNet.Field.DefaultValue)),
                    Value = value!
                }));
            }

            block.Statements.Add(new Code.ExpressionStatement(
                CodeBody.Add(self, nameof(Package.DotNet.Class.Fields), new Code.Get(field))
            ));

            return Result<Code.Statement>.Ok(block);
        }
    }
}
