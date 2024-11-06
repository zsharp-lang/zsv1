using CommonZ.Utils;

namespace ZSharp.IR
{
    public sealed class ImportedModule(Module imported) : ModuleMember
    {
        private ModuleCollection<ImportedMember>? _importedMembers;

        public Module Imported { get; } = imported;

        public Collection<ImportedMember> ImportedMembers
        {
            get
            {
                if (Module is null)
                    throw new InvalidOperationException("Can't access imported members without a module");

                if (_importedMembers is not null)
                    return _importedMembers;

                Interlocked.CompareExchange(ref _importedMembers, new(Module), null);
                return _importedMembers;
            }
        }

        public bool HasImportedMembers => !_importedMembers.IsNullOrEmpty();
    }
}
