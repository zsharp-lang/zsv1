namespace ZSharp.Interpreter
{
    public interface IRuntime
    {
        public void Import(IR.Module module);
    }
}
