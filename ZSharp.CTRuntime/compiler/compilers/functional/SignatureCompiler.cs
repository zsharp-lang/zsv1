using ZSharp.RAST;

namespace ZSharp.CTRuntime
{
    internal sealed class SignatureCompiler(ZSCompiler compiler)
        : CompilerBase(compiler)
    {
        private readonly CTCodeCompiler typeCompiler = new(compiler, new CTProcessor(compiler));

        public IR.Signature Compile(RSignature signature)
        {
            IR.Signature result = new(null!);

            if (signature.Args is not null)
                foreach (var arg in signature.Args)
                    result.Args.Parameters.Add(Compile(arg));

            return result;
        }

        private IR.Parameter Compile(RParameter parameter)
        {
            if (parameter.Type is null)
                throw new NotImplementedException();

            if (typeCompiler.Bind(parameter.Type) is not Class @class)
                throw new();

            var result = new IR.Parameter(parameter.Name!, @class.IR);

            Context.SetRT(parameter.Id, new Parameter(result, (@class as ICTBinding).Read(Compiler)));

            return result;
        }
    }
}
