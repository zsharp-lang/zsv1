namespace ZSharp.SourceCompiler.Script
{
    public static class Compiler
    {
        public static IResult Compile(AST.Document document)
            => Compilers.Compile(document);
    }
}
