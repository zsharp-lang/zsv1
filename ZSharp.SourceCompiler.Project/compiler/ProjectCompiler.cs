using System;

namespace ZSharp.SourceCompiler.Project
{
    public sealed partial class ProjectCompiler(
        Interpreter.Interpreter interpreter,
        Project project
    )
    {
        public Interpreter.Interpreter Interpreter { get; } = interpreter;

        public Project Project { get; } = project;

        public ProjectResult Compile()
        {
            throw new NotImplementedException();
        }
    }
}
