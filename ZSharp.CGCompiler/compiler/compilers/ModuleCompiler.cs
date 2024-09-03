using CommonZ.Utils;
using ZSharp.CGObjects;
using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal class ModuleCompiler(Context context)
        : ContextCompiler<RModule, Module>(context)
    {
        private readonly FunctionCompiler functionCompiler = new(context);

        protected internal override void AddCode(CGCode code)
            => Result.Content.AddRange(code);

        protected internal override CGObject? Compile(RDefinition definition)
            => base.Compile(definition) ?? definition switch
            {
                RFunction function => Compile(function),
                RLetDefinition let => Compile(let),
                //RVarDefinition var => Compile(var),
                //ROOPDefinition oop => Compile(oop),
                _ => null,
            };

        protected override Module Create()
            => new(Node.Name!);

        protected override void Define(Module @object)
        {
            if (@object.Name is not null && @object.Name != string.Empty)
                base.Define(@object);
        }

        protected override void Compile(Module module)
        {
            Emit([CG.Object(Result), CG.Enter()]);

            foreach (var statement in Node.Content?.Statements ?? Collection<RStatement>.Empty)
                Context.Compile(statement);

            Emit([CG.Leave()]);
        }

        private Function Compile(RFunction function)
            => functionCompiler.Compile(function);

        private Global Compile(RLetDefinition let)
        {
            var @global = new Global(let.Name ?? "??");

            if (let.Type is not null)
                @global.Type = Context.Compile(let.Type);

            global.Initializer = Context.Compile(let.Value);

            Emit([
                CG.Object(@global),
                CG.Compile(),
                ]);

            return @global;
        }

        private CGObject Compile(RVarDefinition var)
        {
            throw new NotImplementedException();
        }

        private CGObject Compile(ROOPDefinition oop)
        {
            throw new NotImplementedException();
        }
    }
}
