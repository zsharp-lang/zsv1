// What the code `build` generates calls: reading a value of JSON into a type and writing
// one out. The code that knows a structure's fields is generated, for each structure an
// endpoint reaches; what it leaves to these is only what has no structure of its own, as
// a string, a number or a list of any element.
//
// A missing member, and a null one, leave a field as it was made: null, or zero.

// ---- Reading

// Whether an object has a member that isn't null.
fun isPresent(json: JsonValue, name: String): Boolean {
    let found = json.member(name);
    if (found == null) return false;
    if (found.IsNull) return false;
    return true;
}

fun memberOf(json: JsonValue, name: String): JsonValue { return json.member(name); }

fun isEmpty(json: JsonValue): Boolean {
    if (json == null) return true;
    return json.IsNull;
}

fun decodeString(json: JsonValue): String {
    if (isEmpty(json)) return null;
    return json.asString();
}

fun decodeInt(json: JsonValue): Int32 {
    if (isEmpty(json)) return 0;
    return json.asInt();
}

fun decodeDouble(json: JsonValue): Double {
    if (isEmpty(json)) return 0.0;
    return json.asDouble();
}

fun decodeBoolean(json: JsonValue): Boolean {
    if (isEmpty(json)) return false;
    return json.asBoolean();
}

fun decodeNullableInt(json: JsonValue): Nullable[Int32] {
    if (isEmpty(json)) return null;
    return json.asInt();
}

fun decodeNullableDouble(json: JsonValue): Nullable[Double] {
    if (isEmpty(json)) return null;
    return json.asDouble();
}

fun decodeNullableBoolean(json: JsonValue): Nullable[Boolean] {
    if (isEmpty(json)) return null;
    return json.asBoolean();
}

// A value of a type that is none of a structure, as a union is, is the JSON itself.
fun decodeAny(json: JsonValue): Object {
    if (isEmpty(json)) return null;
    return json;
}

// ---- Writing

fun newObject(): JsonValue { return JsonValue.object(); }

// Sets an object's member, unless there is no value for it.
fun putMember(target: JsonValue, name: String, value: JsonValue) {
    if (value != null) target.put(name, value);
}

fun isNothing(value: Object): Boolean { return value == null; }

fun encodeString(value: String): JsonValue {
    if (value == null) return null;
    return JsonValue.ofString(value);
}

fun encodeInt(value: Int32): JsonValue { return JsonValue.ofNumber(value); }

fun encodeDouble(value: Double): JsonValue { return JsonValue.ofNumber(value); }

fun encodeBoolean(value: Boolean): JsonValue { return JsonValue.ofBoolean(value); }

fun encodeNullableInt(value: Nullable[Int32]): JsonValue {
    if (value.HasValue == false) return null;
    return JsonValue.ofNumber(value.Value);
}

fun encodeNullableDouble(value: Nullable[Double]): JsonValue {
    if (value.HasValue == false) return null;
    return JsonValue.ofNumber(value.Value);
}

fun encodeNullableBoolean(value: Nullable[Boolean]): JsonValue {
    if (value.HasValue == false) return null;
    return JsonValue.ofBoolean(value.Value);
}

// What is JSON already is written as it is, and so is what JSON has a kind for: a string, a
// number or a boolean. Anything else, which a union's value could be, has no JSON to write
// here, as a Z# class can't be told from an `Object` yet.
fun encodeAny(value: Object): JsonValue {
    if (value == null) return null;
    if (value is let text: String) return JsonValue.ofString(text);
    if (value is let number: Int32) return JsonValue.ofNumber(number);
    if (value is let number: Double) return JsonValue.ofNumber(number);
    if (value is let flag: Boolean) return JsonValue.ofBoolean(flag);
    if (value.GetType().Name == "JsonValue") return Unsafe.As[JsonValue](value);
    return null;
}

// ---- Collections

// The code generated for a list or a map is a loop, written as HIR with the concrete
// types it reaches, so it needs no generic function: what it asks of these is the little
// that is the same for all of them.

fun below(index: Int32, count: Int32): Boolean { return index < count; }

fun next(index: Int32): Int32 { return index + 1; }

// The keys of any map, as the names a JSON object has.
fun keysOf(map: IDictionary): List[String] {
    let result = List[String]();
    if (map == null) return result;
    let keys = map.Keys.GetEnumerator();
    while (keys.MoveNext()) {
        result.Add(keys.Current as String);
    }
    return result;
}

fun newArray(): JsonValue { return JsonValue.array(); }
