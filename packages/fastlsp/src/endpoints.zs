// What a function says it answers: an annotation, as std:framework's declarations are.
// A decorator only says it: `build` reads what a module's functions say and writes the
// code that makes them endpoints.
//
//     @request("textDocument/hover")
//     fun hover(params: HoverParams): Hover { ... }
//
// The parameter and return types are the protocol's own; a notification returns nothing.

// Says something of a definition that can be annotated, and gives the definition.
fun say(member: Instance, annotation: Object): Instance {
    if (member is let annotated: Annotated) annotated.Annotations.Add(annotation);
    return member;
}

// A function that answers a request or a notification: its method, as the protocol names
// it, and whether it is a request, which has a result, or a notification, which has none.
//
// It is a CLR class, as an annotation has to be one: the list of what is said of a
// definition holds objects, and a Z# class can't be told from an `Object` yet.
@public def Endpoint of DotNet_Class {
    @public let Method: String;
    @public let IsRequest: Boolean;

    @public @explicitThis new(this, method: String, isRequest: Boolean) {
        this.Method = method;
        this.IsRequest = isRequest;
    }

    // As a decorator: says it of the function it is given.
    @public @explicitThis fun apply(this, member: Instance): Instance { return say(member, this); }
}

// Marks a function as answering the request of a method.
fun request(method: String): Func[Instance, Instance] { return Endpoint(method, true).apply; }

// Marks a function as handling the notification of a method.
fun notification(method: String): Func[Instance, Instance] { return Endpoint(method, false).apply; }
