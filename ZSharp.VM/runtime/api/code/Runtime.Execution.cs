namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Execute the given code in a new context using the given locals.
        /// </summary>
        /// <param name="code">The code to evaluate.</param>
        /// <param name="locals">Values to assign to local variables which 
        /// the frame will be created with.</param>
        public void ExecuteInNewFrame(Code code, params ZSObject[] locals)
            => Run(code, locals);

        /// <summary>
        /// Execute the given code in the context of the current frame.
        /// </summary>
        /// <param name="code">The code to evaluate.</param>
        /// <param name="isolated">If <see langword="true"/>, the code will not modify the context
        /// of the current frame but a copy of it.</param>
        public void ExecuteInCurrentFrame(Code code, bool isolated)
        {
            Frame frame = Frame.NoArguments(0, code.Instructions, code.StackSize);
            RunFrame(frame);
            if (!isolated)
                for (int i = 0; i < CurrentFrame.Locals.Length; i++)
                    CurrentFrame.Local(i, frame.Local(i));
        }
    }
}
