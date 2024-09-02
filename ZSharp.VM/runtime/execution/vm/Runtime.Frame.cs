namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        private readonly Stack<Frame> frameStack = [];

        internal Frame CurrentFrame => frameStack.Peek();

        public bool HasFrames => frameStack.Count > 0;

        private void PushFrame(Frame frame)
            => frameStack.Push(frame);

        private Frame PopFrame() => frameStack.Pop();
    }
}
