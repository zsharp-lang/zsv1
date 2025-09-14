namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private CG cg;
        private IR ir;
        private Overloading overloading;
        private Reflection reflection;
        private TS ts;

        public ref CG CG => ref cg;

        public ref IR IR => ref ir;

        public ref Reflection Reflection => ref reflection;

        public ref TS TS => ref ts;

        public TS TypeSystem { init => ts = value; }
    }
}
