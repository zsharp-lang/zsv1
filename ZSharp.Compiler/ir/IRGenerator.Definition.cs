using ZSharp.CGObjects;
using ZSharp.CGRuntime;

namespace ZSharp.Compiler
{
    internal partial class IRGenerator
		: IDefinitionHandler
    {
		public void Define(CGObject @object)
			=> Definition(@object);

		private IR.IRObject Definition(CGObject @object)
			=> @object switch
			{
				Global global => Definition(global),
				Module module => Definition(module),
				_ => throw new NotImplementedException(),
			};

		private IR.Global Definition(Global global)
		{
			if (CurrentContext is not Module module)
				throw new Exception("Global definition must be in a module context");

			var declaredType = global.Type is null ? null : EvaluateType(global.Type);

			var initializerCode = global.Initializer is null
				? null : Read(runtime.Run(global.Initializer));

			if (initializerCode is not null)
			{
				var initializerType = initializerCode.RequireValueType();

				declaredType ??= initializerType;

				var assignmentCode = AssignTo(initializerType, declaredType);

				if (assignmentCode is not null)
					initializerCode.Append(assignmentCode);
			}
			else if (declaredType is null) throw new Exception("Unknown type!");
			else initializerCode = Code.Empty;

            var ir = new IR.Global(global.Name, declaredType)
            {
                Initializer = [.. initializerCode.Instructions]
				// todo: ^^^ should the IR also contain information about stack size?
            };

            var initIR = module.InitFunction.IR!;
            initIR.Body.StackSize = Math.Max(initIR.Body.StackSize, initializerCode.MaxStackSize);
            initIR.Body.Instructions.AddRange([
				..initializerCode.Instructions,
				new IR.VM.SetGlobal(ir)
			]);

            module.IR!.Globals.Add(ir);

            return ir;
		}

		private IR.Module Definition(Module module)
		{
			Module? parent = CurrentContext as Module;

			if (CurrentContext is not null && parent is null)
				throw new Exception("Module definition must be in a module or a top level context");

			var ir = module.IR = new IR.Module(module.Name);
			
			ir.Functions.Add(module.InitFunction.IR = new(RuntimeModule.TypeSystem.Void));

			if (parent is not null)
				parent.IR!.Submodules.Add(ir);

			using (ContextOf(module))
				foreach (var contentObject in runtime.Run(module.Content))
					throw new Exception("top-level code must not generate RT code!");

			return ir;
		}
	}
}
