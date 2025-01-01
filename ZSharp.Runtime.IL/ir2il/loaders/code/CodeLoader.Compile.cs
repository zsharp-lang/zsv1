using VM = ZSharp.IR.VM;

namespace ZSharp.Runtime.NET.IR2IL
{
    partial class CodeLoader
    {
        private readonly Stack<Type> _stack = [];

        private void Compile(VM.Call call)
        {
            if (!Context.Cache(call.Callable, out var callable))
                throw new();

            foreach (var _ in call.Callable.Signature.GetParameters())
                _stack.Pop();

            if (callable is IL.MethodInfo method)
            {
                Output.Emit(IL.Emit.OpCodes.Call, method);

                if (method.ReturnType != typeof(void))
                    _stack.Push(method.ReturnType);
            }
            else if (callable is IL.ConstructorInfo constructor)
                Output.Emit(IL.Emit.OpCodes.Call, constructor);
            else throw new($"Unknown callable type: {callable.GetType()}");
        }

        private void Compile(VM.CallIndirect callIndirect)
        {
            throw new NotImplementedException();
        }

        private void Compile(VM.CallInternal callInternal)
        {
            throw new($"Use {nameof(VM.Call)} instead!");
        }

        private void Compile(VM.CallVirtual callVirtual)
        {
            if (!Context.Cache<IL.MethodInfo>(callVirtual.Method, out var method))
                throw new();

            if (!method.IsVirtual && !method.IsAbstract)
                throw new($"Method {method} is not virtual or abstract!");

            Output.Emit(IL.Emit.OpCodes.Callvirt, method);

            if (method.ReturnType != typeof(void))
                _stack.Push(method.ReturnType);
        }

        private void Compile(VM.CreateInstance createInstance)
        {
            if (!Context.Cache<IL.ConstructorInfo>(createInstance.Constructor.Method, out var constructor))
                throw new();

            Output.Emit(IL.Emit.OpCodes.Newobj, constructor);

            _stack.Push(constructor.DeclaringType ?? throw new());
        }

        private void Compile(VM.Dup _)
        {
            Output.Emit(IL.Emit.OpCodes.Dup);

            _stack.Push(_stack.Peek());
        }

        private void Compile(VM.GetArgument getArgument)
        {
            var p = Args[getArgument.Argument];
            var index = p.Position;

            if (index == 0)
                Output.Emit(IL.Emit.OpCodes.Ldarg_0);
            else if (index == 1)
                Output.Emit(IL.Emit.OpCodes.Ldarg_1);
            else if (index == 2)
                Output.Emit(IL.Emit.OpCodes.Ldarg_2);
            else if (index == 3)
                Output.Emit(IL.Emit.OpCodes.Ldarg_3);
            else if (index <= byte.MaxValue)
                Output.Emit(IL.Emit.OpCodes.Ldarg_S, (byte)index);
            else
                Output.Emit(IL.Emit.OpCodes.Ldarg, index);

            _stack.Push(p.Type);
        }

        private void Compile(VM.GetField getField)
        {
            if (!Context.Cache(getField.Field, out var field))
                throw new();

            Output.Emit(field.IsStatic ? IL.Emit.OpCodes.Ldsfld : IL.Emit.OpCodes.Ldfld, field);

            _stack.Push(field.FieldType);
        }

        private void Compile(VM.GetGlobal getGlobal)
        {
            if (!Context.Cache(getGlobal.Global, out var global))
                throw new();

            Output.Emit(IL.Emit.OpCodes.Ldsfld, global);

            _stack.Push(global.FieldType);
        }

        private void Compile(VM.GetLocal getLocal)
        {
            var l = Locals[getLocal.Local];
            var index = l.LocalIndex;

            if (index == 0)
                Output.Emit(IL.Emit.OpCodes.Ldloc_0);
            else if (index == 1)
                Output.Emit(IL.Emit.OpCodes.Ldloc_1);
            else if (index == 2)
                Output.Emit(IL.Emit.OpCodes.Ldloc_2);
            else if (index == 3)
                Output.Emit(IL.Emit.OpCodes.Ldloc_3);
            else if (index <= byte.MaxValue)
                Output.Emit(IL.Emit.OpCodes.Ldloc_S, (byte)index);
            else
                Output.Emit(IL.Emit.OpCodes.Ldloc, index);

            _stack.Push(l.LocalType);
        }

        private void Compile(VM.GetObject getObject)
        {
            Loader.GetObjectFunction(this, getObject);
        }

        private void Compile(VM.Jump jump)
        {
            Output.Emit(IL.Emit.OpCodes.Br, labels[jump.Target]);
        }

        private void Compile(VM.JumpIfTrue jumpIfTrue)
        {
            Output.Emit(IL.Emit.OpCodes.Brtrue, labels[jumpIfTrue.Target]);

            _stack.Pop();
        }

        private void Compile(VM.JumpIfFalse jumpIfFalse)
        {
            Output.Emit(IL.Emit.OpCodes.Brfalse, labels[jumpIfFalse.Target]);

            _stack.Pop();
        }

        private void Compile(VM.Nop _)
        {
            Output.Emit(IL.Emit.OpCodes.Nop);
        }

        private void Compile(VM.Pop _)
        {
            Output.Emit(IL.Emit.OpCodes.Pop);

            _stack.Pop();
        }

        private void Compile(VM.PutBoolean putBoolean)
        {
            Output.Emit(putBoolean.Value ? IL.Emit.OpCodes.Ldc_I4_1 : IL.Emit.OpCodes.Ldc_I4_0);

            _stack.Push(typeof(bool));
        }

        private void Compile(VM.PutFloat32 putFloat32)
        {
            Output.Emit(IL.Emit.OpCodes.Ldc_R4, putFloat32.Value);

            _stack.Push(typeof(float));
        }

        private void Compile(VM.PutInt32 putInt32)
        {
            Output.Emit(IL.Emit.OpCodes.Ldc_I4, putInt32.Value);

            _stack.Push(typeof(int));
        }

        private void Compile(VM.PutNull _)
        {
            Output.Emit(IL.Emit.OpCodes.Ldnull);

            _stack.Push(typeof(object));
        }

        private void Compile(VM.PutString putString)
        {
            Output.Emit(IL.Emit.OpCodes.Ldstr, putString.Value);

            _stack.Push(typeof(string));
        }

        private void Compile(VM.Return _)
        {
            Output.Emit(IL.Emit.OpCodes.Ret);
        }

        private void Compile(VM.SetArgument setArgument)
        {
            var index = Args[setArgument.Argument].Position;

            if (index <= byte.MaxValue)
                Output.Emit(IL.Emit.OpCodes.Starg_S, (byte)index);
            else
                Output.Emit(IL.Emit.OpCodes.Starg, index);

            _stack.Pop();
        }

        private void Compile(VM.SetField setField)
        {
            if (!Context.Cache(setField.Field, out var field))
                throw new();

            Output.Emit(field.IsStatic ? IL.Emit.OpCodes.Stsfld : IL.Emit.OpCodes.Stfld, field);

            _stack.Pop();
        }

        private void Compile(VM.SetGlobal setGlobal)
        {
            if (!Context.Cache(setGlobal.Global, out var global))
                throw new();

            Output.Emit(IL.Emit.OpCodes.Stsfld, global);

            _stack.Pop();
        }

        private void Compile(VM.SetLocal setLocal)
        {
            var index = Locals[setLocal.Local].LocalIndex;

            if (index <= byte.MaxValue)
                Output.Emit(IL.Emit.OpCodes.Stloc_S, (byte)index);
            else
                Output.Emit(IL.Emit.OpCodes.Stloc, index);

            _stack.Pop();
        }

        private void Compile(VM.Swap _)
        {
            var tmp1 = Output.DeclareLocal(_stack.Peek());
            Output.Emit(IL.Emit.OpCodes.Stloc, tmp1.LocalIndex);
            var tp1 = _stack.Pop();

            var tmp2 = Output.DeclareLocal(_stack.Peek());
            Output.Emit(IL.Emit.OpCodes.Stloc, tmp2.LocalIndex);
            var tp2 = _stack.Pop();

            Output.Emit(IL.Emit.OpCodes.Ldloc, tmp1.LocalIndex);

            _stack.Push(tp1);

            Output.Emit(IL.Emit.OpCodes.Ldloc, tmp2.LocalIndex);

            _stack.Push(tp2);
        }
    }
}
