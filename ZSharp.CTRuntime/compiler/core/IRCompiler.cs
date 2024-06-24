//using CommonZ.Utils;
//using System.Diagnostics.CodeAnalysis;
//using ZSharp.RAST;
//using ZSharp.VM;

//namespace ZSharp.CTRuntime
//{
//    public sealed class IRCompiler
//    {
//        private readonly CodeCompiler<CTType> ctCodeCompiler;
//        private readonly CodeCompiler<RTType> rtCodeCompiler;
//        private readonly ModuleCompiler moduleCompiler;

//        public Interpreter Interpreter { get; }

//        public IRCompiler()
//            : this(new())
//        {
//        }

//        public IRCompiler(Interpreter interpreter)
//        {
//            Interpreter = interpreter;

//            ctCodeCompiler = new(this, new GenericBinder<ZSObject>(;
//            moduleCompiler = new(this);
//        }

//        internal ContextManager UseCTCompiler(ICTDefinitionCompiler compiler)
//            => ctCodeCompiler.UseCompiler(compiler);

//        internal ContextManager UseRTCompiler(IRTDefinitionCompiler compiler)
//            => rtCodeCompiler.UseCompiler(compiler);

//        public Code Compile(RExpression expression)
//            => codeCompiler.Compile(expression);

//        public Code Compile(RStatement statement)
//            => codeCompiler.Compile(statement);

//        public IR.Module Compile(RModule module)
//            => moduleCompiler.Compile(module);

//        public IR.Module Compile(DomainContext context, RModule module)
//        {
//            using (UseContext(context)) 
//                return Compile(module);
//        }

//        #region Evaluation

//        public ZSObject? Evaluate(Code code)
//        {
//            var instructions = Interpreter.Assemble(code.Instructions);
//            var result = Interpreter.Evaluate(instructions, code.StackSize);

//            if (result is null && code.Type != TypeSystem.Void)
//                throw new("Invalid return value");

//            return result;
//        }

//        public ZSObject? Evaluate(RExpression expression) 
//            => Evaluate(Compile(expression));

//        public T? Evaluate<T>(Code code)
//            where T : class
//            => Evaluate(code) as T;

//        public T? Evaluate<T>(RExpression expression)
//            where T : class
//            => Evaluate<T>(Compile(expression));

//        public ZSObject EvaluateObject(Code code)
//            => Evaluate(code) ?? throw new("Invalid return value");

//        public ZSObject EvaluateObject(RExpression expression)
//            => EvaluateObject(Compile(expression));

//        public T EvaluateObject<T>(Code code)
//            where T : class
//            => EvaluateObject(code) as T ?? throw new("Invalid return value");

//        public T EvaluateObject<T>(RExpression expression)
//            where T : class
//            => EvaluateObject<T>(Compile(expression));

//        public bool TryEvaluate(Code code, [NotNullWhen(true)] out ZSObject? result)
//        {
//            return (result = Evaluate(code)) is not null;
//        }

//        public bool TryEvaluate(RExpression expression, [NotNullWhen(true)] out ZSObject? result) 
//            => TryEvaluate(Compile(expression), out result);

//        public bool TryEvaluate<T>(Code code, [NotNullWhen(true)] out T? result)
//            where T : class
//            => TryEvaluate(code, out result) && result is not null;

//        public bool TryEvaluate<T>(RExpression expression, [NotNullWhen(true)] out T? result)
//            where T : class
//            => TryEvaluate<T>(Compile(expression), out result);

//        #endregion

//        #region Execution

//        public void Execute(Code code) 
//            => Interpreter.Execute(Interpreter.Assemble(code.Instructions), code.StackSize);

//        public void Execute(RStatement statement)
//            => Execute(Compile(statement));

//        #endregion
//    }
//}
