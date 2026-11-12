namespace ZSharp.Toolchain
{
    /// <summary>
    /// The modules of the toolchain's types, by the names Z# imports them with. A module's
    /// public types are its members, so Z# reaches them as it reaches any CLR type.
    /// <list type="bullet">
    /// <item><see cref="Packages"/>, under <c>pkg:</c>: definitions with no state of their
    /// own, Z#'s layers and the CLR's, which any program can use.</item>
    /// <item><see cref="Core"/>, under <c>core:</c>: what is bound to the toolchain that is
    /// running, the compiler's REIs and the language's compiler.</item>
    /// </list>
    /// </summary>
    public static class ToolchainModules
    {
        public static IEnumerable<(string Name, System.Reflection.Module Module)> Packages()
            =>
            [
                ("zsharp/core", typeof(CommonZ.Error).Module),
                ("zsharp/hir", typeof(HIR.Instance).Module),
                ("zsharp/hir/code", typeof(HIR.Code.Code).Module),
                ("zsharp/mir", typeof(MIR.CompilerObject).Module),
                ("zsharp/lir", typeof(LIR.Function).Module),
                ("zsharp/lang/hir", typeof(Lang.Language).Module),
                ("zsharp/lang/ast", typeof(AST.Node).Module),
                ("zsharp/lang/parser", typeof(Parser.Parser).Module),
                ("zsharp/lang/syntax", typeof(Syntax.Grammar).Module),
                ("zsharp/lang/tokenizer", typeof(Tokenizer.Tokenizer).Module),
                ("clr/hir", typeof(Package.DotNet.Class).Module),
                ("clr/lir", typeof(LIR.CLR.ClrType).Module),
            ];

        public static IEnumerable<(string Name, System.Reflection.Module Module)> Core()
            =>
            [
                ("zsharp/h2m", typeof(Compiler.CodeLoaders).Module),
                ("zsharp/mir/core", typeof(Compiler.UnsupportedError).Module),
                ("zsharp/mir/cg", typeof(Compiler.CG).Module),
                ("zsharp/mir/cg/direct", typeof(Compiler.ICTCallable).Module),
                ("zsharp/mir/cg/typed", typeof(Compiler.IRTCallable).Module),
                ("zsharp/mir/ts", typeof(Compiler.ITyped).Module),
                ("zsharp/mir/overloading", typeof(Compiler.OverloadGroup).Module),
                ("zsharp/mir/matching", typeof(Compiler.Matching).Module),
                ("zsharp/mir/matching/static", typeof(Compiler.ICTBind).Module),
                ("zsharp/m2l", typeof(Compiler.ICompileIRExpression).Module),
                ("zsharp/lang", typeof(Compiler.ASTCompiler).Module),
            ];
    }
}
