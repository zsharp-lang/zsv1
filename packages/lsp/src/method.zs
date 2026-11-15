// A request or a notification of the protocol, as the specification describes it. The
// types are written as the specification writes them, as `Hover | null`, since a Z# type
// can't be a union yet; a name of a structure is the name of its class.
def MethodInfo {
    // The name on the wire, as `textDocument/hover`.
    let name: String = null;
    // `request` or `notification`.
    let kind: String = null;
    // `clientToServer`, `serverToClient` or `both`.
    let direction: String = null;
    let paramsType: String = null;
    let resultType: String = null;
    let partialResultType: String = null;
    let errorDataType: String = null;
    // The options a registration of it takes, if it can be registered.
    let registrationOptions: String = null;
    // The method a registration is sent as, if it is not its own.
    let registrationMethod: String = null;

    new(this, name: String, kind: String, direction: String, paramsType: String, resultType: String,
        partialResultType: String, errorDataType: String, registrationOptions: String,
        registrationMethod: String) {
        this.name = name;
        this.kind = kind;
        this.direction = direction;
        this.paramsType = paramsType;
        this.resultType = resultType;
        this.partialResultType = partialResultType;
        this.errorDataType = errorDataType;
        this.registrationOptions = registrationOptions;
        this.registrationMethod = registrationMethod;
    }

    get IsRequest(): Boolean { return kind == "request"; }
}

// The method of the protocol of a name, or null.
fun find(name: String): MethodInfo {
    let index = 0;
    while (index < methods.Count) {
        if (methods[index].name == name) return methods[index];
        index = index + 1;
    }
    return null;
}
