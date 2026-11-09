namespace ZSharp.Platform.Runtime
{
    /// <summary>
    /// The CLR handles, realized by the running process: an entity is found in its module
    /// by its metadata token, and this runtime's own module, whose types it built, before
    /// the modules of the assemblies loaded.
    /// </summary>
    partial class Runtime
    {
        private readonly Dictionary<ClrType, Type> clrTypes = [];
        private readonly Dictionary<ClrMethod, IL.MethodBase> clrMethods = [];
        private readonly Dictionary<ClrField, IL.FieldInfo> clrFields = [];

        private static readonly Dictionary<short, Emit.OpCode> opcodes = typeof(Emit.OpCodes)
            .GetFields(IL.BindingFlags.Public | IL.BindingFlags.Static)
            .Select(field => (Emit.OpCode)field.GetValue(null)!)
            .ToDictionary(opcode => opcode.Value);

        private IL.Module ModuleOf(ClrEntity entity)
        {
            if (entity.Module == module.ScopeName && entity.Assembly == module.Assembly.GetName().Name)
                return module;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetName().Name == entity.Assembly && assembly.GetModules().FirstOrDefault(candidate => candidate.ScopeName == entity.Module) is { } found)
                    return found;

            throw new InvalidOperationException($"No assembly {entity.Assembly} with a module {entity.Module} is loaded.");
        }

        internal Type ImportClrType(ClrType type)
        {
            if (!clrTypes.TryGetValue(type, out var result))
                clrTypes[type] = result = ModuleOf(type).ResolveType(type.Token);

            return result;
        }

        private const IL.BindingFlags Declared = IL.BindingFlags.Public | IL.BindingFlags.NonPublic
            | IL.BindingFlags.Instance | IL.BindingFlags.Static | IL.BindingFlags.DeclaredOnly;

        /// <summary>
        /// A method as its declaring type has it, found among the type's members by its
        /// token, which a generic instance's members share with its definition's. Of an
        /// instance that has a type still being built, it is found through the definition's.
        /// A method the runtime makes up, as an array's, has no token, so it is found by its
        /// name and the types of its parameters.
        /// </summary>
        internal IL.MethodBase ImportClrMethod(ClrMethod method)
        {
            if (clrMethods.TryGetValue(method, out var known))
                return known;

            var owner = ImportType(method.DeclaringType);
            IL.MethodBase result;

            if ((method.Token & 0x00FFFFFF) == 0)
            {
                var parameters = method.Parameters.Select(ImportType).ToArray();

                result = method.IsConstructor
                    ? owner.GetConstructor(Declared, parameters)!
                    : owner.GetMethod(method.Name, Declared, parameters)!;
            }
            else if (ContainsBuilder(owner))
                result = OnInstance(Member(owner.GetGenericTypeDefinition(), method), owner);
            else
                result = Member(owner, method);

            if (result is null)
                throw new InvalidOperationException($"{method.Name} isn't a method of {owner.Name}.");

            if (method.TypeArguments.Count > 0)
                result = ((IL.MethodInfo)result).MakeGenericMethod([.. method.TypeArguments.Select(ImportType)]);

            return clrMethods[method] = result;
        }

        private static IL.MethodBase Member(Type type, ClrMethod method)
            => method.IsConstructor
                ? type.GetConstructors(Declared).First(constructor => constructor.MetadataToken == method.Token)
                : type.GetMethods(Declared).First(candidate => candidate.MetadataToken == method.Token);

        internal IL.FieldInfo ImportClrField(ClrField field)
        {
            if (clrFields.TryGetValue(field, out var known))
                return known;

            var owner = ImportType(field.DeclaringType);

            var result = ContainsBuilder(owner)
                ? Emit.TypeBuilder.GetField(owner, owner.GetGenericTypeDefinition().GetFields(Declared).First(candidate => candidate.MetadataToken == field.Token))
                : owner.GetFields(Declared).First(candidate => candidate.MetadataToken == field.Token);

            return clrFields[field] = result;
        }

        private static IL.MethodBase OnInstance(IL.MethodBase method, Type type)
            => ContainsBuilder(type)
                ? method switch
                {
                    IL.ConstructorInfo constructor => Emit.TypeBuilder.GetConstructor(type, constructor),
                    IL.MethodInfo info => Emit.TypeBuilder.GetMethod(type, info),
                    _ => throw new NotSupportedException($"Can't call a {method.GetType().Name}.")
                }
                : IL.MethodBase.GetMethodFromHandle(method.MethodHandle, type.TypeHandle)!;

        /// <summary>
        /// Runs a function whose code is IL in place of a call: its arguments are already
        /// on the stack.
        /// </summary>
        internal void EmitClrCode(Emit.ILGenerator il, ClrCode code)
        {
            var locals = code.Locals.Select(local => il.DeclareLocal(ImportType(local))).ToArray();

            foreach (var instruction in code.Instructions)
            {
                if (!opcodes.TryGetValue(instruction.OpCode, out var opcode))
                    throw new NotSupportedException($"There is no instruction with the opcode {instruction.OpCode}.");

                switch (instruction.Operand)
                {
                    case null: il.Emit(opcode); break;
                    case int value: il.Emit(opcode, value); break;
                    case long value: il.Emit(opcode, value); break;
                    case float value: il.Emit(opcode, value); break;
                    case double value: il.Emit(opcode, value); break;
                    case string value: il.Emit(opcode, value); break;
                    case ClrLocal local: il.Emit(opcode, locals[local.Index]); break;
                    case ClrField field: il.Emit(opcode, ImportClrField(field)); break;
                    case ClrMethod method when ImportClrMethod(method) is IL.ConstructorInfo constructor: il.Emit(opcode, constructor); break;
                    case ClrMethod method: il.Emit(opcode, (IL.MethodInfo)ImportClrMethod(method)); break;
                    case ClassMethod method: il.Emit(opcode, Referenced(method)); break;
                    // A function of no module runs on its own, as a dynamic method, which
                    // nothing can point to.
                    case LIR.Function { Module: null } function:
                        throw new NotSupportedException($"Function {function.Name} belongs to no module, so the runtime can't point to it, as a delegate of it would.");
                    case LIR.Function function: il.Emit(opcode, ImportFunction(function)); break;
                    case LIR.Type type: il.Emit(opcode, ImportType(type)); break;
                    default: throw new NotSupportedException($"An instruction can't have a {instruction.Operand.GetType().Name} as its operand.");
                }
            }
        }
    }
}
