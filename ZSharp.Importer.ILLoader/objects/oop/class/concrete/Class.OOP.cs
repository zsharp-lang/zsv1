using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler.Features;

namespace ZSharp.Importer.ILLoader.Objects
{
    partial class Class : ISingleInheritance
    {
        private CompilerObject? _base;

        public CompilerObject? Base
        {
            get
            {
                if (_base is not null) return _base;

                return _base = Loader.LoadType(IL.BaseType ?? typeof(object));
            }
        }

        [MemberNotNullWhen(true, nameof(Base))]
        public bool HasBase => Base is not null;
    }
}
