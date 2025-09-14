using CommonZ.Utils;
using ZSharp.Importer.ILLoader.Objects;

namespace ZSharp.Importer.ILLoader
{
    partial class ILLoader
    {
        public Mapping<string, Namespace> Namespaces { get; } = [];

        public Namespace Namespace(string fullName)
        {
            var parts = fullName.Split('.');

            if (parts.Length == 0)
                throw new ArgumentException(
                    "Namespace name must contain at least 1 identifier",
                    nameof(fullName)
                );

            if (!Namespaces.TryGetValue(parts[0], out var @namespace))
                @namespace = Namespaces[parts[0]] = new(parts[0], this);

            foreach (var part in parts.Skip(1))
                if (!@namespace.Members.TryGetValue(part, out var member))
                    @namespace.AddMember(part, member = new Namespace(part, this));
                else if (member is not Namespace ns)
                    throw new ArgumentException(
                        $"{part} in {@namespace.FullName} is not a namespace"
                    );
                else @namespace = ns;

            return @namespace;
        }
    }
}
