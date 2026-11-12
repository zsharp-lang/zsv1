// The key of a type in a member's declared signature, by which a CLR handle finds the
// member among the members of a type in other assemblies: overloads differ in it. LIR
// and Cecil give it the same way, a type parameter by its position.

// A type of a CLR handle's signature.
fun keyOf(type: LIRType): String {
    if (type is clr of ClrType) return cecilName(clr);
    if (type is array of ClrArrayType) return keyOf(array.Element) + rankKey(array.Rank);
    if (type is reference of ByRefType) return keyOf(reference.Target) + "&";
    if (type is pointer of ClrPointerType) return keyOf(pointer.Target) + "*";
    if (type is parameter of ClrGenericParameter) return parameterKey(parameter.OfMethod, parameter.Position);
    if (type is instance of GenericInstance) {
        let key = keyOf(instance.Definition) + "<";
        let index = 0;
        while (index < instance.Arguments.Count) {
            if (index > 0) key = key + ",";
            key = key + keyOf(instance.Arguments[index]);
            index = index + 1;
        }
        return key + ">";
    }
    return "?";
}

// A type of a Cecil signature. A modifier, as `in` has, doesn't tell overloads apart.
fun cecilKeyOf(type: TypeReference): String {
    if (type is parameter of GenericParameter) return parameterKey(parameter.Type == GenericParameterType.Method, parameter.Position);
    if (type is instance of GenericInstanceType) {
        let key = cecilKeyOf(instance.ElementType) + "<";
        let index = 0;
        while (index < instance.GenericArguments.Count) {
            if (index > 0) key = key + ",";
            key = key + cecilKeyOf(instance.GenericArguments[index]);
            index = index + 1;
        }
        return key + ">";
    }
    if (type is array of ArrayType) return cecilKeyOf(array.ElementType) + rankKey(array.Rank);
    if (type is reference of ByReferenceType) return cecilKeyOf(reference.ElementType) + "&";
    if (type is pointer of PointerType) return cecilKeyOf(pointer.ElementType) + "*";
    if (type is required of RequiredModifierType) return cecilKeyOf(required.ElementType);
    if (type is optional of OptionalModifierType) return cecilKeyOf(optional.ElementType);
    return type.FullName;
}

// A CLR type's full name as Cecil writes it, a nested type's after a `/`.
fun cecilName(type: ClrType): String {
    if (type.DeclaringType != null) return cecilName(type.DeclaringType) + "/" + type.Name;
    if (type.Namespace == null) return type.Name;
    if (type.Namespace == "") return type.Name;
    return type.Namespace + "." + type.Name;
}

fun rankKey(count: Int32): String {
    let key = "[";
    let index = 1;
    while (index < count) {
        key = key + ",";
        index = index + 1;
    }
    return key + "]";
}

fun parameterKey(ofMethod: Boolean, position: Int32): String {
    if (ofMethod) return "!!" + position;
    return "!" + position;
}
