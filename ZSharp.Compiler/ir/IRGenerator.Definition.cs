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

			//var type = global.Type; // TODO: Implement type generation
			var ir = new IR.Global(global.Name, null!);
			module.IR!.Globals.Add(ir);

			return ir;
		}

		private IR.Module Definition(Module module)
		{
			Module? parent = CurrentContext as Module;

			if (CurrentContext is not null && parent is null)
				throw new Exception("Module definition must be in a module or a top level context");

			var ir = new IR.Module(module.Name);

			if (parent is not null)
				parent.IR!.Submodules.Add(ir);

			IR.Function init = new((null as IR.IType)!);

			using (ContextOf(module))
				foreach (var contentObject in runtime.Run(module.Content))
					init.Body.Instructions.AddRange(Read(contentObject));

			return ir;
		}
	}
}
