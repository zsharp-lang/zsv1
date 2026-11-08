namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The modules of the toolchain's types, by the names Z# imports them with from
    /// <c>std:</c>, under <c>zsharp/</c>: the standard API of the toolchain. A module's
    /// public types are its members, so Z# reaches them as it reaches any CLR type. What
    /// is live, bound to the toolchain that is running, is under <c>core:</c> instead.
    /// </summary>
    public static class ToolchainModules
    {
        public static IEnumerable<(string Name, System.Reflection.Module Module)> All()
            =>
            [
                ("zsharp/common", typeof(CommonZ.Error).Module),
                ("zsharp/hir", typeof(HIR.Instance).Module),
                ("zsharp/hir/code", typeof(HIR.Code.Code).Module),
                ("zsharp/mir", typeof(MIR.CompilerObject).Module),
                ("zsharp/lir", typeof(LIR.Function).Module),
                ("zsharp/lang", typeof(Language).Module),
                ("zsharp/lang/ast", typeof(AST.Node).Module),
                ("zsharp/lang/parser", typeof(Parser.Parser).Module),
                ("zsharp/compiler/ast", typeof(Compiler.ASTCompiler).Module),
                ("zsharp/compiler/cg", typeof(Compiler.CG).Module),
                ("zsharp/compiler/cg/direct", typeof(Compiler.ICTCallable).Module),
                ("zsharp/compiler/cg/typed", typeof(Compiler.IRTCallable).Module),
                ("zsharp/compiler/ir", typeof(Compiler.ICompileIRExpression).Module),
                ("zsharp/compiler/ts", typeof(Compiler.ITyped).Module),
                ("zsharp/compiler/code", typeof(Compiler.CodeLoaders).Module),
                ("zsharp/compiler/overloading", typeof(Compiler.OverloadGroup).Module),
                ("zsharp/package/dotnet", typeof(Package.DotNet.Class).Module),
            ];
    }
}
