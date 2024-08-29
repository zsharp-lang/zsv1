using CommonZ.Utils;
using ZSharp.CGObjects;
using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal class ModuleCompiler(Context context)
        : ContextCompiler<RModule, Module>(context)
    {
        protected internal override void AddCode(CGCode code)
            => Result.Content.AddRange(code);

        protected internal override CGObject? Compile(RDefinition definition)
            => base.Compile(definition) ?? definition switch
            {
                RLetDefinition let => Compile(let),
                //RVarDefinition var => Compile(var),
                //ROOPDefinition oop => Compile(oop),
                _ => null,
            };

        protected override Module Create()
            => new(Node.Name);

        protected override void Define(Module module)
        {
            if (Context.CurrentContextObject is null)
                return;

            if (module.Name is not null && module.Name != string.Empty)
                Emit([
                    CG.Object(Result),
                    CG.Set(module.Name)
                    ]);

            Emit([
                CG.Object(module),
                CG.Compile()
                ]);
        }

        protected override void Compile(Module module)
        {
            Emit([CG.Object(Result), CG.Enter()]);

            foreach (var statement in Node.Content?.Statements ?? Collection<RStatement>.Empty)
            {
                Context.Compile(statement);
            }

            Emit([CG.Leave()]);
        }

        private Global Compile(RLetDefinition let)
        {
            var @global = new Global(let.Name ?? "??");

            if (let.Type is not null)
                @global.Type = Context.Compile(let.Type);

            global.Initializer = Context.Compile(let.Value);

            Emit([
                CG.Object(@global),
                CG.Dup(),

                CG.Set(global.Name),
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
