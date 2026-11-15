// What a module's endpoints are made into. `build` reads what the module's functions say
// they answer (see `endpoints.zs`) and adds to the module, as HIR:
//
// - for each structure an endpoint reaches, a function that reads it from JSON and one that
//   writes it, from the fields its class has;
// - for each endpoint, a function that takes the JSON of its parameter, reads it, calls the
//   endpoint and writes what it returns;
// - `register(server)`, which gives a `Server` each of those, by the method it answers.
//
// It is HIR and nothing more: it names no platform, and any that lowers HIR can run it.
// What the code calls, the reading and writing of strings and numbers, is in `codec.zs`.

// ---- Building code

fun argument(value: Expression): Argument {
    let made = Argument();
    made.Value = value;
    return made;
}

fun call(callee: Expression, arguments: List[Argument]): Call {
    let made = Call();
    made.Callee = callee;
    made.Arguments = arguments;
    return made;
}

fun call1(callee: Expression, first: Expression): Call {
    let arguments = List[Argument]();
    arguments.Add(argument(first));
    return call(callee, arguments);
}

fun call2(callee: Expression, first: Expression, second: Expression): Call {
    let arguments = List[Argument]();
    arguments.Add(argument(first));
    arguments.Add(argument(second));
    return call(callee, arguments);
}

fun call3(callee: Expression, first: Expression, second: Expression, third: Expression): Call {
    let arguments = List[Argument]();
    arguments.Add(argument(first));
    arguments.Add(argument(second));
    arguments.Add(argument(third));
    return call(callee, arguments);
}

fun member(target: Expression, name: String): Expression {
    let made = NamedMember();
    made.Target = target;
    made.Name = name;
    return made;
}

fun text(value: String): Expression { return StringLiteral(value); }

fun giving(value: Expression): Statement {
    let made = Return();
    made.Value = value;
    return made;
}

// What a type or a function is, to refer to in code.
fun instanceOf(node: Object): Instance {
    if (node is let found: Instance) return found;
    return null;
}

fun ref(node: Object): Expression { return Get(instanceOf(node)); }

// A property named for a keyword has a trailing `_`, which isn't part of its name on the
// wire.
fun wireName(name: String): String {
    if (name.EndsWith("_")) return name.Substring(0, name.Length - 1);
    return name;
}

// ---- Generating

def Generator {
    let node: Module;
    let decoders: Dictionary[Object, Function] = Dictionary[Object, Function]();
    let encoders: Dictionary[Object, Function] = Dictionary[Object, Function]();
    let made: Int32 = 0;

    new(this, node: Module) { this.node = node; }

    // Adds a function of one parameter to the module: its name is made unique.
    fun declare(name: String, parameter: String, parameterType: Type, returns: Type): Function {
        made = made + 1;
        let function = Function(name + "$" + made);
        function.Parameters.Add(Positional(parameter, parameterType));
        function.ReturnType = returns;
        function.Body = Code();
        node.Definitions.Add(function);
        return function;
    }

    fun parameterOf(function: Function): Expression { return Get(function.Parameters[0] as Positional); }

    fun isDefinition(type: Type, name: String): Boolean {
        if (type is let generic: GenericInstance) {
            if (generic.Definition is let clr: DotNet_Class) return clr.Name == name;
            if (generic.Definition is let value: DotNet_Struct) return value.Name == name;
        }
        return false;
    }

    // A local, declared in a function with the value it starts with, and what refers to it.
    fun local(function: Function, name: String, type: Type, init: Expression): Local {
        let made = Local(name, type);
        made.Init = init;
        function.Locals.Add(made);
        function.Body.Block.Statements.Add(Declare(made));
        return made;
    }

    // `if (<condition>) return null;`
    fun returnsNullIf(function: Function, condition: Expression) {
        let guard = IfStatement();
        guard.Condition = condition;
        guard.Then = giving(NullLiteral());
        function.Body.Block.Statements.Add(guard);
    }

    // `while (below(index, <count>)) { <body>; index = next(index); }`
    fun loop(function: Function, index: Local, count: Expression, body: Statement) {
        let step = Set();
        step.Target = Get(index);
        step.Value = call1(ref(next as Instance), Get(index));

        let inner = Code().Block;
        inner.Statements.Add(body);
        inner.Statements.Add(ExpressionStatement(step));

        let repeat = While();
        repeat.Condition = call2(ref(below as Instance), Get(index), count);
        repeat.Body = inner;
        function.Body.Block.Statements.Add(repeat);
    }

    // The element of an indexed expression: `<target>[<index>]`.
    fun element(target: Expression, index: Expression): Expression {
        let made = Index();
        made.Target = target;
        made.Arguments.Add(argument(index));
        return made;
    }

    // ---- Reading

    // The code that reads a value of a type from the JSON the expression gives.
    fun decode(type: Type, json: Expression): Expression {
        if (type == (String as Type)) return call1(ref(decodeString as Instance), json);
        if (type == (Int32 as Type)) return call1(ref(decodeInt as Instance), json);
        if (type == (Double as Type)) return call1(ref(decodeDouble as Instance), json);
        if (type == (Boolean as Type)) return call1(ref(decodeBoolean as Instance), json);
        if (type == (Object as Type)) return call1(ref(decodeAny as Instance), json);
        if (isDefinition(type, "System.Nullable`1")) {
            let argument = (type as GenericInstance).Arguments[0];
            if (argument == (Int32 as Type)) return call1(ref(decodeNullableInt as Instance), json);
            if (argument == (Double as Type)) return call1(ref(decodeNullableDouble as Instance), json);
            return call1(ref(decodeNullableBoolean as Instance), json);
        }
        return call1(ref(decoderOf(type)), json);
    }

    // The function that reads a type, which is made once: for a structure, from the fields
    // of its class; for a list or a map, a loop over what it has.
    fun decoderOf(type: Type): Function {
        if (decoders.ContainsKey(type)) return decoders[type];

        let function = declare("decode", "json", JsonValue as Type, type);
        // Registered before it is filled, as a structure can have fields of itself.
        decoders[type] = function;
        let json = parameterOf(function);

        if (type is let structure: Class) {
            returnsNullIf(function, call1(ref(isEmpty as Instance), json));
            let result = local(function, "result", type, call(ref(structure), List[Argument]()));

            let index = 0;
            while (index < structure.Fields.Count) {
                let field = structure.Fields[index];
                if (field.IsStatic == false) {
                    let name = wireName(field.Name);
                    let present = IfStatement();
                    present.Condition = call2(ref(isPresent as Instance), json, text(name));
                    let assign = Set();
                    assign.Target = member(Get(result), field.Name);
                    assign.Value = decode(field.Type, call2(ref(memberOf as Instance), json, text(name)));
                    present.Then = ExpressionStatement(assign);
                    function.Body.Block.Statements.Add(present);
                }
                index = index + 1;
            }
            function.Body.Block.Statements.Add(giving(Get(result)));
        } else if (isDefinition(type, "System.Collections.Generic.List`1")) {
            let generic = type as GenericInstance;
            returnsNullIf(function, call1(ref(isEmpty as Instance), json));
            let result = local(function, "result", type, call(ref(generic), List[Argument]()));
            let index = local(function, "index", Int32 as Type, IntegerLiteral(0));

            let add = ExpressionStatement(call1(member(Get(result), "Add"),
                decode(generic.Arguments[0], call1(member(json, "at"), Get(index)))));
            loop(function, index, member(json, "Count"), add);
            function.Body.Block.Statements.Add(giving(Get(result)));
        } else if (isDefinition(type, "System.Collections.Generic.Dictionary`2")) {
            let generic = type as GenericInstance;
            returnsNullIf(function, call1(ref(isEmpty as Instance), json));
            let result = local(function, "result", type, call(ref(generic), List[Argument]()));
            let index = local(function, "index", Int32 as Type, IntegerLiteral(0));

            let key = call1(member(json, "nameAt"), Get(index));
            let add = ExpressionStatement(call2(member(Get(result), "Add"), key,
                decode(generic.Arguments[1], call1(member(json, "member"), call1(member(json, "nameAt"), Get(index))))));
            loop(function, index, member(json, "Count"), add);
            function.Body.Block.Statements.Add(giving(Get(result)));
        } else {
            function.Body.Block.Statements.Add(giving(call1(ref(decodeAny as Instance), json)));
        }
        return function;
    }

    // ---- Writing

    fun encode(type: Type, value: Expression): Expression {
        if (type == (String as Type)) return call1(ref(encodeString as Instance), value);
        if (type == (Int32 as Type)) return call1(ref(encodeInt as Instance), value);
        if (type == (Double as Type)) return call1(ref(encodeDouble as Instance), value);
        if (type == (Boolean as Type)) return call1(ref(encodeBoolean as Instance), value);
        if (type == (Object as Type)) return call1(ref(encodeAny as Instance), value);
        if (isDefinition(type, "System.Nullable`1")) {
            let argument = (type as GenericInstance).Arguments[0];
            if (argument == (Int32 as Type)) return call1(ref(encodeNullableInt as Instance), value);
            if (argument == (Double as Type)) return call1(ref(encodeNullableDouble as Instance), value);
            return call1(ref(encodeNullableBoolean as Instance), value);
        }
        return call1(ref(encoderOf(type)), value);
    }

    fun encoderOf(type: Type): Function {
        if (encoders.ContainsKey(type)) return encoders[type];

        let function = declare("encode", "value", type, JsonValue as Type);
        encoders[type] = function;
        let value = parameterOf(function);

        if (type is let structure: Class) {
            returnsNullIf(function, call1(ref(isNothing as Instance), value));
            let result = local(function, "result", JsonValue as Type, call(ref(newObject as Instance), List[Argument]()));

            let index = 0;
            while (index < structure.Fields.Count) {
                let field = structure.Fields[index];
                if (field.IsStatic == false) {
                    let written = encode(field.Type, member(value, field.Name));
                    function.Body.Block.Statements.Add(ExpressionStatement(
                        call3(ref(putMember as Instance), Get(result), text(wireName(field.Name)), written)));
                }
                index = index + 1;
            }
            function.Body.Block.Statements.Add(giving(Get(result)));
        } else if (isDefinition(type, "System.Collections.Generic.List`1")) {
            let generic = type as GenericInstance;
            returnsNullIf(function, call1(ref(isNothing as Instance), value));
            let result = local(function, "result", JsonValue as Type, call(ref(newArray as Instance), List[Argument]()));
            let index = local(function, "index", Int32 as Type, IntegerLiteral(0));

            let add = ExpressionStatement(call1(member(Get(result), "add"),
                encode(generic.Arguments[0], element(value, Get(index)))));
            loop(function, index, member(value, "Count"), add);
            function.Body.Block.Statements.Add(giving(Get(result)));
        } else if (isDefinition(type, "System.Collections.Generic.Dictionary`2")) {
            let generic = type as GenericInstance;
            returnsNullIf(function, call1(ref(isNothing as Instance), value));
            let result = local(function, "result", JsonValue as Type, call(ref(newObject as Instance), List[Argument]()));
            let keys = local(function, "keys", List[String] as Type, call1(ref(keysOf as Instance), value));
            let index = local(function, "index", Int32 as Type, IntegerLiteral(0));

            let key = element(Get(keys), Get(index));
            let put = ExpressionStatement(call2(member(Get(result), "put"), key,
                encode(generic.Arguments[1], element(value, element(Get(keys), Get(index))))));
            loop(function, index, member(Get(keys), "Count"), put);
            function.Body.Block.Statements.Add(giving(Get(result)));
        } else {
            function.Body.Block.Statements.Add(giving(call1(ref(encodeAny as Instance), value)));
        }
        return function;
    }

    // ---- Endpoints

    // The function that answers an endpoint with the JSON of its parameter: reads it, calls
    // the endpoint, and writes what it gives.
    fun wrap(endpoint: Function, annotation: Endpoint): Function {
        let function = declare("endpoint", "json", JsonValue as Type, JsonValue as Type);
        let code = function.Body.Block.Statements;

        let arguments = List[Argument]();
        if (endpoint.Parameters.Count > 0) {
            let parameter = endpoint.Parameters[0] as Positional;
            arguments.Add(argument(decode(parameter.Type, parameterOf(function))));
        }
        let invoked = call(Get(endpoint), arguments);

        if (annotation.IsRequest) {
            if (endpoint.ReturnType == null) {
                code.Add(ExpressionStatement(invoked));
                code.Add(giving(NullLiteral()));
            } else {
                code.Add(giving(encode(endpoint.ReturnType, invoked)));
            }
        } else {
            code.Add(ExpressionStatement(invoked));
            code.Add(giving(NullLiteral()));
        }
        return function;
    }
}

// Adds what a module's endpoints need to it, and gives it.
fun build(node: Module): Module {
    let generator = Generator(node);

    let register = Function("register");
    register.Parameters.Add(Positional("server", Server as Type));
    register.Body = Code();
    register.Body.Block.Statements.Add(ExpressionStatement(call1(ref(use as Instance), Get(register.Parameters[0] as Positional))));

    let index = 0;
    let count = node.Definitions.Count;
    while (index < count) {
        if (node.Definitions[index] is let function: Function) {
            let at = 0;
            while (at < function.Annotations.Count) {
                if (function.Annotations[at] is let endpoint: Endpoint) {
                    let wrapper = generator.wrap(function, endpoint);
                    let registered = "onNotification";
                    if (endpoint.IsRequest) registered = "onRequest";
                    let server = Get(register.Parameters[0] as Positional);
                    register.Body.Block.Statements.Add(ExpressionStatement(
                        call2(member(server, registered), text(endpoint.Method), Get(wrapper))));
                }
                at = at + 1;
            }
        }
        index = index + 1;
    }

    node.Definitions.Add(register);
    return node;
}
