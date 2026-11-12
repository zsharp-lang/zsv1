// std:encoding — what describes a module, encoded, and given again.
//
// An encoder runs as a module is built, before it is lowered: it writes the module's HIR
// as a graph, and gives each definition it writes an id, kept as metadata, which the
// platform writes on what the definition lowers to, as an attribute on the CLR. What it
// wrote, the unit carries, as an assembly's reflection blob.
//
// Giving it again is the decoder's half: an importer of the unit gives the module it
// decodes, each definition bound to what it was built as, by the entity that carries its
// id; and `describe`, the composition of the platform's half and this one, is what
// `reflect` is mapped onto: what carries metadata gives its id and its unit, and the unit
// gives the node.
//
// The structure of a node is inferred from its fields. Bodies are left out: a built
// definition's code is what it was built as.
//
//   - A node another unit's decoding gave is written as that unit's, by its id.
//   - A node that describes a CLR entity, as `String`'s, is written as the entity.
//   - A CLR definition of the module is written as the entity it is built as, by its id.
//   - Anything else is written by its fields, with what is shared written once.

import { Object, String, Boolean, Int32, Int64, Byte, Type, List, Dictionary, ilLoader } from "core:runtime";
import { Class as DotNet_Class, explicitThis } from "pkg:clr/hir";
import { Assembly } from Object.Assembly;
import { public } from "pkg:clr/hir";
import { Instance, Annotated, Kept, Encoded } from "pkg:zsharp/hir";
import { Code, Local } from "pkg:zsharp/hir/code";
import { DotNetDefinition } from "pkg:clr/hir";
import { Error, Result, IResult } from "pkg:zsharp/core";
import { bindings } from "core:compiler";
import { describers, enter } from "core:zsharp";
import { entrypoint } from "std:framework";

// The id a definition is encoded with, kept on what it is built as, so what carries it
// gives the definition again.
@public def ReflectionId of DotNet_Class : Kept {
    @public let Id: Int32;

    @public @explicitThis new(this, id: Int32) { this.Id = id; }
}

// Fails the encoding, with why.
fun fail(message: String): Object {
    let failed: IResult[Object, Error] = Result[Object].Error(message);
    return failed.Unwrap();
}

// What of a node is its code, which a built definition was built as, so it isn't written.
fun isCode(field: FieldInfo): Boolean {
    if (field.FieldType == Code) return true;
    if (field.FieldType.IsGenericType) {
        let arguments = field.FieldType.GetGenericArguments();
        if (arguments.Length == 1) return arguments[0] == Local;
    }
    return false;
}

// What a list holds, as an array.
fun listed(list: Object): Array {
    return list.GetType().GetMethod("ToArray", Type.EmptyTypes).Invoke(list, null) as Array;
}

fun objectType(): Type {
    return Object;
}

// The fields of an object, its type's and its bases'.
fun fieldsOf(type: Type): List[FieldInfo] {
    let found = List[FieldInfo]();
    let current = type;
    while (current != null) {
        let declared = current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        let index = 0;
        while (index < declared.Length) {
            found.Add(declared[index]);
            index = index + 1;
        }
        current = current.BaseType;
    }
    return found;
}

// A field of a type by its name, or null.
fun foundField(type: Type, name: String): FieldInfo {
    let fields = fieldsOf(type);
    let index = 0;
    while (index < fields.Count) {
        if (fields[index].Name == name) return fields[index];
        index = index + 1;
    }
    return null;
}

// The id kept on an entity, or -1.
fun idOn(entity: Object): Int32 {
    let kept = Metadata.Of(Metadata.Entity(entity));
    let index = 0;
    while (index < kept.Count) {
        if (kept[index] is let id: ReflectionId) return id.Id;
        index = index + 1;
    }
    return -1;
}

// ---- Units, decoded

// A unit, as it is decoded once in a process: the entities that carry ids, and the nodes
// it gave each id, so an importer and `describe` give the same nodes.
@public def Unit of DotNet_Class {
    @public let Assembly: Assembly;
    @public let Entities: Dictionary[Int32, Object];
    @public let Nodes: Dictionary[Int32, Object];
    @public let Root: Object;

    @public @explicitThis new(this, assembly: Assembly) {
        this.Assembly = assembly;
        this.Entities = Dictionary[Int32, Object]();
        this.Nodes = Dictionary[Int32, Object]();
    }
}

let units: Dictionary[Assembly, Unit] = Dictionary[Assembly, Unit]();

// The unit an assembly is, which a node it gave belongs to, by its id.
let origins: Dictionary[Object, Unit] = Dictionary[Object, Unit]();
let originIds: Dictionary[Object, Int32] = Dictionary[Object, Int32]();

fun unitOf(assembly: Assembly): Unit {
    if (units.ContainsKey(assembly)) return units[assembly];

    let unit = Unit(assembly);
    units[assembly] = unit;

    let all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    let types = assembly.GetTypes();
    let index = 0;
    while (index < types.Length) {
        let id = idOn(types[index]);
        if (id >= 0) unit.Entities[id] = types[index];

        let members = types[index].GetMembers(all);
        let each = 0;
        while (each < members.Length) {
            let member = idOn(members[each]);
            if (member >= 0) unit.Entities[member] = members[each];
            each = each + 1;
        }
        index = index + 1;
    }

    let blob = Metadata.Blob(assembly);
    if (blob != null) {
        unit.Root = Decoder(unit, BinaryReader(MemoryStream(blob))).decode();
        bind(unit);
    }
    return unit;
}

// Binds each node the unit gave to the entity built of it, as the node's kind says, once
// something binds that kind.
fun bind(unit: Unit) {
    let ids = List[Int32](unit.Nodes.Keys);
    let index = 0;
    while (index < ids.Count) {
        let id = ids[index];
        let node = unit.Nodes[id];
        if (unit.Entities.ContainsKey(id)) {
            let failed = bindings.Bind(node, unit.Entities[id]);
            if (failed != null) fail("A node of " + unit.Assembly.GetName().Name + " can't be bound: " + failed);
        }
        index = index + 1;
    }
}

// ---- Encoding

@public def Encoder of DotNet_Class {
    @public let objects: List[Object];
    @public let indexes: Dictionary[Object, Int32];
    @public let ids: Dictionary[Object, Int32];
    @public let annotationLists: List[Object];
    @public let stream: MemoryStream;
    @public let body: BinaryWriter;

    @public @explicitThis new(this) {
        this.objects = List[Object]();
        this.indexes = Dictionary[Object, Int32]();
        this.ids = Dictionary[Object, Int32]();
        this.annotationLists = List[Object]();
        this.stream = MemoryStream();
        this.body = BinaryWriter(this.stream);
    }

    @public @explicitThis fun encode(this, root: Object): Byte[] {
        this.index(root);
        let at = 0;
        while (at < this.objects.Count) {
            this.record(this.objects[at]);
            at = at + 1;
        }
        this.body.Flush();

        let whole = MemoryStream();
        let header = BinaryWriter(whole);
        header.Write(1);
        header.Write(this.objects.Count);
        header.Write(this.stream.ToArray());
        header.Flush();
        return whole.ToArray();
    }

    // The index of an object written by its fields, which an annotated one is given an
    // id with.
    @public @explicitThis fun index(this, value: Object): Int32 {
        if (this.indexes.ContainsKey(value)) return this.indexes[value];

        let at = this.objects.Count;
        this.objects.Add(value);
        this.indexes[value] = at;

        if (value is let annotated: Annotated) {
            this.annotationLists.Add(annotated.Annotations);
            this.identify(value);
        }
        return at;
    }

    // Gives a definition its id, kept as metadata, replacing one an earlier encoding gave.
    @public @explicitThis fun identify(this, value: Object): Int32 {
        if (this.ids.ContainsKey(value)) return this.ids[value];

        let id = this.ids.Count;
        this.ids[value] = id;

        if (value is let annotated: Annotated) {
            let index = 0;
            while (index < annotated.Annotations.Count) {
                let removed = false;
                if (annotated.Annotations[index] is let earlier: ReflectionId) {
                    annotated.Annotations.RemoveAt(index);
                    removed = true;
                }
                if (removed == false) index = index + 1;
            }
            annotated.Annotations.Add(ReflectionId(id));
        }
        return id;
    }

    @public @explicitThis fun value(this, value: Object) {
        let w = this.body;
        if (value == null) {
            w.Write(0);
            return;
        }
        if (value is let flag: Boolean) {
            w.Write(1);
            w.Write(flag);
            return;
        }
        if (value is let number: Int32) {
            w.Write(2);
            w.Write(number);
            return;
        }
        if (value is let long: Int64) {
            w.Write(3);
            w.Write(long);
            return;
        }
        if (value is let text: String) {
            w.Write(4);
            w.Write(text);
            return;
        }

        let type = value.GetType();
        if (type.IsEnum) {
            w.Write(5);
            w.Write(type.AssemblyQualifiedName);
            w.Write(Convert.ToInt64(value));
            return;
        }
        if (value is let clr: Type) {
            w.Write(6);
            w.Write(clr.AssemblyQualifiedName);
            return;
        }

        if (value is let node: Instance) {
            if (origins.ContainsKey(node)) {
                w.Write(9);
                w.Write(origins[node].Assembly.GetName().Name);
                w.Write(originIds[node]);
                return;
            }

            let described = ilLoader.Described(node);
            if (described != null) {
                w.Write(7);
                this.entity(described);
                return;
            }

            if (node is let defined: DotNetDefinition) {
                if ((node is let annotated: Annotated) == false) fail("A CLR definition that can't be annotated can't be encoded.");
                w.Write(8);
                w.Write(this.identify(node));
                return;
            }
        }

        if (value is let callable: Delegate) fail("A delegate can't be encoded.");
        if (type.Assembly.IsDynamic) fail("A " + type.Name + " is of a type the running process made, which can't be encoded.");

        w.Write(10);
        w.Write(this.index(value));
    }

    // A CLR entity, by its names, as it is found again where it is decoded.
    @public @explicitThis fun entity(this, entity: Object) {
        let w = this.body;
        if (entity is let type: Type) {
            w.Write(1);
            w.Write(type.AssemblyQualifiedName);
            return;
        }
        if (entity is let method: MethodInfo) {
            w.Write(2);
            w.Write(method.DeclaringType.AssemblyQualifiedName);
            w.Write(method.Name);
            this.parameters(method.GetParameters());
            return;
        }
        if (entity is let constructor: ConstructorInfo) {
            w.Write(3);
            w.Write(constructor.DeclaringType.AssemblyQualifiedName);
            this.parameters(constructor.GetParameters());
            return;
        }
        if (entity is let unit: ReflectedModule) {
            w.Write(4);
            w.Write(unit.Assembly.FullName);
            return;
        }
        fail("A " + entity.GetType().Name + " can't be encoded as what it describes.");
    }

    @public @explicitThis fun parameters(this, parameters: ParameterInfo[]) {
        this.body.Write(parameters.Length);
        let index = 0;
        while (index < parameters.Length) {
            this.body.Write(parameters[index].ParameterType.AssemblyQualifiedName);
            index = index + 1;
        }
    }

    @public @explicitThis fun record(this, value: Object) {
        let w = this.body;
        let type = value.GetType();
        w.Write(type.AssemblyQualifiedName);

        if (type.IsArray) {
            let items = value as Array;
            w.Write(1);
            w.Write(items.Length);
            let index = 0;
            while (index < items.Length) {
                this.value(items.GetValue(index));
                index = index + 1;
            }
            return;
        }

        if (type.GetInterface("System.Collections.IList") != null) {
            let list = listed(value);
            // What an annotation list holds is written only if it is encoded.
            let filter = this.annotationLists.Contains(value);
            let kept = List[Object]();
            let index = 0;
            while (index < list.Length) {
                let item = list.GetValue(index);
                if (filter == false) kept.Add(item);
                if (filter) {
                    if (item is let encoded: Encoded) kept.Add(item);
                }
                index = index + 1;
            }

            w.Write(2);
            w.Write(kept.Count);
            index = 0;
            while (index < kept.Count) {
                this.value(kept[index]);
                index = index + 1;
            }
            return;
        }

        w.Write(3);
        let id = -1;
        if (this.ids.ContainsKey(value)) id = this.ids[value];
        w.Write(id);

        let all = fieldsOf(type);
        let written = List[FieldInfo]();
        let field = 0;
        while (field < all.Count) {
            if (isCode(all[field]) == false) written.Add(all[field]);
            field = field + 1;
        }

        w.Write(written.Count);
        field = 0;
        while (field < written.Count) {
            w.Write(written[field].Name);
            this.value(written[field].GetValue(value));
            field = field + 1;
        }
    }
}

// ---- Decoding

// A reference to an object of the graph, which may be decoded after what refers to it.
@public def Pending of DotNet_Class {
    @public let Index: Int32;

    @public @explicitThis new(this, index: Int32) { this.Index = index; }
}

// An object as it was read, before the objects it refers to are made.
@public def Read of DotNet_Class {
    @public let Type: Type;
    @public let Kind: Int32;
    @public let Id: Int32;
    @public let Names: List[String];
    @public let Values: List[Object];

    @public @explicitThis new(this, type: Type, kind: Int32) {
        this.Type = type;
        this.Kind = kind;
        this.Id = -1;
        this.Names = List[String]();
        this.Values = List[Object]();
    }
}

@public def Decoder of DotNet_Class {
    @public let unit: Unit;
    @public let reader: BinaryReader;
    @public let objects: List[Object];

    @public @explicitThis new(this, unit: Unit, reader: BinaryReader) {
        this.unit = unit;
        this.reader = reader;
        this.objects = List[Object]();
    }

    @public @explicitThis fun decode(this): Object {
        let version = this.reader.ReadInt32();
        if (version != 1) fail("The blob of " + this.unit.Assembly.GetName().Name + " is of another encoding.");

        let count = this.reader.ReadInt32();
        let read = List[Read]();
        let index = 0;
        while (index < count) {
            read.Add(this.read());
            index = index + 1;
        }

        // Every object is made first, so each can refer to any.
        index = 0;
        while (index < read.Count) {
            let each = read[index];
            if (each.Kind == 1) this.objects.Add(Array.CreateInstance(each.Type.GetElementType(), each.Values.Count));
            if (each.Kind == 2) this.objects.Add(Activator.CreateInstance(each.Type));
            if (each.Kind == 3) this.objects.Add(RuntimeHelpers.GetUninitializedObject(each.Type));
            index = index + 1;
        }

        index = 0;
        while (index < read.Count) {
            this.fill(read[index], this.objects[index]);
            index = index + 1;
        }

        // What a node means is its library's, so the assembly of each kind of node is
        // entered, as importing it would, but for the unit's own, which is being imported.
        // That is once every node is made, as entering a library can import what refers to
        // this unit's nodes, as a library's entry point imports what compiles its nodes.
        index = 0;
        while (index < read.Count) {
            let owner = read[index].Type.Assembly;
            if (owner != this.unit.Assembly) enter(owner);
            index = index + 1;
        }

        return this.objects[0];
    }

    @public @explicitThis fun fill(this, read: Read, made: Object) {
        let index = 0;
        if (read.Kind == 1) {
            let array = made as Array;
            while (index < read.Values.Count) {
                array.SetValue(this.resolve(read.Values[index]), index);
                index = index + 1;
            }
            return;
        }
        if (read.Kind == 2) {
            let adding = read.Type.GetMethod("Add");
            while (index < read.Values.Count) {
                let argument = Array.CreateInstance(objectType(), 1);
                argument.SetValue(this.resolve(read.Values[index]), 0);
                adding.Invoke(made, argument as Object[]);
                index = index + 1;
            }
            return;
        }

        // A field the type no longer has is left out, so a type that dropped one reads
        // what was encoded of it before, as the standard library's builds are read by the
        // next build of the library that defines their nodes.
        while (index < read.Names.Count) {
            let field = foundField(read.Type, read.Names[index]);
            if (field != null) field.SetValue(made, this.resolve(read.Values[index]));
            index = index + 1;
        }

        if (read.Id >= 0) {
            this.unit.Nodes[read.Id] = made;
            origins[made] = this.unit;
            originIds[made] = read.Id;
        }
    }

    @public @explicitThis fun resolve(this, value: Object): Object {
        if (value is let pending: Pending) return this.objects[pending.Index];
        return value;
    }

    @public @explicitThis fun type(this, name: String): Type {
        let found = Type.GetType(name);
        if (found == null) fail("The type " + name + " isn't loaded where " + this.unit.Assembly.GetName().Name + " is decoded.");
        return found;
    }

    @public @explicitThis fun read(this): Read {
        let type = this.type(this.reader.ReadString());
        let kind = this.reader.ReadInt32();
        let read = Read(type, kind);

        if (kind == 3) {
            read.Id = this.reader.ReadInt32();
            let fields = this.reader.ReadInt32();
            let index = 0;
            while (index < fields) {
                read.Names.Add(this.reader.ReadString());
                read.Values.Add(this.value());
                index = index + 1;
            }
            return read;
        }

        let count = this.reader.ReadInt32();
        let index = 0;
        while (index < count) {
            read.Values.Add(this.value());
            index = index + 1;
        }
        return read;
    }

    @public @explicitThis fun value(this): Object {
        let tag = this.reader.ReadInt32();
        if (tag == 0) return null;
        if (tag == 1) return this.reader.ReadBoolean();
        if (tag == 2) return this.reader.ReadInt32();
        if (tag == 3) return this.reader.ReadInt64();
        if (tag == 4) return this.reader.ReadString();
        if (tag == 5) {
            let type = this.type(this.reader.ReadString());
            return Enum.ToObject(type, this.reader.ReadInt64());
        }
        if (tag == 6) return this.type(this.reader.ReadString());
        if (tag == 7) return this.entity();
        if (tag == 8) {
            let id = this.reader.ReadInt32();
            if (this.unit.Entities.ContainsKey(id) == false) fail("Nothing in " + this.unit.Assembly.GetName().Name + " is built of the CLR definition " + id + ".");
            return ilLoader.Load(this.unit.Entities[id] as Type);
        }
        if (tag == 9) {
            let name = this.reader.ReadString();
            let id = this.reader.ReadInt32();
            let other = unitOf(Assembly.Load(name));
            if (other.Nodes.ContainsKey(id) == false) fail(name + " gives no node " + id + ".");
            return other.Nodes[id];
        }
        if (tag == 10) return Pending(this.reader.ReadInt32());
        return fail("The blob of " + this.unit.Assembly.GetName().Name + " has a value of no kind.");
    }

    // A CLR entity, found by its names, as the IL loader describes it.
    @public @explicitThis fun entity(this): Object {
        let kind = this.reader.ReadInt32();
        if (kind == 1) return ilLoader.Load(this.type(this.reader.ReadString()));
        if (kind == 2) {
            let owner = this.type(this.reader.ReadString());
            let name = this.reader.ReadString();
            let method = owner.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, this.parameters());
            if (method == null) fail(owner.Name + " has no method " + name + " of those parameters.");
            return ilLoader.Load(method);
        }
        if (kind == 3) {
            let owner = this.type(this.reader.ReadString());
            let constructor = owner.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, this.parameters());
            if (constructor == null) fail(owner.Name + " has no constructor of those parameters.");
            return ilLoader.Load(constructor);
        }
        if (kind == 4) return ilLoader.Load(Assembly.Load(this.reader.ReadString()).ManifestModule);
        return fail("The blob of " + this.unit.Assembly.GetName().Name + " has an entity of no kind.");
    }

    @public @explicitThis fun parameters(this): Type[] {
        let count = this.reader.ReadInt32();
        let types = List[Type]();
        let index = 0;
        while (index < count) {
            types.Add(this.type(this.reader.ReadString()));
            index = index + 1;
        }
        return types.ToArray();
    }
}

// ---- The halves

// What describes a module, encoded, as the unit it is built into carries it. Encoding
// gives each definition written its id, so it is to be done before the module is lowered.
fun encode(node: Instance): Byte[] { return Encoder().encode(node); }

// What importing an assembly gives, if it carries what an encoder wrote of it: its
// module, decoded, each definition bound to what it was built as.
fun describeAssembly(assembly: Assembly): Instance {
    if (Metadata.Blob(assembly) == null) return null;
    return unitOf(assembly).Root as Instance;
}

// Reflection, as `reflect` is mapped onto it: what carries metadata gives its id and its
// unit, which gives the node; what carries none is a CLR type, which the IL loader
// describes, or nothing.
fun describe(value: Object): Instance {
    let entity = Metadata.Entity(value);
    let unit = Metadata.Unit(entity);
    if (unit != null) {
        let id = idOn(entity);
        if (id >= 0) {
            let decoded = unitOf(unit.Assembly);
            if (decoded.Nodes.ContainsKey(id)) return decoded.Nodes[id] as Instance;
        }
    }
    if (value is let type: Type) return ilLoader.Load(type);
    return null;
}

// Importing an assembly that carries a blob gives what it describes.
@entrypoint
fun install() { describers.Add(describeAssembly); }
