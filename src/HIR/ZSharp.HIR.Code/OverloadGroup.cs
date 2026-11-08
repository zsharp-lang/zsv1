namespace ZSharp.HIR.Code
{
    /// <summary>
    /// Several instances that one name stands for. It is structure only: choosing
    /// between them is up to the CO it loads to.
    /// </summary>
    public sealed class OverloadGroup
        : Instance
    {
        public List<Instance> Items { get; } = [];
    }
}
