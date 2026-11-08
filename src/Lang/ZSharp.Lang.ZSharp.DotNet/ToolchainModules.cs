namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// The modules of the toolchain Z# runs on, by the names Z# imports them with from
    /// <c>core:</c>. A module's public types are its members, so Z# reaches the toolchain's
    /// own types, as HIR nodes and CG protocols, as it reaches any CLR type.
    /// </summary>
    public static class ToolchainModules
    {
        public static IEnumerable<(string Name, System.Reflection.Module Module)> All()
            =>
            [
                ("common", typeof(CommonZ.Error).Module),
                ("hir", typeof(HIR.Instance).Module),
                ("hir/code", typeof(HIR.Code.Code).Module),
                ("mir", typeof(MIR.CompilerObject).Module),
                ("lir", typeof(LIR.Function).Module),
                ("ast", typeof(AST.Node).Module),
                ("compiler/cg", typeof(Compiler.CG).Module),
                ("compiler/cg/direct", typeof(Compiler.ICTCallable).Module),
                ("compiler/cg/typed", typeof(Compiler.IRTCallable).Module),
                ("compiler/ir", typeof(Compiler.ICompileIRExpression).Module),
                ("compiler/overloading", typeof(Compiler.OverloadGroup).Module),
                ("compiler/ast", typeof(Compiler.ASTCompiler).Module),
                ("lang", typeof(Language).Module),
                ("compiler/ts", typeof(Compiler.ITyped).Module),
                ("compiler/code", typeof(Compiler.CodeLoaders).Module),
                ("dotnet", typeof(Package.DotNet.Class).Module),
            ];
    }
}
