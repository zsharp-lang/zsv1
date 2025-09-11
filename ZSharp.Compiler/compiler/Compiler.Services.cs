namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private CG cg;
        private TS ts;

        public ref CG CG => ref cg;

        public IR IR { get; }

        public ref TS TS => ref ts;

        public TS TypeSystem { init => ts = value; }
    }
}
