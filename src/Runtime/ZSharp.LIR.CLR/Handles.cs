namespace ZSharp.LIR.CLR
{
    /// <summary>
    /// The handles of the CTS and of IL, which every CLR platform realizes. A handle
    /// describes what it stands for by data only, so a platform maps it against the
    /// assemblies it targets, whether those of the running process or another version's.
    /// <para>
    /// An entity is found by the module that defines it and its metadata token, which a
    /// platform of the running process uses, and by its names, which a platform targeting
    /// other assemblies finds it by, as a type the reference assemblies forward.
    /// </para>
    /// </summary>
    public abstract class ClrEntity
        : LIR.Handle
    {
        /// <summary>
        /// The simple name of the assembly that defines it.
        /// </summary>
        public required string Assembly { get; init; }

        /// <summary>
        /// The name of the module of that assembly that defines it.
        /// </summary>
        public required string Module { get; init; }

        public int Token { get; init; }

        public required string Name { get; init; }
    }

    /// <summary>
    /// A type of the CTS. A generic one is its definition, which <see cref="LIR.GenericInstance"/>
    /// instantiates, and its name has its arity, as <c>List`1</c>.
    /// </summary>
    public sealed class ClrType
        : ClrEntity
        , LIR.Type
    {
        public string? Namespace { get; init; }

        public ClrType? DeclaringType { get; init; }

        public bool IsValueType { get; init; }

        /// <summary>
        /// Its name as reflection gives it: its namespace, and a nested type's after the
        /// type it is in, with a <c>+</c>.
        /// </summary>
        public string FullName
            => DeclaringType is { } outer
                ? $"{outer.FullName}+{Name}"
                : Namespace is { Length: > 0 } ? $"{Namespace}.{Name}" : Name;
    }

    /// <summary>
    /// An array of a type. A vector, the array of one dimension that starts at zero, has
    /// rank 1 and is what <c>T[]</c> is.
    /// </summary>
    public sealed class ClrArrayType(LIR.Type element, int rank = 1, bool isVector = true)
        : LIR.Type
    {
        public LIR.Type Element { get; } = element;

        public int Rank { get; } = rank;

        public bool IsVector { get; } = isVector;
    }

    /// <summary>
    /// A pointer to a value of a type.
    /// </summary>
    public sealed class ClrPointerType(LIR.Type target)
        : LIR.Type
    {
        public LIR.Type Target { get; } = target;
    }

    /// <summary>
    /// A type parameter of a generic type or method, by its position, as a member's
    /// declared signature uses it.
    /// </summary>
    public sealed class ClrGenericParameter(int position, bool ofMethod)
        : LIR.Type
    {
        public int Position { get; } = position;

        public bool OfMethod { get; } = ofMethod;
    }

    /// <summary>
    /// A member of a type, which belongs to the type it is declared by: for a member of a
    /// generic instance, the instance.
    /// </summary>
    public abstract class ClrMember
        : ClrEntity
    {
        /// <summary>
        /// The type the member is reached on: a <see cref="ClrType"/>, or a
        /// <see cref="LIR.GenericInstance"/> of one.
        /// </summary>
        public required LIR.Type DeclaringType { get; init; }
    }

    /// <summary>
    /// A method or a constructor. Calling it is what it does; an instance method takes its
    /// object first, by its address if it is a value type's.
    /// </summary>
    public sealed class ClrMethod(LIR.Signature signature)
        : ClrMember
        , LIR.ICallable
    {
        public LIR.Signature Signature { get; } = signature;

        public bool IsConstructor { get; init; }

        public bool IsStatic { get; init; }

        public bool IsVirtual { get; init; }

        /// <summary>
        /// Whether it takes its object by its address, as an instance method of a value
        /// type does.
        /// </summary>
        public bool TakesAddress { get; init; }

        public int GenericArity { get; init; }

        /// <summary>
        /// The types of its parameters as it declares them, which tell overloads apart.
        /// </summary>
        public List<LIR.Type> Parameters { get; init; } = [];

        /// <summary>
        /// The type arguments of an instance of a generic method, or none.
        /// </summary>
        public List<LIR.Type> TypeArguments { get; init; } = [];
    }

    public sealed class ClrField
        : ClrMember
    {
        public bool IsStatic { get; init; }
    }

    /// <summary>
    /// A function whose code is IL: calling it runs its instructions, with its arguments
    /// on the stack.
    /// </summary>
    public sealed class ClrCode(LIR.Signature signature)
        : LIR.Handle
        , LIR.ICallable
    {
        public LIR.Signature Signature { get; } = signature;

        /// <summary>
        /// The types of the locals its instructions use, which a <see cref="ClrLocal"/>
        /// refers to by its index.
        /// </summary>
        public List<LIR.Type> Locals { get; } = [];

        public List<ClrInstruction> Instructions { get; } = [];

        public ClrCode Emit(System.Reflection.Emit.OpCode opcode, object? operand = null)
        {
            Instructions.Add(new(opcode.Value, operand));

            return this;
        }
    }

    /// <summary>
    /// An instruction, by the value of its opcode, which is the same on every platform, and
    /// its operand: nothing, a number, a string, an LIR type, a CLR method or field, a
    /// method of a class being defined, an LIR function, or a <see cref="ClrLocal"/>.
    /// </summary>
    public readonly record struct ClrInstruction(short OpCode, object? Operand);

    /// <summary>
    /// A local of a <see cref="ClrCode"/>, as an instruction's operand.
    /// </summary>
    public sealed record ClrLocal(int Index);
}
