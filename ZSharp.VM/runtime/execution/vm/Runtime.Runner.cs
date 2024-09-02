namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        private void Run(Frame frame)
            => RunFrame(frame);

        private void RunFrame(Frame frame)
        {
            PushFrame(frame);
            RunCurrentFrame();
        }

        private void RunCurrentFrame()
        {
            int frameCount = frameStack.Count;
            while (frameStack.Count >= frameCount && !CurrentFrame.IsEndOfProgram)
                Execute(CurrentFrame.NextInstruction());
        }
    }
}
