namespace ZSharp.Runtime.NET.IR2IL
{
    internal static class SignatureExtensions
    {
        public static IR.Parameter[] GetParameters(this IR.Signature signature)
            => [
                ..signature.Args.Parameters,
                ..(signature.IsVarArgs ? new IR.Parameter[] { signature.Args.Var! } : []),
                ..signature.KwArgs.Parameters,
                ..(signature.IsVarKwArgs ? new IR.Parameter[] { signature.KwArgs.Var! } : [])
            ];
    }
}
