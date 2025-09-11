namespace ZSharp.Compiler.ILLoader.Objects
{
    partial class Module
    {
        public ILLoader Loader { get; }

        internal ModuleBodyLoader BodyLoader { get; }

        public CompilerObject? LoadMember(string name)
        {
            if (!BodyLoader.LoadMember(name))
                return null;

            return Members[name];
        }
    }
}
