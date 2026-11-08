namespace ZSharp.Platform.Runtime
{
    partial class CodeEmitter
    {
        private void Expression(LIR.Expression expression)
        {
            switch (expression)
            {
                case LIR.Constant node: Constant(node); break;
                case LIR.GetLocal node: GetLocal(node.Variable); break;
                case LIR.GetGlobal node: il.Emit(Emit.OpCodes.Ldsfld, runtime.ImportGlobal(node.Global)); break;
                case LIR.GetField node: GetField(node); break;
                case LIR.GetFunction node: GetFunction(node); break;
                case LIR.Call node: Call(node); break;
                case LIR.CallIndirect node: CallIndirect(node); break;
                case LIR.New node: New(node); break;
                case LIR.Allocate node: Allocate(node); break;
                case LIR.Load node: Load(node); break;
                case LIR.Sequence node: Statement(node.Block); Expression(node.Result); break;
                case AddressOf node: Address(node.Variable); break;
                default: throw new NotSupportedException($"Can't emit a {expression.GetType().Name}.");
            }
        }

        private void Constant(LIR.Constant node)
        {
            switch (node.Value)
            {
                case null: Default(runtime.ImportType(node.Type)); break;
                case string value: il.Emit(Emit.OpCodes.Ldstr, value); break;
                case bool value: il.Emit(value ? Emit.OpCodes.Ldc_I4_1 : Emit.OpCodes.Ldc_I4_0); break;
                case int value: il.Emit(Emit.OpCodes.Ldc_I4, value); break;
                case long value: il.Emit(Emit.OpCodes.Ldc_I8, value); break;
                case float value: il.Emit(Emit.OpCodes.Ldc_R4, value); break;
                case double value: il.Emit(Emit.OpCodes.Ldc_R8, value); break;
                default: Exposed(node); break;
            }
        }

        private void Exposed(LIR.Constant node)
        {
            var type = runtime.ImportType(node.Type);

            il.Emit(Emit.OpCodes.Ldc_I4, ExposedObjects.Add(node.Value!));
            il.Emit(Emit.OpCodes.Call, typeof(ExposedObjects).GetMethod(nameof(ExposedObjects.Get))!);
            il.Emit(type.IsValueType ? Emit.OpCodes.Unbox_Any : Emit.OpCodes.Castclass, type);
        }

        private void Default(Type type)
        {
            if (!type.IsValueType)
            {
                il.Emit(Emit.OpCodes.Ldnull);
                return;
            }

            var temporary = il.DeclareLocal(type);

            il.Emit(Emit.OpCodes.Ldloca, temporary);
            il.Emit(Emit.OpCodes.Initobj, type);
            il.Emit(Emit.OpCodes.Ldloc, temporary);
        }

        private void GetLocal(LIR.Variable variable)
        {
            if (variable is LIR.Parameter parameter)
                il.Emit(Emit.OpCodes.Ldarg, ParameterIndex(parameter));
            else
                il.Emit(Emit.OpCodes.Ldloc, Local(variable));
        }

        private void GetField(LIR.GetField node)
        {
            FieldOwnerAddress(node.Target);
            il.Emit(Emit.OpCodes.Ldfld, runtime.ImportField(StructureOf(node.Target), node.Field));
        }

        private void GetFunction(LIR.GetFunction node)
        {
            if (node.Callable is not LIR.Function target)
                throw new NotSupportedException("Only a function defined in LIR can be used as a value.");

            il.Emit(Emit.OpCodes.Ldftn, runtime.ImportFunction(target));
        }

        private void Call(LIR.Call node)
        {
            // A built type calls a standalone function through its delegate, which goes
            // under the arguments.
            if (node.Callable is LIR.Function { Module: null } callee && !standalone)
            {
                var (index, type) = runtime.StandaloneDelegate(callee);

                il.Emit(Emit.OpCodes.Ldc_I4, index);
                il.Emit(Emit.OpCodes.Call, typeof(ExposedObjects).GetMethod(nameof(ExposedObjects.Get))!);
                il.Emit(Emit.OpCodes.Castclass, typeof(Standalone));
                il.Emit(Emit.OpCodes.Callvirt, typeof(Standalone).GetProperty(nameof(Standalone.Delegate))!.GetMethod!);
                il.Emit(Emit.OpCodes.Castclass, type);

                foreach (var argument in node.Arguments)
                    Expression(argument);

                il.Emit(Emit.OpCodes.Callvirt, type.GetMethod("Invoke")!);
                return;
            }

            foreach (var (argument, index) in node.Arguments.Select((argument, index) => (argument, index)))
                if (index == 0 && runtime.TakesAddress(node.Callable))
                    Address(argument);
                else
                    Expression(argument);

            runtime.EmitCall(il, node.Callable);
        }

        private void CallIndirect(LIR.CallIndirect node)
        {
            // The callee goes on top of the stack, but is evaluated first.
            var callee = il.DeclareLocal(typeof(nint));

            Expression(node.Callee);
            il.Emit(Emit.OpCodes.Stloc, callee);

            foreach (var argument in node.Arguments)
                Expression(argument);

            il.Emit(Emit.OpCodes.Ldloc, callee);
            il.EmitCalli(
                Emit.OpCodes.Calli,
                IL.CallingConventions.Standard,
                runtime.ImportType(node.Signature.ReturnType),
                [.. node.Signature.Parameters.Select(parameter => runtime.ImportType(parameter.Type))],
                null
            );
        }

        private void New(LIR.New node)
        {
            // A polymorphic structure's value is a new object of its class.
            if (Runtime.IsPolymorphic(node.Structure))
            {
                il.Emit(Emit.OpCodes.Newobj, runtime.Referenced(runtime.ConstructorOf(node.Structure)));

                foreach (var (field, initializer) in node.Structure.Fields.Zip(node.Fields))
                {
                    il.Emit(Emit.OpCodes.Dup);
                    Expression(initializer);
                    il.Emit(Emit.OpCodes.Stfld, runtime.ImportField(node.Structure, field));
                }

                return;
            }

            var type = runtime.ImportType(node.Structure);
            var value = il.DeclareLocal(type);

            il.Emit(Emit.OpCodes.Ldloca, value);
            il.Emit(Emit.OpCodes.Initobj, type);

            foreach (var (field, initializer) in node.Structure.Fields.Zip(node.Fields))
            {
                il.Emit(Emit.OpCodes.Ldloca, value);
                Expression(initializer);
                il.Emit(Emit.OpCodes.Stfld, runtime.ImportField(node.Structure, field));
            }

            il.Emit(Emit.OpCodes.Ldloc, value);
        }

        private void Allocate(LIR.Allocate node)
        {
            Expression(node.Value);
            il.Emit(Emit.OpCodes.Newobj, Runtime.BoxConstructor(runtime.ImportType(TypeOf(node))));
        }

        private void Load(LIR.Load node)
        {
            Expression(node.Reference);
            il.Emit(Emit.OpCodes.Ldfld, Runtime.BoxValue(runtime.ImportType(TypeOf(node.Reference))));
        }

        /// <summary>
        /// Puts the address of the structure a field is read from or written to on
        /// the stack.
        /// </summary>
        private void FieldOwnerAddress(LIR.Expression target)
        {
            // An object's fields are reached through the object itself.
            if (Runtime.IsPolymorphic(TypeOf(target)))
                Expression(target);
            else if (TypeOf(target) is LIR.Reference)
            {
                Expression(target);
                il.Emit(Emit.OpCodes.Ldflda, Runtime.BoxValue(runtime.ImportType(TypeOf(target))));
            }
            else
                Address(target);
        }

        private void Address(LIR.Expression expression)
        {
            switch (expression)
            {
                case LIR.GetLocal { Variable: LIR.Parameter parameter }:
                    il.Emit(Emit.OpCodes.Ldarga, ParameterIndex(parameter));
                    break;
                case LIR.GetLocal node:
                    il.Emit(Emit.OpCodes.Ldloca, Local(node.Variable));
                    break;
                case LIR.GetGlobal node:
                    il.Emit(Emit.OpCodes.Ldsflda, runtime.ImportGlobal(node.Global));
                    break;
                case LIR.GetField node:
                    FieldOwnerAddress(node.Target);
                    il.Emit(Emit.OpCodes.Ldflda, runtime.ImportField(StructureOf(node.Target), node.Field));
                    break;
                case LIR.Load node:
                    Expression(node.Reference);
                    il.Emit(Emit.OpCodes.Ldflda, Runtime.BoxValue(runtime.ImportType(TypeOf(node.Reference))));
                    break;
                case LIR.Sequence node when AddressOf.Has(node.Result):
                    Statement(node.Block);
                    Address(node.Result);
                    break;
                default:
                    var temporary = il.DeclareLocal(runtime.ImportType(TypeOf(expression)));
                    Expression(expression);
                    il.Emit(Emit.OpCodes.Stloc, temporary);
                    il.Emit(Emit.OpCodes.Ldloca, temporary);
                    break;
            }
        }
    }
}
