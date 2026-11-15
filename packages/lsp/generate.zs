// Generates pkg:lsp's types from the LSP specification's own machine-readable form,
// `metaModel.json`: a class for each structure and each literal structure in one, a class
// of constants for each enumeration, and a table of the requests and notifications.
//
//     zs packages/lsp/generate.zs
//
// Run it from the toolchain's directory. It writes `packages/lsp/src/generated`, which is
// checked in, so a project that imports `pkg:lsp` needs neither the generator nor the model.
//
// What the model says that a Z# class can't yet, is kept as a comment: Z# has no unions, so
// a field of a union is an `Object` with the union written beside it, and no inheritance,
// so a structure has the properties of what it extends and mixes in. A property whose name
// is a keyword of Z# is written with a trailing `_`.
import "std:prelude";
import { print } from "std:io";
import { Object, String, Int32, Boolean, List, Dictionary } from "core:runtime";
import { Path, File, Directory, StringBuilder, Environment } from Object.Assembly;
import { JsonDocument, JsonElement } from Path.Combine(Path.GetDirectoryName(Object.Assembly.Location), "System.Text.Json.dll");

module Generator {

let model = JsonDocument.Parse(File.ReadAllText("packages/lsp/metaModel.json")).RootElement;

// What the model has, by name.
let structures = Dictionary[String, JsonElement]();
let enumerations = Dictionary[String, JsonElement]();
let aliases = Dictionary[String, JsonElement]();

// The Z# type an alias is, found once.
let aliased = Dictionary[String, String]();

// The classes of the literal structures found while the types were written, and the names
// they were given.
let literals = StringBuilder();
let literalNames = Dictionary[String, Int32]();

let keywords: String[] = [
    "as", "break", "case", "def", "continue", "else", "for", "from", "fun", "get", "if",
    "import", "in", "is", "let", "module", "new", "of", "or", "return", "set", "var", "when", "while"
];

fun has(e: JsonElement, name: String): Boolean {
    return e.TryGetProperty(name, let value: JsonElement);
}

fun text(e: JsonElement, name: String): String {
    return e.GetProperty(name).GetString();
}

// A string property that may be absent, or null.
fun optional(e: JsonElement, name: String): String {
    if (has(e, name)) return e.GetProperty(name).GetString();
    return null;
}

fun flag(e: JsonElement, name: String): Boolean {
    if (has(e, name)) return e.GetProperty(name).GetBoolean();
    return false;
}

fun count(e: JsonElement): Int32 { return e.GetArrayLength(); }

fun capitalized(name: String): String {
    return name.Substring(0, 1).ToUpper() + name.Substring(1);
}

fun isKeyword(name: String): Boolean {
    let index = 0;
    while (index < keywords.Length) {
        if (keywords[index] == name) return true;
        index = index + 1;
    }
    return false;
}

fun fieldName(name: String): String {
    if (isKeyword(name)) return name + "_";
    return name;
}

// What a documentation comment is written as: each of its lines.
fun comment(into: StringBuilder, indent: String, documentation: String) {
    if (documentation == null) return;
    let rest = documentation.Replace("\r", "");
    while (rest != null) {
        let end = rest.IndexOf("\n");
        let line = rest;
        if (end >= 0) {
            line = rest.Substring(0, end);
            rest = rest.Substring(end + 1);
        } else {
            rest = null;
        }
        if (line == "") {
            into.Append(indent + "//\n");
        } else {
            into.Append(indent + "// " + line + "\n");
        }
    }
}

// The type as the specification writes it, for the comments and the table.
fun written(t: JsonElement): String {
    let kind = text(t, "kind");
    if (kind == "base") return text(t, "name");
    if (kind == "reference") return text(t, "name");
    if (kind == "array") {
        let element = t.GetProperty("element");
        let inner = written(element);
        let elementKind = text(element, "kind");
        if (elementKind == "or") return "(" + inner + ")[]";
        if (elementKind == "and") return "(" + inner + ")[]";
        return inner + "[]";
    }
    if (kind == "map") return "{ [key: " + written(t.GetProperty("key")) + "]: " + written(t.GetProperty("value")) + " }";
    if (kind == "or") return joined(t.GetProperty("items"), " | ");
    if (kind == "and") return joined(t.GetProperty("items"), " & ");
    if (kind == "tuple") return "[" + joined(t.GetProperty("items"), ", ") + "]";
    if (kind == "literal") return "{ ... }";
    if (kind == "stringLiteral") return "'" + text(t, "value") + "'";
    if (kind == "integerLiteral") return t.GetProperty("value").ToString();
    if (kind == "booleanLiteral") return t.GetProperty("value").ToString();
    return "?";
}

fun joined(items: JsonElement, separator: String): String {
    let result = "";
    let index = 0;
    while (index < count(items)) {
        if (index > 0) result = result + separator;
        result = result + written(items[index]);
        index = index + 1;
    }
    return result;
}

fun isNull(t: JsonElement): Boolean {
    if (text(t, "kind") == "base") return text(t, "name") == "null";
    return false;
}

// Whether the type says it can be null, as `string | null` does.
fun nullable(t: JsonElement): Boolean {
    if (text(t, "kind") != "or") return false;
    let items = t.GetProperty("items");
    let index = 0;
    while (index < count(items)) {
        if (isNull(items[index])) return true;
        index = index + 1;
    }
    return false;
}

fun isValueType(type: String): Boolean {
    if (type == "Int32") return true;
    if (type == "Boolean") return true;
    if (type == "Double") return true;
    return false;
}

fun defaultOf(type: String): String {
    if (type == "Int32") return "0";
    if (type == "Boolean") return "false";
    if (type == "Double") return "0.0";
    return "null";
}

// The name of a literal structure's class, which nothing else has.
fun literalName(owner: String, hint: String): String {
    let name = owner + capitalized(hint);
    if (literalNames.ContainsKey(name)) {
        let next = literalNames[name] + 1;
        literalNames[name] = next;
        return name + next;
    }
    literalNames[name] = 1;
    if (structures.ContainsKey(name)) return name + "Literal";
    return name;
}

// The Z# type of a type of the model. A literal structure gets a class, named by what holds
// it and the property it is the type of.
fun zType(t: JsonElement, owner: String, hint: String): String {
    let kind = text(t, "kind");
    if (kind == "base") {
        let name = text(t, "name");
        if (name == "integer") return "Int32";
        if (name == "uinteger") return "Int32";
        if (name == "decimal") return "Double";
        if (name == "boolean") return "Boolean";
        if (name == "null") return "Object";
        return "String";
    }
    if (kind == "reference") {
        let name = text(t, "name");
        if (enumerations.ContainsKey(name)) {
            if (text(enumerations[name].GetProperty("type"), "name") == "string") return "String";
            return "Int32";
        }
        if (aliases.ContainsKey(name)) return aliasType(name);
        return name;
    }
    if (kind == "array") return "List[" + zType(t.GetProperty("element"), owner, hint + "Item") + "]";
    if (kind == "map") return "Dictionary[String, " + zType(t.GetProperty("value"), owner, hint + "Value") + "]";
    if (kind == "stringLiteral") return "String";
    if (kind == "integerLiteral") return "Int32";
    if (kind == "booleanLiteral") return "Boolean";
    if (kind == "literal") return literalClass(t.GetProperty("value"), owner, hint);
    if (kind == "or") {
        let items = t.GetProperty("items");
        let present = 0;
        let single = 0;
        let index = 0;
        while (index < count(items)) {
            if (isNull(items[index]) == false) {
                present = present + 1;
                single = index;
            }
            index = index + 1;
        }
        if (present == 1) return zType(items[single], owner, hint);
        return sharedType(items, owner, hint);
    }
    if (kind == "tuple") {
        let items = t.GetProperty("items");
        return "List[" + sharedType(items, owner, hint) + "]";
    }
    return "Object";
}

// The one type every non-null item is, or `Object`. A literal structure among several
// types has no class: the union is written in a comment.
fun sharedType(items: JsonElement, owner: String, hint: String): String {
    let result: String = null;
    let index = 0;
    while (index < count(items)) {
        let item = items[index];
        if (isNull(item) == false) {
            let kind = text(item, "kind");
            if (kind == "literal") return "Object";
            if (kind == "or") return "Object";
            if (kind == "and") return "Object";
            let mapped = zType(item, owner, hint);
            if (result == null) {
                result = mapped;
            } else if (result != mapped) {
                return "Object";
            }
        }
        index = index + 1;
    }
    if (result == null) return "Object";
    return result;
}

fun aliasType(name: String): String {
    if (aliased.ContainsKey(name)) return aliased[name];
    // A recursive alias, as LSPAny is, stops at Object.
    aliased[name] = "Object";
    let result = zType(aliases[name].GetProperty("type"), name, "");
    aliased[name] = result;
    return result;
}

// The properties of a structure and of what it extends and mixes in, in that order, the
// nearest overriding.
fun collect(structure: JsonElement, properties: List[JsonElement], index: Dictionary[String, Int32]) {
    let parents = List[JsonElement]();
    if (has(structure, "extends")) {
        let extended = structure.GetProperty("extends");
        let at = 0;
        while (at < count(extended)) { parents.Add(extended[at]); at = at + 1; }
    }
    if (has(structure, "mixins")) {
        let mixed = structure.GetProperty("mixins");
        let at = 0;
        while (at < count(mixed)) { parents.Add(mixed[at]); at = at + 1; }
    }

    let parent = 0;
    while (parent < parents.Count) {
        collect(structures[text(parents[parent], "name")], properties, index);
        parent = parent + 1;
    }

    let own = structure.GetProperty("properties");
    let at = 0;
    while (at < count(own)) {
        let property = own[at];
        let name = text(property, "name");
        if (index.ContainsKey(name)) {
            properties[index[name]] = property;
        } else {
            index[name] = properties.Count;
            properties.Add(property);
        }
        at = at + 1;
    }
}

// A class with the given properties, which a structure or a literal structure is.
fun classOf(name: String, documentation: String, source: String, properties: List[JsonElement]): String {
    let result = StringBuilder();
    comment(result, "", documentation);
    if (source != null) result.Append("// (" + source + ")\n");
    result.Append("def " + name + " {\n");

    let at = 0;
    while (at < properties.Count) {
        let property = properties[at];
        let propertyType = property.GetProperty("type");
        let type = zType(propertyType, name, text(property, "name"));
        let canBeNull = flag(property, "optional");
        if (nullable(propertyType)) canBeNull = true;

        let declared = type;
        let initial = defaultOf(type);
        if (isValueType(type)) {
            if (canBeNull) {
                declared = "Nullable[" + type + "]";
                initial = "null";
            }
        }

        comment(result, "    ", optional(property, "documentation"));
        let note = written(propertyType);
        if (note != type) result.Append("    // " + note + "\n");
        result.Append("    let " + fieldName(text(property, "name")) + ": " + declared + " = " + initial + ";\n");
        at = at + 1;
    }

    result.Append("    new() { }\n");
    result.Append("}\n\n");
    return result.ToString();
}

// A literal structure's class, written into `literals`.
fun literalClass(literal: JsonElement, owner: String, hint: String): String {
    let name = literalName(owner, hint);
    let properties = List[JsonElement]();
    let own = literal.GetProperty("properties");
    let at = 0;
    while (at < count(own)) { properties.Add(own[at]); at = at + 1; }
    literals.Append(classOf(name, null, "a literal structure of " + owner, properties));
    return name;
}

fun enumerationOf(enumeration: JsonElement): String {
    let name = text(enumeration, "name");
    let type = "Int32";
    if (text(enumeration.GetProperty("type"), "name") == "string") type = "String";

    let result = StringBuilder();
    comment(result, "", optional(enumeration, "documentation"));
    result.Append("def " + name + " {\n");
    let values = enumeration.GetProperty("values");
    let at = 0;
    while (at < count(values)) {
        let member = values[at];
        comment(result, "    ", optional(member, "documentation"));
        let value = member.GetProperty("value").ToString();
        if (type == "String") value = "\"" + value + "\"";
        result.Append("    @static let " + text(member, "name") + ": " + type + " = " + value + ";\n");
        at = at + 1;
    }
    result.Append("    new() { }\n");
    result.Append("}\n\n");
    return result.ToString();
}

fun quoted(value: String): String {
    if (value == null) return "null";
    return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}

// What a request or a notification is, as the table has it.
fun methodOf(message: JsonElement, kind: String): String {
    let paramsType: String = null;
    if (has(message, "params")) {
        let params = message.GetProperty("params");
        // Several parameter types, as a partial result has, are written as they are.
        paramsType = written(params);
    }
    let resultType: String = null;
    let partial: String = null;
    let errorData: String = null;
    if (has(message, "result")) resultType = written(message.GetProperty("result"));
    if (has(message, "partialResult")) partial = written(message.GetProperty("partialResult"));
    if (has(message, "errorData")) errorData = written(message.GetProperty("errorData"));
    let options: String = null;
    if (has(message, "registrationOptions")) options = written(message.GetProperty("registrationOptions"));

    return "    all.Add(MethodInfo(" + quoted(text(message, "method")) + ", " + quoted(kind) + ", "
        + quoted(text(message, "messageDirection")) + ", " + quoted(paramsType) + ", "
        + quoted(resultType) + ", " + quoted(partial) + ", " + quoted(errorData) + ", "
        + quoted(options) + ", " + quoted(optional(message, "registrationMethod")) + "));\n";
}

// Reads what the model has.
fun readInto(name: String, into: Dictionary[String, JsonElement]) {
    let items = model.GetProperty(name);
    let at = 0;
    while (at < count(items)) {
        into[text(items[at], "name")] = items[at];
        at = at + 1;
    }
}

fun run() {
    readInto("structures", structures);
    readInto("enumerations", enumerations);
    readInto("typeAliases", aliases);

    let structureCode = StringBuilder();
    structureCode.Append("// Generated from the LSP " + model.GetProperty("metaData").GetProperty("version").GetString() + " model by packages/lsp/generate.zs. Do not edit.\n");
    structureCode.Append("// The structures of the protocol: a class for each, with the properties of what it extends.\n\n");

    let all = model.GetProperty("structures");
    let at = 0;
    while (at < count(all)) {
        let structure = all[at];
        let properties = List[JsonElement]();
        collect(structure, properties, Dictionary[String, Int32]());
        structureCode.Append(classOf(text(structure, "name"), optional(structure, "documentation"), null, properties));
        at = at + 1;
    }
    structureCode.Append("// Literal structures the protocol writes inline, each named by what holds it.\n\n");
    structureCode.Append(literals.ToString());

    let enumerationCode = StringBuilder();
    enumerationCode.Append("// Generated from the LSP " + model.GetProperty("metaData").GetProperty("version").GetString() + " model by packages/lsp/generate.zs. Do not edit.\n");
    enumerationCode.Append("// The enumerations of the protocol: a class of constants for each.\n\n");
    let enums = model.GetProperty("enumerations");
    at = 0;
    while (at < count(enums)) {
        enumerationCode.Append(enumerationOf(enums[at]));
        at = at + 1;
    }

    let methodCode = StringBuilder();
    methodCode.Append("// Generated from the LSP " + model.GetProperty("metaData").GetProperty("version").GetString() + " model by packages/lsp/generate.zs. Do not edit.\n");
    methodCode.Append("// Each request and notification of the protocol.\n\n");
    methodCode.Append("fun everyMethod(): List[MethodInfo] {\n    let all = List[MethodInfo]();\n");
    let requests = model.GetProperty("requests");
    at = 0;
    while (at < count(requests)) {
        methodCode.Append(methodOf(requests[at], "request"));
        at = at + 1;
    }
    let notifications = model.GetProperty("notifications");
    at = 0;
    while (at < count(notifications)) {
        methodCode.Append(methodOf(notifications[at], "notification"));
        at = at + 1;
    }
    methodCode.Append("    return all;\n}\n\n");
    methodCode.Append("let methods: List[MethodInfo] = everyMethod();\n");

    Directory.CreateDirectory("packages/lsp/src/generated");
    File.WriteAllText("packages/lsp/src/generated/structures.zs", structureCode.ToString());
    File.WriteAllText("packages/lsp/src/generated/enumerations.zs", enumerationCode.ToString());
    File.WriteAllText("packages/lsp/src/generated/methods.zs", methodCode.ToString());
    print("wrote " + count(all) + " structures, " + count(enums) + " enumerations and " + (count(requests) + count(notifications)) + " methods");
}
}

Generator.run();
