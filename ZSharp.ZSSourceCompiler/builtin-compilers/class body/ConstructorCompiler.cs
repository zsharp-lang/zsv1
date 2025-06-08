using ZSharp.Compiler;

namespace ZSharp.ZSSourceCompiler
{
    public sealed class ConstructorCompiler(ClassBodyCompiler compiler, Constructor node)
        : ContextCompiler<Constructor, Objects.Constructor>(compiler.Compiler, node, new(node.Name))
        , IOverrideCompileExpression
    {
        public Objects.Parameter This { get; private set; } = null!;

        public override Objects.Constructor Compile()
        {
            using (Context.Compiler(this))
            using (Context.Scope(Object))
                CompileConstructor();

            return base.Compile();
        }

        private void CompileConstructor()
        {
            if (Node.Signature.Args is not null)
                foreach (var parameter in Node.Signature.Args)
                    Object.Signature.Args.Add(Compile(parameter));

            if (Node.Signature.VarArgs is not null)
                //Object.Signature.VarArgs = Compile(Node.Signature.VarArgs);
                throw new NotImplementedException();

            if (Node.Signature.KwArgs is not null)
                foreach (var parameter in Node.Signature.KwArgs)
                    Object.Signature.KwArgs.Add(Compile(parameter));

            if (Node.Signature.VarKwArgs is not null)
                //Object.Signature.VarKwArgs = Compile(Node.Signature.VarKwArgs);
                throw new NotImplementedException();

            if (Object.Signature.Args.Count == 0)
                Object.Signature.Args.Add(new("this"));

            (This = Object.Signature.Args[0]).Type ??= Object.Owner;

            compiler.AddToNextPass(() =>
             {
                 using (Context.Compiler(this))
                 using (Context.Scope(Object))
                 using (Context.Scope())
                     CompileConstructorBody();
             });
        }

        private void CompileConstructorBody()
        {
            if (Node.Body is not null)
                Object.Body = Compiler.CompileNode(Node.Body).Unwrap();

            Object.IR = Compiler.Compiler.CompileIRObject<IR.Constructor, IR.Class>(Object, null!);

            Object.IR.Method.Body.Instructions.Add(new IR.VM.Return());
        }

        private Objects.Parameter Compile(Parameter parameter)
        {
            var result = new Objects.Parameter(parameter.Name);

            Context.CurrentScope.Set(parameter.Alias ?? parameter.Name, result);

            if (parameter.Type is not null)
                result.Type = Compiler.CompileType(parameter.Type).Unwrap();

            if (parameter.Initializer is not null)
                Compiler.LogError("Parameter initializers are not supported yet.", parameter);

            return result;
        }

        private ObjectResult? Compile(IdentifierExpression identifier)
        {
            CompilerObject? member = null;

            Compiler.Compiler.CurrentContext.PerformOperation<ILookupContext>(
                scope =>
                {
                    return scope.Get(identifier.Name, out member);
                },
                new UntilContextTypeStrategy<IObjectContext<Objects.Class>>(
                    new ParentContextStrategy()
                )
            );

            if (member is null)
                return null;

            if (member is Objects.IRTBoundMember boundMember)
                return ObjectResult.Ok(boundMember.Bind(Compiler.Compiler, This));

            if (member is Objects.OverloadGroup group)
                return ObjectResult.Ok(new Objects.OverloadGroup(group.Name)
                {
                    Overloads = [.. group.Overloads.Select(
                        overload => overload is Objects.IRTBoundMember boundMember ?
                        boundMember.Bind(Compiler.Compiler, This) : overload
                    )],
                });

            return ObjectResult.Ok(member);
        }

        ObjectResult? IOverrideCompileNode<Expression>.CompileNode(ZSSourceCompiler compiler, Expression node)
            => node switch
            {
                IdentifierExpression identifier => Compile(identifier),
                _ => null,
            };
    }
}
