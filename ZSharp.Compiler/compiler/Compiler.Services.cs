namespace ZSharp.Compiler
{
    public sealed partial class Compiler
    {
        private CG cg = new();
        private Evaluator evaluator = new();
        private IR ir = new(runtimeModule);
        private OOP oop = new();
        private Overloading overloading = new();
        private Reflection reflection = new();
        private TS ts = new();

        public ref CG CG => ref cg;

        public ref Evaluator Evaluator => ref evaluator;

        public ref IR IR => ref ir;

        public ref OOP OOP => ref oop;

        public ref Reflection Reflection => ref reflection;

        public ref TS TS => ref ts;

        //public TS TypeSystem { init => ts = value; }
    }
}
