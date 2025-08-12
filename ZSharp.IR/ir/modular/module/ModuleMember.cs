namespace ZSharp.IR
{
    public abstract class ModuleMember 
        : IRDefinition
        , IModuleMember
    {
        public Module? Module { get; internal set; }
    }
}
