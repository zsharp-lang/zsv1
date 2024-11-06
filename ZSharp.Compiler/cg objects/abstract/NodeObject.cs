using ZSharp.RAST;

namespace ZSharp.Compiler
{
    public class NodeObject(RDefinition definition, CGObject @object)
        : CGObject
    {
        public RDefinition Node { get; } = definition;

        public CGObject Object { get; } = @object;
    }

    public sealed class NodeObject<R, CG>(R node, CG @object)
        : NodeObject(node, @object)
        where R : RDefinition
        where CG : CGObject
    {
        public new R Node => (R)base.Node;

        public new CG Object => (CG)base.Object;
    }
}
