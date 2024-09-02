namespace ZSharp.VM
{
    public sealed partial class Runtime
    {
        private static void ExecuteNop() { }

        private void ExecutePush(Instruction instruction)
            => CurrentFrame.Push(instruction.As<ZSObject>());

        private void ExecutePop(Instruction _)
            => CurrentFrame.Pop();

        private void ExecuteDup(Instruction _)
            => CurrentFrame.Push(CurrentFrame.Peek());

        private void ExecuteGetArgument(Instruction instruction)
            => CurrentFrame.Push(CurrentFrame.Argument(instruction.As<int>()));

        private void ExecuteSetArgument(Instruction instruction)
            => CurrentFrame.Argument(instruction.As<int>(), CurrentFrame.Pop());

        private void ExecuteGetLocal(Instruction instruction)
            => CurrentFrame.Push(CurrentFrame.Local(instruction.As<int>()));

        private void ExecuteSetLocal(Instruction instruction)
            => CurrentFrame.Local(instruction.As<int>(), CurrentFrame.Pop());

        private void ExecuteJump(Instruction instruction)
            => CurrentFrame.JumpTo(instruction.As<int>());

        private void ExecuteJumpIf(Instruction instruction)
            => CurrentFrame.JumpTo(IsTrue(CurrentFrame.Pop()) ? instruction.As<int>() : 0);

        private void ExecuteCall(Instruction instruction)
        {
            var function = instruction.As<ZSFunction>();
            
            var args = new ZSObject[function.ArgumentCount];
            for (var i = args.Length - 1; i >= 0; i--)
                args[i] = CurrentFrame.Pop();

            var frame = new Frame(args, function.LocalCount, function.Code, function.StackSize);

            PushFrame(frame);
        }

        private void ExecuteCallInternal(Instruction instruction)
        {
            var function = instruction.As<ZSInternalFunction>();

            var args = new ZSObject[function.ArgumentCount];
            for (var i = args.Length - 1; i >= 0; i--)
                args[i] = CurrentFrame.Pop();

            var result = function.Call(args);

            ExecuteReturnVoid();

            if (result is not null)
                CurrentFrame.Push(result);
        }

        private void ExecuteCallVirtual(Instruction instruction)
        {
            int argc = instruction.As<int>();

            var args = new ZSObject[argc];
            for (var i = args.Length - 1; i >= 0; i--)
                args[i] = CurrentFrame.Pop();

            var function = CurrentFrame.Pop() as ZSFunction ?? throw new Exception();

            if (args.Length < 1) throw new("Virtual call requires at least one argument.");
            if (args[0].Type is not Types.Object objectType)
                throw new("Virtual method may only be called when the first argument is an object.");
            if (objectType.VTable is null)
                throw new("Object doesn't have a vtable");

            function = objectType.VTable[function];

            var frame = new Frame(args, function.LocalCount, function.Code, function.StackSize);

            PushFrame(frame);
        }

        private void ExecuteReturn()
        {
            var returnValue = CurrentFrame.Pop();

            ExecuteReturnVoid();

            CurrentFrame.Push(returnValue);
        }

        private void ExecuteReturnVoid()
        {
            PopFrame();
        }

        private void ExecuteNewArray(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        private void ExecuteGetMember(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        private void ExecuteSetMember(Instruction instruction)
        {
            throw new NotImplementedException();
        }

        private void ExecuteGetField(Instruction instruction)
        {
            if (CurrentFrame.Pop() is not ZSStruct @struct)
                throw new Exception();

            CurrentFrame.Push(@struct.GetField(instruction.As<int>()));
        }

        private void ExecuteSetField(Instruction instruction)
        {
            if (CurrentFrame.Pop() is not ZSStruct @struct)
                throw new Exception();

            @struct.SetField(instruction.As<int>(), CurrentFrame.Pop());
        }

        private void ExecuteLoadObjectFromMetadata(Instruction instruction)
        {
            CurrentFrame.Push(LoadIR(instruction.As<IR.IRObject>()));
        }
    }
}
