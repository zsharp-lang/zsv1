using CommonZ;
using ZSharp.Compiler;
using ZSharp.Importer.ILLoader;
using ZSharp.Importer.String;
using ZSharp.Lang;
using ZSharp.Platform.Runtime;

using CompilerObject = ZSharp.MIR.CompilerObject;
using Scope = ZSharp.Compiler.Current;

namespace ZSharp.Toolchain
{
    /// <summary>
    /// The toolchain, as the CLI puts it together from Z#'s parts: the only place that
    /// combines them. Each step sets up one part, on top of the ones before it.
    /// </summary>
    public sealed class Toolchain
    {
        private readonly Runtime runtime = new();
        private readonly ILLoader il;
        private readonly MappedLoader loader = new(Scope.HIRLoader);
        private readonly MappedCodeLoader codeLoader = new(Scope.HIRCodeLoader);
        private readonly MappedReifier reifier = new(Scope.HIRReifier);
        private readonly HIRRunner runner = new();
        private readonly StringImporter importer = new();
        private readonly LibraryImporter std = new("standard");
        private readonly LibraryImporter core = new("core");
        private readonly ASTCompiler compiler = new();
        private readonly MappedASTCompiler mappedCompiler;
        private readonly Binder binder = new();
        private readonly MappedBinder mappedBinder;
        private readonly Language language;
        private Lang.DotNet.DotNetDocuments host = null!;
        private DocumentImporter documents = null!;

        public Toolchain(bool traceReification, bool dumpDefinitions)
        {
            il = new(runtime);
            mappedCompiler = new(compiler);
            mappedBinder = new(binder);
            language = new(binder);

            SetUpCompiler(traceReification);
            SetUpExecution();
            SetUpLibraries();
            SetUpLanguage(dumpDefinitions);
            SetUpDocuments();
        }

        /// <summary>
        /// Runs a document as the script, the first document.
        /// </summary>
        public Error? Run(string file)
            => documents.Run(file).When(out _).Error(out var failure) ? failure : null;

        /// <summary>
        /// The compiler's REIs, with the dispatchers, loaders and reifiers that implement
        /// them, and the runtime the IL loader imports from.
        /// </summary>
        private void SetUpCompiler(bool traceReification)
        {
            global::Core.Runtime.ModuleScope.Runtime = runtime;
            global::Core.Runtime.ModuleScope.ILLoader = il;

            Scope.MappedLoader = loader;
            Scope.MappedCodeLoader = codeLoader;
            loader.Apply();
            codeLoader.Apply();

            // Each dispatcher asks the ones applied before it first.
            new Compiler.CGDispatchers.Direct.Dispatcher(Scope.CG).Apply();
            new Compiler.CGDispatchers.Typed.Dispatcher(Scope.CG, Scope.TS).Apply();
            new Compiler.MatchingDispatchers.Static.Dispatcher(Scope.Matching, Scope.TS).Apply();

            // Members from outside an object's own, as extension methods, come after its own.
            var external = new Compiler.CGDispatchers.External.Dispatcher(Scope.CG, Scope.Overloading);
            external.Apply();
            external.Add(il.Extensions);

            new Compiler.TSDispatchers.Static.Dispatcher(Scope.TS).Apply();
            new Compiler.IRDispatchers.Static.Dispatcher(Scope.IR).Apply();
            new Compiler.OverloadingDispatchers.Static.Dispatcher(Scope.Overloading).Apply();

            CodeLoaders.Register(codeLoader, loader, Scope.HIRCodeLoader, Scope.HIRLoader, Scope.CG, Scope.Matching, Scope.Overloading);
            il.Install(loader, codeLoader);

            reifier.Apply();
            new CodeReifiers(Scope.HIRReifier, il).Register(reifier);

            if (traceReification)
                Tracing.TraceReification(Scope.HIRReifier);
        }

        /// <summary>
        /// Running code: HIR code runs on the runtime, compiled for it.
        /// </summary>
        private void SetUpExecution()
        {
            var mappedRunner = new MappedRunner(runner, Scope.HIRCodeLoader, Execute, Evaluate);
            mappedRunner.Apply();
            CodeRunners.Register(mappedRunner, runner);
        }

        private Error? Execute(CompilerObject @object)
        {
            if (Scope.IR.CompileStatement(@object, runtime).When(out var statement).Error(out var error))
                return error;

            runtime.Execute(statement!);

            return null;
        }

        private IResult<object?, Error> Evaluate(CompilerObject @object)
            => Scope.IR.CompileExpression(@object, runtime).When(out var expression).Error(out var error)
                ? Result<object?>.Error(error)
                : Result<object?>.Ok(runtime.Evaluate(expression!));

        /// <summary>
        /// The libraries the toolchain provides, `core:` and `std:`, by name.
        /// </summary>
        private void SetUpLibraries()
        {
            std.Add("io", il.Load(typeof(global::Standard.IO.ModuleScope).Module));

            core.Add("compiler", il.Load(typeof(global::Core.Compiler.ModuleScope).Module));
            core.Add("runtime", il.Load(typeof(global::Core.Runtime.ModuleScope).Module));
            core.Add("diagnostics", il.Load(typeof(Diagnostics.ModuleScope).Module));
            core.Add("zsharp", il.Load(typeof(global::Core.ZSharp.ModuleScope).Module));
            global::Core.ZSharp.ModuleScope.Libraries = core;

            foreach (var (name, module) in ToolchainModules.All())
                std.Add(name, il.Load(module));

            importer.Register("std", std);
            importer.Register("core", core);
        }

        /// <summary>
        /// The Z# language: its compiler, binder and meta-types, and the documents it runs.
        /// </summary>
        private void SetUpLanguage(bool dumpDefinitions)
        {
            mappedCompiler.Apply();
            mappedBinder.Apply();

            global::Core.ZSharp.ModuleScope.Compiler = compiler;
            global::Core.ZSharp.ModuleScope.Language = language;
            global::Core.ZSharp.ModuleScope.Runner = statement => runner.RunStatement(statement);
            global::Core.ZSharp.ModuleScope.Evaluator = expression => runner.EvaluateExpression(expression);
            global::Core.ZSharp.ModuleScope.Importer = importer.Import;
            DocumentCompiler.Install(loader, Scope.HIRLoader);
            language.Install(mappedCompiler, IsUnderlyingType);
            DocumentCompiler.Install(mappedBinder);

            // Operators and import are calls of functions the libraries provide.
            foreach (var library in new[] { typeof(global::Core.Runtime.ModuleScope), typeof(global::Core.ZSharp.ModuleScope) })
                foreach (var (syntax, function) in il.Operators(library))
                    language.Add(syntax, function);

            foreach (var (syntax, function) in il.ClrOperators())
                language.Add(syntax, function);

            foreach (var (syntax, function) in il.LiftedOperators(language.Functions))
                language.Add(syntax, function);

            // Each document runs with a frame holding its locals.
            host = new Lang.DotNet.DotNetDocuments(runtime, il, instance =>
            {
                if (dumpDefinitions)
                    Console.Error.WriteLine("definition:" + Environment.NewLine + Diagnostics.HIRPrinter.Print(instance) + Environment.NewLine);

                return Scope.HIRLoader.LoadInstance(instance).When(out _).Error(out var error) ? error : null;
            });
            host.Install(loader);
            host.Install(reifier);

            global::Core.ZSharp.ModuleScope.MetaTypes = Lang.DotNet.DotNetMetaTypes.Install(language, mappedCompiler, mappedBinder, runner, il, out var bodies);
            global::Core.ZSharp.ModuleScope.Bodies = bodies;
        }

        private static bool IsUnderlyingType(HIR.Code.Expression type, HIR.Instance underlying)
            => Scope.HIRCodeLoader.LoadExpression(type).When(out var target).IsOk
            && Scope.HIRLoader.LoadInstance(underlying).When(out var definition).IsOk
            && Scope.TS.IsAssignableTo(target!, definition!)
            && Scope.TS.IsAssignableTo(definition!, target!);

        /// <summary>
        /// What a source imports as: with no prefix it is a file, a <c>.zs</c> file a
        /// document and a <c>.dll</c> file an assembly, and the standard library written in
        /// Z# is found from where the toolchain runs.
        /// </summary>
        private void SetUpDocuments()
        {
            documents = new DocumentImporter(language, compiler, host);
            var files = new FileImporter(compiler);
            files.Register(".zs", documents.Import);
            importer.DefaultImporter = files;

            // An assembly is imported as its module. This is what the host loads, as what
            // the toolchain is built of has to be loaded before anything in Z# can be.
            var assemblies = new Assemblies(il, global::Core.ZSharp.ModuleScope.Describers) { ImportLibrary = source => importer.Import(source) };
            global::Core.ZSharp.ModuleScope.Enter = assemblies.Enter;
            files.Register(".dll", assemblies.ImportFile);
            global::Core.ZSharp.ModuleScope.Files = files;
            global::Core.ZSharp.ModuleScope.Importers = importer;
            global::Core.ZSharp.ModuleScope.Documents = documents;

            // A library that is built is its assembly, and one built library's assembly finds
            // another's.
            if (DocumentLibrary.Above(AppContext.BaseDirectory, "std") is { } standard)
            {
                var library = new DocumentLibrary(standard, documents, "std", path => Result<HIR.Instance>.Ok(assemblies.Import(Assemblies.Read(path))));
                std.Fallback = library;
                System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += (_, name) => library.Assembly(name.Name!) is { } path ? Assemblies.Read(path) : null;
            }
        }
    }
}
