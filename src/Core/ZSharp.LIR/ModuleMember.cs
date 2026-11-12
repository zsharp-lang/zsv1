namespace ZSharp.LIR
{
    public abstract class ModuleMember : Definition
    {
        public Module? Module { get; internal set; }
    }
}
