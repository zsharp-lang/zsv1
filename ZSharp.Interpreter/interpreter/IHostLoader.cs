namespace ZSharp.Interpreter
{
    public interface IHostLoader
    {
        public IR.Module Import(System.Reflection.Module module);
    }
}
