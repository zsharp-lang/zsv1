using ZSharp.Importer.ILLoader;

using Code = ZSharp.HIR.Code;

namespace ZSharp.Lang.DotNet
{
    /// <summary>
    /// A CLR class's named constructors, in the CLR's terms, as the CLR has none. Each is
    /// a constructor that its meta-compiler marked <see cref="Package.DotNet.NamedConstructor"/>,
    /// and is made, once the class's members are built, as their decorators left them:
    /// <list type="bullet">
    /// <item>the constructor, private, taking first a private marker type of its own,
    /// nested in the class and named <c>&lt;name&gt;</c>, so no other constructor has its
    /// signature;</item>
    /// <item>a static method of the name, with the constructor's accessibility and
    /// parameters, which calls it, and is what <c>Class.name(...)</c> calls.</item>
    /// </list>
    /// Both say they are the named constructor.
    /// </summary>
    public sealed class NamedConstructors(ILLoader il)
    {
        public void Define(Package.DotNet.Class owner)
        {
            foreach (var constructor in owner.Constructors.ToList())
                if (constructor.Annotations.OfType<Package.DotNet.NamedConstructor>().FirstOrDefault() is { } named)
                    Define(owner, constructor, named);
        }

        private void Define(Package.DotNet.Class owner, Package.DotNet.Constructor constructor, Package.DotNet.NamedConstructor named)
        {
            // What the static method returns is the class, which a generic one would need
            // instantiated with its own type parameters.
            if (owner.GenericParameters.Count > 0)
                throw new InvalidOperationException($"Class {owner.Name} is generic, so its named constructor {named.Name} can't be defined yet.");

            var marker = new Package.DotNet.Class($"<{named.Name}>") { Accessibility = Package.DotNet.Accessibility.Private };
            owner.NestedDefinitions.Add(marker);

            // The marker goes before what a call passes, after an explicit this.
            List<Code.Parameter> called = [.. il.CallParameters(constructor.Parameters, constructor.CustomMetadata)];
            constructor.Parameters.Insert(constructor.Parameters.Count - called.Count, new Package.DotNet.Parameter("marker", marker));

            var factory = new Package.DotNet.Method(named.Name)
            {
                IsStatic = true,
                Accessibility = constructor.Accessibility,
                ReturnType = owner,
                Annotations = { named },
            };

            var call = new Code.Call { Callee = new Code.Get(owner) };
            call.Arguments.Add(new() { Value = new Code.Cast { Value = new Code.NullLiteral(), Type = new Code.Get(marker) } });

            foreach (var parameter in called)
            {
                var passed = new Package.DotNet.Parameter(parameter.Name, parameter.Type)
                {
                    DefaultValue = (parameter as Package.DotNet.Parameter)?.DefaultValue,
                };

                factory.Parameters.Add(passed);
                call.Arguments.Add(new() { Value = new Code.Get(passed) });
            }

            factory.Body = new();
            factory.Body.Block.Statements.Add(new Code.Return { Value = call });
            owner.Methods.Add(factory);

            constructor.Accessibility = Package.DotNet.Accessibility.Private;
        }
    }
}
