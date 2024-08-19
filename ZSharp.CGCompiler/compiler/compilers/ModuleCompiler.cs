using CommonZ.Utils;
using ZSharp.CGRuntime;
using ZSharp.RAST;

namespace ZSharp.CGCompiler
{
    internal class ModuleCompiler(Context context)
        : ContextCompiler<RModule, Module>(context)
    {
        protected internal override void AddCode(CGCode code)
            => Result.Content.AddRange(code);

        protected internal override object? Compile(RDefinition definition)
            => base.Compile(definition) ?? definition switch
            {
                //RLetDefinition let => Compile(let),
                //RVarDefinition var => Compile(var),
                //ROOPDefinition oop => Compile(oop),
                _ => null,
            };

        protected override Module Create()
        {
            Module module = new(Node.Name);

            if (module.Name is not null)
                Emit([
                    CG.Object(module),
                    CG.Set(module.Name)
                    ]);

            Emit([
                CG.Definition()
                ]);

            return module;
        }

        protected override void Compile(Module module)
        {
            foreach (var statement in Node.Content?.Statements ?? Collection<RStatement>.Empty)
            {
                Context.Compile(statement);
            }
        }

        private CGObject Compile(RLetDefinition let)
        {
            //throw new NotImplementedException();

            var @global = new Global(let.Name ?? "??");

            if (let.Type is not null)
                @global.Type = Context.Compile(let.Type);
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
