namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Evaluate the given code in a new context using the given locals.
        /// </summary>
        /// <param name="code">The code to evaluate.</param>
        /// <param name="locals">Values to assign to local variables which 
        /// the frame will be created with.</param>
        /// <returns>The value at the top of the stack after the code is executed.</returns>
        /// <exception cref="StackIsEmptyException">Raised if there is no object left on the
        /// stack at the end of execution. May also be raised when incorrect code is passed.</exception>
        public ZSObject EvaluateInNewFrame(Code code, params ZSObject[] locals)
        {
            Frame frame = Frame.WithLocals(code, locals);
            RunFrame(frame);
            return frame.Peek();
        }

        /// <summary>
        /// Evaluate the given code in the context of the current frame.
        /// </summary>
        /// <param name="code">The code to evaluate.</param>
        /// <param name="isolated">If <see langword="true"/>, the code will not modify the context
        /// of the current frame but a copy of it.</param>
        /// <returns>The value at the top of the stack after the code is executed.</returns>
        /// <exception cref="StackIsEmptyException">Raised if there is no object left on the
        /// stack at the end of execution. May also be raised when incorrect code is passed.
        /// The frame is modified regardless whether there's a value on the stack or not.</exception>
        public ZSObject EvaluateInCurrentFrame(Code code, bool isolated)
        {
            Frame frame = Frame.NoArguments(0, code.Instructions, code.StackSize);
            RunFrame(frame);
            if (!isolated)
                for (int i = 0; i < CurrentFrame.Locals.Length; i++)
                    CurrentFrame.Local(i, frame.Local(i));

            return frame.Peek();
        }
    }
}
