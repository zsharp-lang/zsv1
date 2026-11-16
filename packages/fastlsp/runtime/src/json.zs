// JSON, in pure Z#: a value, a parser and a writer. What a server reads and writes is
// JSON, and the code fastlsp generates takes and gives these values, so nothing of the
// runtime's own JSON is in it: any runtime that runs Z# has it.
//
// A value is one of six kinds, as JSON has: null (0), boolean (1), number (2), string (3),
// array (4) and object (5). An object keeps its members in the order they were added.

def JsonValue {
    let kind: Int32 = 0;
    let boolean: Boolean = false;
    let number: Double = 0.0;
    let text: String = null;
    let items: List[JsonValue] = null;
    let keys: List[String] = null;
    let members: Dictionary[String, JsonValue] = null;

    new() { }

    new ofBoolean(this, value: Boolean) { this.kind = 1; this.boolean = value; }
    new ofNumber(this, value: Double) { this.kind = 2; this.number = value; }
    new ofString(this, value: String) { this.kind = 3; this.text = value; }
    new array(this) { this.kind = 4; this.items = List[JsonValue](); }
    new object(this) {
        this.kind = 5;
        this.keys = List[String]();
        this.members = Dictionary[String, JsonValue]();
    }

    get IsNull(): Boolean { return kind == 0; }
    get IsBoolean(): Boolean { return kind == 1; }
    get IsNumber(): Boolean { return kind == 2; }
    get IsString(): Boolean { return kind == 3; }
    get IsArray(): Boolean { return kind == 4; }
    get IsObject(): Boolean { return kind == 5; }

    // The elements of an array, or the members of an object.
    get Count(): Int32 {
        if (kind == 4) return items.Count;
        if (kind == 5) return keys.Count;
        return 0;
    }

    // An array's element.
    fun at(index: Int32): JsonValue { return items[index]; }
    fun add(value: JsonValue) { items.Add(value); }

    // An object's member, or null where it has none.
    fun member(name: String): JsonValue {
        if (kind != 5) return null;
        if (members.ContainsKey(name)) return members[name];
        return null;
    }

    fun has(name: String): Boolean {
        if (kind != 5) return false;
        return members.ContainsKey(name);
    }

    fun put(name: String, value: JsonValue) {
        if (members.ContainsKey(name) == false) keys.Add(name);
        members[name] = value;
    }

    // An object's names, in the order they were added.
    fun nameAt(index: Int32): String { return keys[index]; }

    fun asString(): String {
        if (kind == 3) return text;
        return null;
    }

    fun asInt(): Int32 {
        if (kind == 2) return Convert.ToInt32(number);
        return 0;
    }

    fun asDouble(): Double {
        if (kind == 2) return number;
        return 0.0;
    }

    fun asBoolean(): Boolean {
        if (kind == 1) return boolean;
        return false;
    }

    fun toString(this): String { return writeJson(this); }
}

// ---- Writing

fun writeString(into: StringBuilder, value: String) {
    into.Append("\"");
    let index = 0;
    while (index < value.Length) {
        let code = Convert.ToInt32(value[index]);
        if (code == 34) {
            into.Append("\\\"");
        } else if (code == 92) {
            into.Append("\\\\");
        } else if (code == 10) {
            into.Append("\\n");
        } else if (code == 13) {
            into.Append("\\r");
        } else if (code == 9) {
            into.Append("\\t");
        } else if (code < 32) {
            into.Append("\\u" + code.ToString("x4"));
        } else {
            into.Append(value.Substring(index, 1));
        }
        index = index + 1;
    }
    into.Append("\"");
}

fun writeNumber(into: StringBuilder, value: Double) {
    let whole = Math.Floor(value);
    if (whole == value) {
        if (Math.Abs(value) < 9000000000000000.0) {
            into.Append(Convert.ToInt64(value).ToString());
            return;
        }
    }
    into.Append(value.ToString("R", CultureInfo.InvariantCulture));
}

fun writeInto(into: StringBuilder, value: JsonValue) {
    if (value == null) {
        into.Append("null");
    } else if (value.kind == 0) {
        into.Append("null");
    } else if (value.kind == 1) {
        if (value.boolean) {
            into.Append("true");
        } else {
            into.Append("false");
        }
    } else if (value.kind == 2) {
        writeNumber(into, value.number);
    } else if (value.kind == 3) {
        writeString(into, value.text);
    } else if (value.kind == 4) {
        into.Append("[");
        let index = 0;
        while (index < value.items.Count) {
            if (index > 0) into.Append(",");
            writeInto(into, value.items[index]);
            index = index + 1;
        }
        into.Append("]");
    } else {
        into.Append("{");
        let index = 0;
        while (index < value.keys.Count) {
            if (index > 0) into.Append(",");
            writeString(into, value.keys[index]);
            into.Append(":");
            writeInto(into, value.members[value.keys[index]]);
            index = index + 1;
        }
        into.Append("}");
    }
}

// The text of a value.
fun writeJson(value: JsonValue): String {
    let into = StringBuilder();
    writeInto(into, value);
    return into.ToString();
}

// ---- Parsing

fun isDigit(code: Int32): Boolean {
    if (code < 48) return false;
    if (code > 57) return false;
    return true;
}

fun isNumberCharacter(code: Int32): Boolean {
    if (isDigit(code)) return true;
    return code == 45 or code == 43 or code == 46 or code == 101 or code == 69;
}

// Reads a value from text. If the text isn't JSON, `parse` gives null and `error` says why.
def JsonParser {
    let text: String;
    let position: Int32 = 0;
    let error: String = null;

    new(this, text: String) { this.text = text; }

    fun code(): Int32 {
        if (position >= text.Length) return -1;
        return Convert.ToInt32(text[position]);
    }

    fun fail(message: String): JsonValue {
        if (error == null) error = message + " at " + position;
        return null;
    }

    fun skip() {
        let current = code();
        while (current == 32 or current == 9 or current == 10 or current == 13) {
            position = position + 1;
            current = code();
        }
    }

    // The value the whole text is, or null, in which case `error` is set.
    fun parse(): JsonValue {
        let value = parseValue();
        if (value == null) return null;
        skip();
        if (position < text.Length) return fail("Unexpected text after the value");
        return value;
    }

    fun parseValue(): JsonValue {
        skip();
        let current = code();
        if (current == 123) return parseObject();
        if (current == 91) return parseArray();
        if (current == 34) {
            let read = parseString();
            if (read == null) return null;
            return JsonValue.ofString(read);
        }
        if (current == 116) return literal("true", JsonValue.ofBoolean(true));
        if (current == 102) return literal("false", JsonValue.ofBoolean(false));
        if (current == 110) return literal("null", JsonValue());
        if (current == 45 or isDigit(current)) return parseNumber();
        return fail("Unexpected character");
    }

    fun literal(word: String, value: JsonValue): JsonValue {
        if (position + word.Length > text.Length) return fail("Unexpected end");
        if (text.Substring(position, word.Length) != word) return fail("Unexpected word");
        position = position + word.Length;
        return value;
    }

    fun parseNumber(): JsonValue {
        let start = position;
        while (isNumberCharacter(code())) { position = position + 1; }
        let parsed = Double.Parse(text.Substring(start, position - start), CultureInfo.InvariantCulture);
        return JsonValue.ofNumber(parsed);
    }

    // A string's text, with its quotes read.
    fun parseString(): String {
        position = position + 1;
        let into = StringBuilder();
        let current = code();
        while (current != 34) {
            if (current < 0) {
                fail("Unterminated string");
                return null;
            }
            if (current == 92) {
                position = position + 1;
                let escaped = code();
                if (escaped == 110) {
                    into.Append("\n");
                } else if (escaped == 114) {
                    into.Append("\r");
                } else if (escaped == 116) {
                    into.Append("\t");
                } else if (escaped == 98) {
                    into.Append(Convert.ToChar(8).ToString());
                } else if (escaped == 102) {
                    into.Append(Convert.ToChar(12).ToString());
                } else if (escaped == 117) {
                    if (position + 4 >= text.Length) {
                        fail("Unterminated escape");
                        return null;
                    }
                    into.Append(Convert.ToChar(Convert.ToInt32(text.Substring(position + 1, 4), 16)).ToString());
                    position = position + 4;
                } else {
                    into.Append(text.Substring(position, 1));
                }
            } else {
                into.Append(text.Substring(position, 1));
            }
            position = position + 1;
            current = code();
        }
        position = position + 1;
        return into.ToString();
    }

    fun parseArray(): JsonValue {
        position = position + 1;
        let result = JsonValue.array();
        skip();
        if (code() == 93) {
            position = position + 1;
            return result;
        }
        while (true) {
            let element = parseValue();
            if (element == null) return null;
            result.add(element);
            skip();
            let current = code();
            position = position + 1;
            if (current == 93) return result;
            if (current != 44) return fail("Expected , or ]");
        }
    }

    fun parseObject(): JsonValue {
        position = position + 1;
        let result = JsonValue.object();
        skip();
        if (code() == 125) {
            position = position + 1;
            return result;
        }
        while (true) {
            skip();
            if (code() != 34) return fail("Expected a name");
            let name = parseString();
            if (name == null) return null;
            skip();
            if (code() != 58) return fail("Expected :");
            position = position + 1;
            let member = parseValue();
            if (member == null) return null;
            result.put(name, member);
            skip();
            let current = code();
            position = position + 1;
            if (current == 125) return result;
            if (current != 44) return fail("Expected , or }");
        }
    }
}

// Reads JSON text: a value, or null if it isn't JSON.
fun parseJson(text: String): JsonValue {
    return JsonParser(text).parse();
}
