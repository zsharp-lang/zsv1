using Scope = Core.Compiler.ModuleScope;

namespace ZSharp.Importer.ILLoader
{
    /// <summary>
    /// A module's own members: its functions and its <c>let</c>s. The module becomes an
    /// LIR module, its functions LIR functions and its <c>let</c>s LIR globals, and an
    /// initializer that gives the globals their values runs once it is loaded.
    /// </summary>
    partial class ILLoader
    {
        private readonly Dictionary<Package.DotNet.Module, Objects.DefinedModule> definedModules = [];
        private readonly Dictionary<Package.DotNet.Method, LIR.Function> moduleFunctions = [];
        private readonly Dictionary<Package.DotNet.Field, Objects.ModuleGlobal> moduleGlobals = [];

        private Error? DeclareMembers(Package.DotNet.Module node, LIR.Module module)
        {
            foreach (var field in node.Fields)
            {
                if (CompileType(field.Type).When(out var type).Error(out var error))
                    return error;

                var global = new LIR.Global(type!) { Name = field.Name };

                module.Globals.Add(global);
                moduleGlobals[field] = new Objects.ModuleGlobal(global, () => Scope.HIRLoader.LoadType(field.Type).Unwrap());
            }

            foreach (var method in node.Methods)
            {
                var signature = new LIR.Signature();

                if (Parameters(method.Parameters, signature) is { } error)
                    return error;

                if (method.ReturnType is not null)
                    if (CompileType(method.ReturnType).When(out var type).Error(out error))
                        return error;
                    else
                        signature.ReturnType = type;

                var function = new LIR.Function(signature) { Name = method.Name, Body = new() };

                module.Functions.Add(function);
                moduleFunctions[method] = function;
            }

            return null;
        }

        private Error? LowerMembers(Package.DotNet.Module node, LIR.Module module)
        {
            if (node.Methods.Count == 0 && node.Fields.Count == 0)
                return null;

            foreach (var method in node.Methods)
                if (method.Body is null)
                    return new ErrorMessage($"Function {method.Name} has no body.");
                else if (Lower(method.Body, moduleFunctions[method], method.ReturnType, method.Parameters) is { } error)
                    return error;

            var initializer = new LIR.Function(new LIR.Signature()) { Name = "initialize", Body = new() };

            foreach (var field in node.Fields)
                if (field.DefaultValue is not null && Initialize(field, initializer) is { } error)
                    return error;

            module.Functions.Add(initializer);

            return null;
        }

        /// <summary>
        /// Loads what the classes' code didn't already reach, and gives the module's
        /// variables their values.
        /// </summary>
        private Error? LoadMembers(Package.DotNet.Module node, LIR.Module module)
        {
            if (module.Functions.Count == 0 && module.Globals.Count == 0)
                return null;

            runtime.Load(module);
            runtime.GetMethod(module.Functions.Last()).Invoke(null, []);

            return null;
        }

        private Error? Initialize(Package.DotNet.Field field, LIR.Function initializer)
        {
            if (Scope.HIRCodeLoader.LoadExpression(field.DefaultValue!).When(out var value).Error(out var error))
                return error;

            if (Scope.CG.Set(moduleGlobals[field], value!).When(out var set).Error(out error))
                return error;

            if (Scope.IR.CompileStatement(set!, runtime).When(out var statement).Error(out error))
                return error;

            initializer.Body!.Block.Statements.Add(statement!);

            return null;
        }

        /// <summary>
        /// A function of a module that is defined, if the method is one.
        /// </summary>
        internal CompilerObject? ModuleFunction(Package.DotNet.Method node)
            => moduleFunctions.TryGetValue(node, out var function) ? new Objects.ModuleFunction(node, function) : null;

        /// <summary>
        /// A <c>let</c> of a module that is defined, if the field is one.
        /// </summary>
        internal CompilerObject? ModuleGlobal(Package.DotNet.Field node)
            => moduleGlobals.TryGetValue(node, out var global) ? global : null;
    }
}
