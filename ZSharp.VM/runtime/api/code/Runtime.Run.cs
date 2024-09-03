namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        /// <summary>
        /// Runs the given code in a new frame until the end of the frame.
        /// </summary>
        /// <param name="code"></param>
        public void Run(Code code)
            => Run(code, []);

        /// <summary>
        /// Runs the given code in a new frame until the end of the frame, using the given locals.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="locals"></param>
        public void Run(Code code, params ZSObject[] locals)
            => Run(Frame.WithLocals(code, locals));

        /// <summary>
        /// Runs until there are no more frames on the stack.
        /// This is usually not what you want to use.
        /// You probably want to use <see cref="Run(Code)"/> or <see cref="Run(Code, ZSObject[])"/>
        /// </summary>
        public void RunAllFrames()
        {
            while (HasFrames)
                RunCurrentFrame();
        }
    }
}
