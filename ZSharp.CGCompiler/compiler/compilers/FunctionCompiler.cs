using ZSharp.CGObjects;
using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal sealed class FunctionCompiler(Context context)
        : ContextCompiler<RFunction, RTFunction>(context)
    {
        protected internal override void AddCode(CGCode code)
            => Result.Body.AddRange(code);

        protected override RTFunction Create()
            => new(Node.Name);

        protected override void Compile(RTFunction @object)
        {
            if (Node.Signature.Args is not null)
                foreach (var arg in Node.Signature.Args)
                    Result.Signature.Args.Add(Compile(arg));

            if (Node.Signature.KwArgs is not null)
                foreach (var arg in Node.Signature.KwArgs)
                    Result.Signature.KwArgs.Add(Compile(arg));

            if (Node.Signature.VarArgs is not null)
                Result.Signature.VarArgs = Compile(Node.Signature.VarArgs);

            if (Node.Signature.VarKwArgs is not null)
                Result.Signature.VarKwArgs = Compile(Node.Signature.VarKwArgs);

            if (Node.ReturnType is not null)
                Result.ReturnType = Context.Compile(Node.ReturnType);

            if (Node.Body is not null)
                Context.Compile(Node.Body);
        }

        protected internal override bool Compile(RStatement statement)
        {
            if (statement is RReturn @return)
            {
                if (@return.Value is not null)
                    AddCode(Context.Compile(@return.Value));
                AddCode([
                    CG.Inject(() => new IR.VM.Return())
                    ]);
                return true;
            }
            return base.Compile(statement);
        }

        private Parameter Compile(RParameter parameter)
            => new(parameter.Name ?? throw new Exception())
            {
                Initializer = parameter.Default is null ? null : Context.Compile(parameter.Default),
                Type = parameter.Type is null ? null : Context.Compile(parameter.Type)
            };
    }
}
