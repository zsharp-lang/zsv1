namespace ZSharp.IR
{
    public sealed class ImportedMember : ModuleMember
    {
        public required ModuleMember Member { get; init; }
    }
}
