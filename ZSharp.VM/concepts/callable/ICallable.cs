namespace ZSharp.VM
{
    public interface ICallable<T>
    {
        public ZSObject? Call(Interpreter interpreter, ZSObject callable, Argument[] arguments);
    }
}
