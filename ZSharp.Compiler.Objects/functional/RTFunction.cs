using CommonZ.Utils;
using System.Diagnostics.CodeAnalysis;
using ZSharp.Compiler;

namespace ZSharp.Objects
{
    public class RTFunction(string? name)
        : Function(name)
        , ICTReadable
        , ICompileIRObject<IR.Function, IR.Module>
    {
        [Flags]
        enum BuildState
        {
            None = 0,
            Signature = 0b1,
            Body = 0b10,
            Owner = 0b100,
        }

        private readonly ObjectBuildState<BuildState> state = new();

        CompilerObject ITyped.Type => throw new NotImplementedException();

        public Signature Signature { get; set; } = new();

        public CompilerObject? ReturnType { get; set; }

        public override CompilerObject Call(Compiler.Compiler compiler, Argument[] arguments)
        {
            var (args, kwargs) = Utils.SplitArguments(arguments);

            try
            {
                return Call(compiler, args, kwargs);
            }
            catch (Compiler.InvalidCastException)
            {
                throw new ArgumentMismatchException(this, arguments);
            }
        }

        private RawCode Call(Compiler.Compiler compiler, Collection<CompilerObject> args, Mapping<string, CompilerObject> kwArgs)
        {
            IR ??= CompileIRObject(compiler, null);

            IRCode
                argsCode = new(), varArgsCode = new(),
                kwArgsCode = new(), varKwArgsCode = new();

            if (args.Count > Signature.Args.Count)
                if (Signature.VarArgs is null)
                    throw new($"Function {Name} takes {Signature.Args.Count} arguments, but {args.Count} were given.");
                else
                    throw new NotImplementedException("var args");
            else if (args.Count < Signature.Args.Count)
                throw new($"Function {Name} takes {Signature.Args.Count} arguments, but {args.Count} were given.");

            for (int i = 0; i < Signature.Args.Count; i++)
                argsCode.Append(compiler.CompileIRCode(compiler.Cast(args[i], Signature.Args[i].Type ?? throw new())));

            if (kwArgs.Count > Signature.KwArgs.Count)
                if (Signature.VarKwArgs is null)
                    throw new($"Function {Name} takes {Signature.KwArgs.Count} keyword arguments, but {kwArgs.Count} were given.");
                else
                    throw new NotImplementedException("var kwargs");
            else if (kwArgs.Count < Signature.KwArgs.Count)
                throw new($"Function {Name} takes {Signature.KwArgs.Count} keyword arguments, but {kwArgs.Count} were given.");

            foreach (var kwArgParameter in Signature.KwArgs)
                kwArgsCode.Append(compiler.CompileIRCode(compiler.Cast(kwArgs[kwArgParameter.Name], kwArgParameter.Type ?? throw new())));

            IRCode result = new();
            result.Append(argsCode);
            result.Append(varArgsCode); // should be empty
            result.Append(kwArgsCode);
            result.Append(varKwArgsCode); // should be empty

            result.Instructions.Add(new IR.VM.Call(IR));

            result.Types.Clear();
            if (ReturnType != compiler.TypeSystem.Void)
                result.Types.Add(ReturnType!);

            result.MaxStackSize = Math.Max(result.MaxStackSize, result.Types.Count);

            return new(result);
        }

        IRCode ICTReadable.Read(Compiler.Compiler compiler)
            => new([
                new IR.VM.GetObject(IR!)
            ])
            {
                Types = [null!], // TODO: fix type
            };

        [MemberNotNull(nameof(ReturnType))]
        public IR.Function CompileIRObject(Compiler.Compiler compiler, IR.Module? owner)
        {
            IR ??= 
                new(
                    compiler.CompileIRType(
                        ReturnType ?? throw new PartiallyCompiledObjectException(
                            this,
                            Errors.UndefinedReturnType(Name)
                        )
                    )
                )
            {
                Name = Name
            };

            if (owner is not null && !state.Get(BuildState.Owner))
            {
                owner.Functions.Add(IR);

                state.Set(BuildState.Owner);
            }

            if (!state.Get(BuildState.Signature))
            {
                foreach (var arg in Signature.Args)
                    IR.Signature.Args.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(arg, IR.Signature));

                if (Signature.VarArgs is not null)
                    IR.Signature.Args.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarArgs, IR.Signature);

                foreach (var kwArg in Signature.KwArgs)
                    IR.Signature.KwArgs.Parameters.Add(compiler.CompileIRObject<IR.Parameter, IR.Signature>(kwArg, IR.Signature));

                if (Signature.VarKwArgs is not null)
                    IR.Signature.KwArgs.Var = compiler.CompileIRObject<IR.Parameter, IR.Signature>(Signature.VarKwArgs, IR.Signature);

                state.Set(BuildState.Signature);
            }

            if (Body is not null && !state.Get(BuildState.Body))
            {
                IR.Body.Instructions.AddRange(compiler.CompileIRCode(Body).Instructions);

                state.Set(BuildState.Body);
            }

            return IR;
        }
    }
}
