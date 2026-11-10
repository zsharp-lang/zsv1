// The platform: an assembly being written. LIR modules are added to it, with whatever
// they reach in other modules, and it is written once it has everything. A module is a
// static class of its functions and globals, whose type initializer is the module's
// initializer. A structure is a value type, and a polymorphic one, as a Z# class lowers
// to, a class or an interface, whose methods are the functions that are its methods. A
// CLR class defined in Z# is a class definition, written as the runtime loads one.

// What emitting one function's body keeps: its locals and the loops it is in.
def Body of DotNet_Class {
    let method: MethodDefinition;
    let function: LIRFunction;
    let il: ILProcessor;
    let locals: Dictionary[LIRLocal, VariableDefinition];
    let starts: Dictionary[LIRLoop, Instruction];
    let ends: Dictionary[LIRLoop, Instruction];

    @explicitThis new(this, method: MethodDefinition, function: LIRFunction) {
        this.method = method;
        this.function = function;
        this.il = method.Body.GetILProcessor();
        this.locals = Dictionary[LIRLocal, VariableDefinition]();
        this.starts = Dictionary[LIRLoop, Instruction]();
        this.ends = Dictionary[LIRLoop, Instruction]();
        method.Body.InitLocals = true;
    }

    @explicitThis fun local(this, type: TypeReference): VariableDefinition {
        let variable = VariableDefinition(type);
        this.method.Body.Variables.Add(variable);
        return variable;
    }

    // A method's object is its first parameter, which is `this` to the CLR.
    @explicitThis fun parameter(this, parameter: LIRParameter): ParameterDefinition {
        let index = this.function.Signature.Parameters.IndexOf(parameter);
        if (index < 0) fail("The parameter " + parameter.Name + " isn't one of " + this.function.Name + "'s.");
        if (this.method.HasThis) {
            if (index == 0) return this.method.Body.ThisParameter;
            return this.method.Parameters[index - 1];
        }
        return this.method.Parameters[index];
    }

    @explicitThis fun variable(this, variable: LIRVariable): VariableDefinition {
        if (variable is local of LIRLocal) {
            if (this.locals.ContainsKey(local)) return this.locals[local];
        }
        return fail("A local isn't one of its function's.") as VariableDefinition;
    }

    @explicitThis fun label(this): Instruction { return this.il.Create(OpCodes.Nop); }
}

def Cecil of DotNet_Class : ClrPlatform {
    let assembly: AssemblyDefinition;
    let main: ModuleDefinition;
    let references: References;
    let reflected: Reflected;
    let holders: Dictionary[LIRModule, TypeDefinition];
    let functions: Dictionary[LIRFunction, MethodDefinition];
    let globals: Dictionary[LIRGlobal, FieldDefinition];
    let methods: Dictionary[ClrMethod, MethodReference];
    let structures: Dictionary[LIRStructure, TypeDefinition];
    let fieldsOf: Dictionary[LIRField, FieldDefinition];
    let constructors: Dictionary[LIRStructure, MethodDefinition];
    let instanceMethods: Dictionary[LIRFunction, Boolean];
    let pending: List[LIRFunction];
    let initializers: List[LIRModule];
    let opcodes: Dictionary[Int16, OpCode];
    let names: Dictionary[String, Boolean];
    let standalone: TypeDefinition;
    let box: TypeDefinition;
    let unnamed: Int32;
    let definitions: Dictionary[ClassDefinition, TypeDefinition];
    let definedFields: Dictionary[ClassField, FieldDefinition];
    let definedMethods: Dictionary[ClassMethod, MethodDefinition];
    let definedConstructors: Dictionary[ClassConstructor, MethodDefinition];
    let definedParameters: Dictionary[ClassGenericParameter, GenericParameter];
    let prologues: Dictionary[LIRFunction, MethodReference];

    // An assembly for a version of .NET, as `net8.0`, or, given null, for the .NET that
    // runs it.
    @explicitThis new(this, name: String, version: String) {
        this.assembly = AssemblyDefinition.CreateAssembly(AssemblyNameDefinition(name, Version(1, 0, 0, 0)), name + ".dll", ModuleKind.Dll);
        this.main = this.assembly.MainModule;
        this.references = References(this.main, version);
        this.reflected = Reflected();
        this.holders = Dictionary[LIRModule, TypeDefinition]();
        this.functions = Dictionary[LIRFunction, MethodDefinition]();
        this.globals = Dictionary[LIRGlobal, FieldDefinition]();
        this.methods = Dictionary[ClrMethod, MethodReference]();
        this.structures = Dictionary[LIRStructure, TypeDefinition]();
        this.fieldsOf = Dictionary[LIRField, FieldDefinition]();
        this.constructors = Dictionary[LIRStructure, MethodDefinition]();
        this.instanceMethods = Dictionary[LIRFunction, Boolean]();
        this.pending = List[LIRFunction]();
        this.initializers = List[LIRModule]();
        this.names = Dictionary[String, Boolean]();
        this.unnamed = 0;
        this.definitions = Dictionary[ClassDefinition, TypeDefinition]();
        this.definedFields = Dictionary[ClassField, FieldDefinition]();
        this.definedMethods = Dictionary[ClassMethod, MethodDefinition]();
        this.definedConstructors = Dictionary[ClassConstructor, MethodDefinition]();
        this.definedParameters = Dictionary[ClassGenericParameter, GenericParameter]();
        this.prologues = Dictionary[LIRFunction, MethodReference]();

        this.opcodes = Dictionary[Int16, OpCode]();
        let table: Type = OpCodes;
        let fields = table.GetFields(BindingFlags.Public | BindingFlags.Static);
        let index = 0;
        while (index < fields.Length) {
            let opcode = fields[index].GetValue(null) as OpCode;
            this.opcodes[opcode.Value] = opcode;
            index = index + 1;
        }
    }

    // ---- What is written

    // Adds a module to the assembly, with what it reaches.
    @explicitThis fun add(this, unit: LIRModule) {
        this.holder(unit);

        let index = 0;
        while (index < unit.Structures.Count) {
            this.structure(unit.Structures[index]);
            index = index + 1;
        }

        index = 0;
        while (index < unit.Globals.Count) {
            this.global(unit.Globals[index]);
            index = index + 1;
        }

        index = 0;
        while (index < unit.Functions.Count) {
            if (this.isOperation(unit.Functions[index]) == false) this.function(unit.Functions[index]);
            index = index + 1;
        }

        // The CLR classes the module defines are written with it, and its entry point is
        // marked as one, as the IL loader's importer reads it.
        index = 0;
        while (index < unit.Metadata.Count) {
            if (unit.Metadata[index] is defines of DefinesClass) this.classDefinition(defines.Definition);
            if (unit.Metadata[index] is entry of LIREntryPoint) this.function(entry.Function).CustomAttributes.Add(CustomAttribute(this.attribute("ZSharp.Importer.ILLoader.EntryPointAttribute")));
            index = index + 1;
        }

        this.flush();
    }

    // The assembly is written the same each time it is built from the same LIR: its
    // module's identity is of its content, and it has no time of its own.
    @explicitThis fun write(this, path: String) {
        this.flush();
        let parameters = WriterParameters();
        parameters.DeterministicMvid = true;
        parameters.Timestamp = Convert.ToUInt32(0);
        this.assembly.Write(path, parameters);
    }

    // The files of the assemblies the written one needs that neither .NET nor the toolchain
    // that runs it has, as a package's, with those they need in turn: what a build puts
    // next to it, so it runs where they aren't installed.
    @explicitThis fun dependencies(this): List[String] {
        this.flush();
        let found = List[String]();
        let index = 0;
        while (index < this.main.AssemblyReferences.Count) {
            this.depend(this.main.AssemblyReferences[index].Name, found);
            index = index + 1;
        }
        return found;
    }

    @explicitThis fun depend(this, name: String, found: List[String]) {
        let core: Type = Object;
        let framework = Path.GetDirectoryName(core.Assembly.Location);
        let toolchain = Path.GetDirectoryName(Path.GetFullPath(AppContext.BaseDirectory));

        let assemblies = AppDomain.CurrentDomain.GetAssemblies();
        let index = 0;
        while (index < assemblies.Length) {
            let assembly = assemblies[index];
            if (assembly.GetName().Name == name) {
                if (this.bundles(assembly, framework, toolchain, found)) {
                    found.Add(assembly.Location);

                    let referenced = assembly.GetReferencedAssemblies();
                    let each = 0;
                    while (each < referenced.Length) {
                        this.depend(referenced[each].Name, found);
                        each = each + 1;
                    }
                }
                index = assemblies.Length;
            }
            index = index + 1;
        }
    }

    // Whether an assembly goes with a build: one with a file of its own, that neither .NET
    // nor the toolchain has, and that isn't with it already.
    @explicitThis fun bundles(this, assembly: Assembly, framework: String, toolchain: String, found: List[String]): Boolean {
        if (assembly.IsDynamic) return false;
        if (assembly.Location == "") return false;
        let directory = Path.GetDirectoryName(assembly.Location);
        if (directory == framework) return false;
        if (directory == toolchain) return false;
        return found.Contains(assembly.Location) == false;
    }

    // A name for something LIR names or doesn't, once in the assembly.
    // A type of a dotted name, as `Shape.Class`, is in the namespace of what is before its
    // last dot, so its full name is that name however it is found: a module Reflection.Emit
    // builds refers to a type by its namespace and name.
    @explicitThis fun named(this, name: String, attributes: TypeAttributes, base: TypeReference): TypeDefinition {
        let dot = name.LastIndexOf(".");
        if (dot < 0) return TypeDefinition("", name, attributes, base);
        return TypeDefinition(name.Substring(0, dot), name.Substring(dot + 1), attributes, base);
    }

    @explicitThis fun unique(this, name: String, fallback: String): String {
        let given = name;
        if (given == null) given = fallback;
        if (given == "") given = fallback;
        if (this.names.ContainsKey(given)) {
            this.unnamed = this.unnamed + 1;
            given = given + "$" + this.unnamed;
        }
        this.names[given] = true;
        return given;
    }

    @explicitThis fun holder(this, unit: LIRModule): TypeDefinition {
        if (this.holders.ContainsKey(unit)) return this.holders[unit];

        let attributes = TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class;
        let holder = TypeDefinition("", this.unique(unit.Name, "Module"), attributes, this.system("System.Object"));
        holder.CustomAttributes.Add(CustomAttribute(this.moduleScope()));
        this.annotate(holder, unit.Metadata);
        this.main.Types.Add(holder);
        this.holders[unit] = holder;

        if (unit.Initializer.Block.Statements.Count > 0) this.initializers.Add(unit);
        return holder;
    }

    // A module's class is a module scope, as the IL loader reads one: its static members
    // are the module's, so importing the assembly gives the module.
    @explicitThis fun moduleScope(this): MethodReference { return this.attribute("ZSharp.Importer.ILLoader.ModuleScopeAttribute"); }

    // What was kept of what a definition was lowered from, written as the IL loader reads
    // it back: an attribute of the annotation's type, and each of its fields by name with
    // its value, a constant.
    @explicitThis fun annotate(this, target: ICustomAttributeProvider, metadata: List[LIRMetadata]) {
        let index = 0;
        while (index < metadata.Count) {
            if (metadata[index] is kept of KeptAnnotation) target.CustomAttributes.Add(this.metadataOf(kept.Value));
            index = index + 1;
        }
    }

    @explicitThis fun metadataOf(this, annotation: Object): CustomAttribute {
        let written = CustomAttribute(this.attribute("ZSharp.Importer.ILLoader.MetadataAttribute"));
        let system = this.main.TypeSystem;
        let fields = annotation.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        let names = List[CustomAttributeArgument]();
        let values = List[CustomAttributeArgument]();
        let index = 0;
        while (index < fields.Length) {
            names.Add(CustomAttributeArgument(system.String, fields[index].Name));
            values.Add(CustomAttributeArgument(system.Object, this.constant(fields[index].GetValue(annotation))));
            index = index + 1;
        }

        let core: Type = Object;
        written.ConstructorArguments.Add(CustomAttributeArgument(this.system("System.Type"), this.type(this.reflected.Type(annotation.GetType()))));
        written.ConstructorArguments.Add(CustomAttributeArgument(ArrayType(system.String), names.ToArray()));
        written.ConstructorArguments.Add(CustomAttributeArgument(ArrayType(system.Object), values.ToArray()));
        return written;
    }

    // A constant of an attribute, as its own type gives it.
    @explicitThis fun constant(this, value: Object): CustomAttributeArgument {
        let system = this.main.TypeSystem;
        if (value is text of String) return CustomAttributeArgument(system.String, text);
        if (value is number of Int32) return CustomAttributeArgument(system.Int32, number);
        if (value is long of Int64) return CustomAttributeArgument(system.Int64, long);
        if (value is flag of Boolean) return CustomAttributeArgument(system.Boolean, flag);
        return fail("An annotation kept as metadata has state that isn't a constant.") as CustomAttributeArgument;
    }

    // Carries bytes an encoder wrote in the assembly, as the resource of this name, which
    // the assembly says it carries as its reflection blob.
    @explicitThis fun embed(this, resource: String, bytes: Byte[], decoder: String) {
        this.main.Resources.Add(EmbeddedResource(resource, ManifestResourceAttributes.Public, bytes));
        let blob = CustomAttribute(this.blobAttribute());
        blob.ConstructorArguments.Add(CustomAttributeArgument(this.main.TypeSystem.String, resource));
        blob.ConstructorArguments.Add(CustomAttributeArgument(this.main.TypeSystem.String, decoder));
        this.assembly.CustomAttributes.Add(blob);
    }

    // The constructor of the reflection blob attribute that names the decoder too.
    @explicitThis fun blobAttribute(this): MethodReference {
        let assemblies = AppDomain.CurrentDomain.GetAssemblies();
        let index = 0;
        while (index < assemblies.Length) {
            let found = assemblies[index].GetType("ZSharp.Importer.ILLoader.ReflectionBlobAttribute");
            if (found != null) {
                let constructors = found.GetConstructors();
                let each = 0;
                while (each < constructors.Length) {
                    if (constructors[each].GetParameters().Length == 2) return this.method(this.reflected.Method(constructors[each]));
                    each = each + 1;
                }
            }
            index = index + 1;
        }
        return fail("The IL loader's attributes aren't loaded.") as MethodReference;
    }

    // The constructor of one of the IL loader's attributes, by its full name.
    @explicitThis fun attribute(this, name: String): MethodReference {
        let assemblies = AppDomain.CurrentDomain.GetAssemblies();
        let index = 0;
        while (index < assemblies.Length) {
            let found = assemblies[index].GetType(name);
            if (found != null) return this.method(this.reflected.Method(found.GetConstructors()[0]));
            index = index + 1;
        }
        return fail("The IL loader's attributes aren't loaded.") as MethodReference;
    }

    // Functions that belong to no module, as code run on its own makes, are in a class of
    // their own.
    @explicitThis fun holderOf(this, unit: LIRModule): TypeDefinition {
        if (unit != null) return this.holder(unit);
        if (this.standalone == null) {
            this.standalone = TypeDefinition("", this.unique("<Standalone>", "<Standalone>"), TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class, this.system("System.Object"));
            this.main.Types.Add(this.standalone);
        }
        return this.standalone;
    }

    @explicitThis fun global(this, global: LIRGlobal): FieldDefinition {
        if (this.globals.ContainsKey(global)) return this.globals[global];
        if (global.Module == null) fail("A global belongs to a module.");

        let holder = this.holder(global.Module);
        let field = FieldDefinition(this.unique(global.Name, "global"), FieldAttributes.Public | FieldAttributes.Static, this.type(global.Type));
        holder.Fields.Add(field);
        this.globals[global] = field;
        this.annotate(field, global.Metadata);
        return field;
    }

    @explicitThis fun function(this, function: LIRFunction): MethodDefinition {
        if (this.functions.ContainsKey(function)) return this.functions[function];

        // A method is declared with its class.
        let owner = this.ownerOf(function);
        if (owner != null) {
            this.structure(owner);
            return this.functions[function];
        }

        let holder = this.holderOf(function.Module);
        let method = MethodDefinition(this.unique(function.Name, "function"), MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, this.type(function.Signature.ReturnType));

        let index = 0;
        while (index < function.Signature.Parameters.Count) {
            let parameter = function.Signature.Parameters[index];
            method.Parameters.Add(ParameterDefinition(parameter.Name, ParameterAttributes.None, this.type(parameter.Type)));
            index = index + 1;
        }

        holder.Methods.Add(method);
        this.functions[function] = method;
        this.annotate(method, function.Metadata);
        this.pending.Add(function);
        return method;
    }

    // Emits the bodies of the functions declared, and the initializers of the modules,
    // which may declare more.
    @explicitThis fun flush(this) {
        while (this.pending.Count > 0 or this.initializers.Count > 0) {
            if (this.pending.Count > 0) {
                let function = this.pending[0];
                this.pending.RemoveAt(0);
                if (function.Body == null) fail("The function " + function.Name + " has no body to write.");
                this.emitBody(this.functions[function], function);
            }
            else {
                let unit = this.initializers[0];
                this.initializers.RemoveAt(0);

                let attributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.Static;
                let initializer = MethodDefinition(".cctor", attributes, this.system("System.Void"));
                this.holder(unit).Methods.Add(initializer);

                let code = LIRFunction(LIRSignature());
                code.Body = unit.Initializer;
                this.emitBody(initializer, code);
            }
        }
    }

    @explicitThis fun emitBody(this, method: MethodDefinition, function: LIRFunction) {
        let body = Body(method, function);

        // A constructor's object is first made an object of its base.
        if (this.prologues.ContainsKey(function)) {
            body.il.Emit(OpCodes.Ldarg_0);
            body.il.Emit(OpCodes.Call, this.prologues[function]);
        }

        this.statement(body, function.Body.Block);

        // Reaching the end is valid only when there is nothing to return.
        if (function.Signature.ReturnType == null) body.il.Emit(OpCodes.Ret);
        else {
            body.il.Emit(OpCodes.Ldnull);
            body.il.Emit(OpCodes.Throw);
        }
    }

    // ---- Types

    @explicitThis fun system(this, name: String): TypeReference {
        let core: Type = Object;
        return this.references.type(this.reflected.Type(core.Assembly.GetType(name)) as ClrType);
    }

    @explicitThis fun type(this, type: LIRType): TypeReference {
        if (type == null) return this.system("System.Void");
        if (type is clr of ClrType) return this.references.type(clr);
        if (type is array of ClrArrayType) {
            if (array.Rank == 1) return ArrayType(this.type(array.Element));
            return ArrayType(this.type(array.Element), array.Rank);
        }
        if (type is reference of ByRefType) return ByReferenceType(this.type(reference.Target));
        if (type is pointer of ClrPointerType) return PointerType(this.type(pointer.Target));
        if (type is instance of GenericInstance) {
            // An instance is of the definition itself, not of what its own code calls it.
            let definition: TypeReference = null;
            if (instance.Definition is own of ClassDefinition) definition = this.classDefinition(own);
            else definition = this.type(instance.Definition);
            let made = GenericInstanceType(definition);
            let index = 0;
            while (index < instance.Arguments.Count) {
                made.GenericArguments.Add(this.type(instance.Arguments[index]));
                index = index + 1;
            }
            return made;
        }
        if (type is functionType of LIRFunctionType) return this.system("System.IntPtr");
        if (type is boxed of LIRReference) return this.boxOf(this.type(boxed.Target));
        if (type is structure of LIRStructure) return this.structure(structure);
        if (type is defined of ClassDefinition) return this.selfOf(this.classDefinition(defined));
        if (type is parameter of ClassGenericParameter) {
            if (this.definedParameters.ContainsKey(parameter)) return this.definedParameters[parameter];
            return fail("A type parameter of a class is only known inside its class.") as TypeReference;
        }
        return fail("The Cecil platform can't write a type that is a " + type.GetType().Name + " yet.") as TypeReference;
    }

    @explicitThis fun isValueType(this, type: LIRType): Boolean {
        if (type is clr of ClrType) return clr.IsValueType;
        if (type is instance of GenericInstance) return this.isValueType(instance.Definition);
        if (type is structure of LIRStructure) return this.polymorphicOf(structure) == null;
        return false;
    }

    // A reference to a variable is a box of it, a class with the variable as its field,
    // which the assembly defines for itself.
    @explicitThis fun boxType(this): TypeDefinition {
        if (this.box != null) return this.box;

        let box = TypeDefinition("ZSharp", "Box`1", TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class, this.system("System.Object"));
        let parameter = GenericParameter("T", box);
        box.GenericParameters.Add(parameter);
        this.main.Types.Add(box);
        this.box = box;

        box.Fields.Add(FieldDefinition("Value", FieldAttributes.Public, parameter));

        let attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        let constructor = MethodDefinition(".ctor", attributes, this.system("System.Void"));
        constructor.Parameters.Add(ParameterDefinition("value", ParameterAttributes.None, parameter));
        box.Methods.Add(constructor);

        let self = GenericInstanceType(box);
        self.GenericArguments.Add(parameter);

        let il = constructor.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, this.objectConstructor());
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, this.boxValue(self));
        il.Emit(OpCodes.Ret);
        return box;
    }

    @explicitThis fun objectConstructor(this): MethodReference {
        let core: Type = Object;
        let constructors = core.GetConstructors();
        return this.method(this.reflected.Method(constructors[0]));
    }

    @explicitThis fun boxOf(this, target: TypeReference): TypeReference {
        let made = GenericInstanceType(this.boxType());
        made.GenericArguments.Add(target);
        return made;
    }

    @explicitThis fun boxValue(this, box: TypeReference): FieldReference {
        return FieldReference("Value", this.boxType().GenericParameters[0], box);
    }

    @explicitThis fun boxConstructor(this, box: TypeReference): MethodReference {
        let constructor = MethodReference(".ctor", this.system("System.Void"), box);
        constructor.HasThis = true;
        constructor.Parameters.Add(ParameterDefinition(this.boxType().GenericParameters[0]));
        return constructor;
    }

    // ---- CLR members

    // A method as code calls it: of a generic instance, the instance's, and of a generic
    // method, its instance.
    @explicitThis fun method(this, method: ClrMethod): MethodReference {
        if (this.methods.ContainsKey(method)) return this.methods[method];

        let result: MethodReference = null;
        if (method.DeclaringType is array of ClrArrayType) result = this.arrayMethod(method, array);
        else {
            result = this.references.method(method);
            if (method.DeclaringType is instance of GenericInstance) result = this.onType(result, this.type(instance));
        }

        if (method.TypeArguments.Count > 0) {
            let made = GenericInstanceMethod(result);
            let index = 0;
            while (index < method.TypeArguments.Count) {
                made.GenericArguments.Add(this.type(method.TypeArguments[index]));
                index = index + 1;
            }
            result = made;
        }

        this.methods[method] = result;
        return result;
    }

    // A method of a definition, as an instance of it has it.
    @explicitThis fun onType(this, definition: MethodReference, owner: TypeReference): MethodReference {
        let reference = MethodReference(definition.Name, definition.ReturnType, owner);
        reference.HasThis = definition.HasThis;
        reference.ExplicitThis = definition.ExplicitThis;
        reference.CallingConvention = definition.CallingConvention;

        let index = 0;
        while (index < definition.Parameters.Count) {
            reference.Parameters.Add(ParameterDefinition(definition.Parameters[index].ParameterType));
            index = index + 1;
        }

        index = 0;
        while (index < definition.GenericParameters.Count) {
            reference.GenericParameters.Add(GenericParameter(definition.GenericParameters[index].Name, reference));
            index = index + 1;
        }
        return reference;
    }

    // An array's methods are made up by the runtime, so they are written by their names
    // and signatures.
    @explicitThis fun arrayMethod(this, method: ClrMethod, owner: ClrArrayType): MethodReference {
        let returned = this.system("System.Void");
        let first = 0;
        if (method.IsConstructor == false) {
            returned = this.type(method.Signature.ReturnType);
            first = 1;
        }

        let reference = MethodReference(method.Name, returned, this.type(owner));
        reference.HasThis = true;

        let index = first;
        while (index < method.Signature.Parameters.Count) {
            reference.Parameters.Add(ParameterDefinition(this.type(method.Signature.Parameters[index].Type)));
            index = index + 1;
        }
        return reference;
    }

    @explicitThis fun field(this, field: ClrField): FieldReference {
        let definition = this.references.field(field);
        if (field.DeclaringType is instance of GenericInstance) return FieldReference(definition.Name, definition.FieldType, this.type(instance));
        return definition;
    }

    // ---- Structures and classes

    @explicitThis fun polymorphicOf(this, structure: LIRStructure): Polymorphic {
        let index = 0;
        while (index < structure.Metadata.Count) {
            if (structure.Metadata[index] is mark of Polymorphic) return mark;
            index = index + 1;
        }
        return null;
    }

    @explicitThis fun ownerOf(this, function: LIRFunction): LIRStructure {
        let index = 0;
        while (index < function.Metadata.Count) {
            if (function.Metadata[index] is method of MethodOf) return method.Owner;
            index = index + 1;
        }
        return null;
    }

    // The type a structure is, declared the first time it is reached, before its members,
    // as they can be of it.
    @explicitThis fun structure(this, structure: LIRStructure): TypeDefinition {
        if (this.structures.ContainsKey(structure)) return this.structures[structure];

        let mark = this.polymorphicOf(structure);
        if (mark == null) return this.valueType(structure);
        return this.classOf(structure, mark);
    }

    @explicitThis fun valueType(this, structure: LIRStructure): TypeDefinition {
        let attributes = TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout;
        let type = this.named(this.unique(structure.Name, "Structure"), attributes, this.system("System.ValueType"));
        this.main.Types.Add(type);
        this.structures[structure] = type;
        this.annotate(type, structure.Metadata);
        this.declareFields(structure, type);
        return type;
    }

    @explicitThis fun declareFields(this, structure: LIRStructure, type: TypeDefinition) {
        let index = 0;
        while (index < structure.Fields.Count) {
            let field = structure.Fields[index];
            let name = field.Name;
            if (name == null) name = "field" + index;
            let declared = FieldDefinition(name, FieldAttributes.Public, this.type(field.Type));
            type.Fields.Add(declared);
            this.fieldsOf[field] = declared;
            index = index + 1;
        }
    }

    // A class, or an interface: what implements it is its interfaces, its fields are its
    // own, and an object starts with no code of its own, as the code that makes one sets
    // its fields. Its methods are the functions of its module that are its methods.
    @explicitThis fun classOf(this, structure: LIRStructure, mark: Polymorphic): TypeDefinition {
        let attributes = TypeAttributes.Public | TypeAttributes.Class;
        let base = this.system("System.Object");
        if (mark.IsInterface) {
            attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract;
            base = null;
        }

        let type = this.named(this.unique(structure.Name, "Object"), attributes, base);
        this.main.Types.Add(type);
        this.structures[structure] = type;
        this.annotate(type, structure.Metadata);

        let index = 0;
        while (index < mark.Interfaces.Count) {
            type.Interfaces.Add(InterfaceImplementation(this.structure(mark.Interfaces[index])));
            index = index + 1;
        }

        if (mark.IsInterface == false) {
            this.declareFields(structure, type);

            let constructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
            let constructor = MethodDefinition(".ctor", constructorAttributes, this.system("System.Void"));
            type.Methods.Add(constructor);
            this.constructors[structure] = constructor;

            let il = constructor.Body.GetILProcessor();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, this.objectConstructor());
            il.Emit(OpCodes.Ret);
        }

        if (structure.Module != null) {
            let functions = structure.Module.Functions;
            index = 0;
            while (index < functions.Count) {
                if (this.ownerOf(functions[index]) == structure) this.declareMethod(functions[index], type, mark.IsInterface);
                index = index + 1;
            }
        }
        return type;
    }

    // A method is virtual, so a method of the same name and signature implements an
    // interface's, and one with no code is abstract, as an interface's are.
    @explicitThis fun declareMethod(this, function: LIRFunction, type: TypeDefinition, ofInterface: Boolean) {
        let attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot;
        if (ofInterface or function.Body == null) {
            attributes = attributes | MethodAttributes.Abstract;
            if (ofInterface == false) type.IsAbstract = true;
        }
        else attributes = attributes | MethodAttributes.Final;

        let name = function.Name;
        if (name == null) name = "<unnamed>" + type.Methods.Count;
        if (name == "") name = "<unnamed>" + type.Methods.Count;

        let method = MethodDefinition(name, attributes, this.type(function.Signature.ReturnType));
        let index = 1;
        while (index < function.Signature.Parameters.Count) {
            let parameter = function.Signature.Parameters[index];
            method.Parameters.Add(ParameterDefinition(parameter.Name, ParameterAttributes.None, this.type(parameter.Type)));
            index = index + 1;
        }

        type.Methods.Add(method);
        this.functions[function] = method;
        this.annotate(method, function.Metadata);
        this.instanceMethods[function] = true;
        if (function.Body != null) this.pending.Add(function);
    }

    @explicitThis fun structureOf(this, target: LIRExpression): LIRStructure {
        let type = this.typeOf(target);
        if (type is boxed of LIRReference) type = boxed.Target;
        if (type is structure of LIRStructure) return structure;
        return fail("A field is read from a structure, or a reference to one.") as LIRStructure;
    }

    @explicitThis fun fieldOf(this, field: LIRField): FieldDefinition {
        if (this.fieldsOf.ContainsKey(field)) return this.fieldsOf[field];
        return fail("A field isn't one of a structure the assembly has.") as FieldDefinition;
    }

    // Puts what a field is read from or written to on the stack: an object itself, or the
    // address of a value.
    @explicitThis fun fieldOwner(this, body: Body, target: LIRExpression) {
        let type = this.typeOf(target);
        if (type is boxed of LIRReference) {
            this.expression(body, target);
            body.il.Emit(OpCodes.Ldflda, this.boxValue(this.type(type)));
            return;
        }
        if (this.isValueType(type)) {
            this.address(body, target);
            return;
        }
        this.expression(body, target);
    }

    // A new object is made, and its fields set; a new value is a zero one, its fields set
    // through its address.
    @explicitThis fun construct(this, body: Body, made: LIRNew) {
        let structure = made.Structure;
        let type = this.structure(structure);

        if (this.polymorphicOf(structure) != null) {
            body.il.Emit(OpCodes.Newobj, this.constructors[structure]);
            let index = 0;
            while (index < structure.Fields.Count) {
                body.il.Emit(OpCodes.Dup);
                this.expression(body, made.Fields[index]);
                body.il.Emit(OpCodes.Stfld, this.fieldOf(structure.Fields[index]));
                index = index + 1;
            }
            return;
        }

        let value = body.local(type);
        body.il.Emit(OpCodes.Ldloca, value);
        body.il.Emit(OpCodes.Initobj, type);
        let field = 0;
        while (field < structure.Fields.Count) {
            body.il.Emit(OpCodes.Ldloca, value);
            this.expression(body, made.Fields[field]);
            body.il.Emit(OpCodes.Stfld, this.fieldOf(structure.Fields[field]));
            field = field + 1;
        }
        body.il.Emit(OpCodes.Ldloc, value);
    }

    // ---- CLR classes defined in Z#

    // A class as the runtime loads one: its type parameters, its base and interfaces, its
    // fields, constructors that make an object of its base first, methods marked as what
    // they are, and properties of its accessors.
    @explicitThis fun classDefinition(this, definition: ClassDefinition): TypeDefinition {
        if (this.definitions.ContainsKey(definition)) return this.definitions[definition];

        let attributes = TypeAttributes.Public | TypeAttributes.Class;
        if (definition.IsInterface) attributes = TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract;

        let type = TypeDefinition("", this.unique(definition.Name, "Class"), attributes, null);
        this.main.Types.Add(type);
        this.definitions[definition] = type;
        this.annotate(type, definition.Metadata);

        let index = 0;
        while (index < definition.GenericParameters.Count) {
            let parameter = GenericParameter(definition.GenericParameters[index].Name, type);
            type.GenericParameters.Add(parameter);
            this.definedParameters[definition.GenericParameters[index]] = parameter;
            index = index + 1;
        }

        if (definition.IsInterface == false) {
            if (definition.Base == null) type.BaseType = this.system("System.Object");
            else type.BaseType = this.type(definition.Base);
        }

        index = 0;
        while (index < definition.Interfaces.Count) {
            type.Interfaces.Add(InterfaceImplementation(this.type(definition.Interfaces[index])));
            index = index + 1;
        }

        index = 0;
        while (index < definition.Fields.Count) {
            let field = definition.Fields[index];
            let declared = FieldDefinition(field.Name, FieldAttributes.Public, this.type(field.Type));
            type.Fields.Add(declared);
            this.definedFields[field] = declared;
            index = index + 1;
        }

        index = 0;
        while (index < definition.Constructors.Count) {
            this.declareConstructor(definition, definition.Constructors[index], type);
            index = index + 1;
        }

        index = 0;
        while (index < definition.Methods.Count) {
            this.declareClassMethod(definition, definition.Methods[index], type);
            index = index + 1;
        }

        index = 0;
        while (index < definition.Properties.Count) {
            let property = definition.Properties[index];
            let declared = PropertyDefinition(property.Name, PropertyAttributes.None, this.type(property.Type));
            declared.HasThis = property.IsStatic == false;
            if (property.Getter != null) declared.GetMethod = this.definedMethods[property.Getter];
            if (property.Setter != null) declared.SetMethod = this.definedMethods[property.Setter];
            type.Properties.Add(declared);
            index = index + 1;
        }

        return type;
    }

    @explicitThis fun declareConstructor(this, definition: ClassDefinition, constructor: ClassConstructor, type: TypeDefinition) {
        let attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        let declared = MethodDefinition(".ctor", attributes, this.system("System.Void"));

        let index = 0;
        while (index < constructor.Signature.Parameters.Count) {
            let parameter = constructor.Signature.Parameters[index];
            declared.Parameters.Add(ParameterDefinition(parameter.Name, ParameterAttributes.None, this.type(parameter.Type)));
            index = index + 1;
        }

        type.Methods.Add(declared);
        this.definedConstructors[constructor] = declared;
        this.functions[constructor.Function] = declared;
        this.prologues[constructor.Function] = this.baseConstructor(definition);
        this.pending.Add(constructor.Function);
    }

    // How a method is declared: a method that implements an interface method by its name
    // is virtual and final, as C# makes it, so the CLR maps it to the interface's.
    @explicitThis fun declareClassMethod(this, definition: ClassDefinition, method: ClassMethod, type: TypeDefinition) {
        let attributes = MethodAttributes.Public | MethodAttributes.HideBySig;
        if (method.IsStatic) attributes = attributes | MethodAttributes.Static;
        else if (definition.IsInterface or method.IsAbstract) attributes = attributes | MethodAttributes.Abstract | MethodAttributes.Virtual | MethodAttributes.NewSlot;
        else if (method.IsOverride) attributes = attributes | MethodAttributes.Virtual;
        else if (method.IsVirtual) attributes = attributes | MethodAttributes.Virtual | MethodAttributes.NewSlot;
        else if (definition.Interfaces.Count > 0) attributes = attributes | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final;

        if (method.IsAbstract) {
            if (definition.IsInterface == false) type.IsAbstract = true;
        }

        // A generic method's type parameters come first, as its signature uses them.
        let declared = MethodDefinition(method.Name, attributes, this.system("System.Void"));
        let index = 0;
        while (index < method.GenericParameters.Count) {
            let parameter = GenericParameter(method.GenericParameters[index].Name, declared);
            declared.GenericParameters.Add(parameter);
            this.definedParameters[method.GenericParameters[index]] = parameter;
            index = index + 1;
        }

        declared.ReturnType = this.type(method.Signature.ReturnType);

        index = 1;
        if (method.IsStatic) index = 0;
        while (index < method.Signature.Parameters.Count) {
            let parameter = method.Signature.Parameters[index];
            declared.Parameters.Add(ParameterDefinition(parameter.Name, ParameterAttributes.None, this.type(parameter.Type)));
            index = index + 1;
        }

        type.Methods.Add(declared);
        this.definedMethods[method] = declared;
        this.functions[method.Function] = declared;
        if (method.IsAbstract == false) {
            if (definition.IsInterface == false) this.pending.Add(method.Function);
        }
    }

    // The constructor of the base an object is made of first: its base's that takes
    // nothing.
    @explicitThis fun baseConstructor(this, definition: ClassDefinition): MethodReference {
        if (definition.Base == null) return this.objectConstructor();

        if (definition.Base is defined of ClassDefinition) {
            this.classDefinition(defined);
            let index = 0;
            while (index < defined.Constructors.Count) {
                if (defined.Constructors[index].Signature.Parameters.Count == 0) return this.classConstructorOf(defined.Constructors[index]);
                index = index + 1;
            }
            return fail("The base of " + definition.Name + " has no constructor that takes nothing.") as MethodReference;
        }

        if (definition.Base is clr of ClrType) {
            let flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            let found = this.references.system(clr).GetConstructor(flags, Type.EmptyTypes);
            if (found == null) fail("The base of " + definition.Name + " has no constructor that takes nothing.");
            return this.method(this.reflected.Method(found));
        }

        return fail("The Cecil platform can't make an object of the base of " + definition.Name + " yet.") as MethodReference;
    }

    // A generic class's own code refers to it as its instance over its own type
    // parameters, as C# does.
    @explicitThis fun selfOf(this, type: TypeDefinition): TypeReference {
        if (type.GenericParameters.Count == 0) return type;
        let made = GenericInstanceType(type);
        let index = 0;
        while (index < type.GenericParameters.Count) {
            made.GenericArguments.Add(type.GenericParameters[index]);
            index = index + 1;
        }
        return made;
    }

    @explicitThis fun ownerOfMember(this, owner: ClassDefinition): TypeReference {
        if (owner == null) return fail("A member of a class isn't in a class.") as TypeReference;
        return this.selfOf(this.classDefinition(owner));
    }

    @explicitThis fun classFieldOf(this, field: ClassField): FieldReference {
        let owner = this.ownerOfMember(field.Owner);
        let declared = this.definedFields[field];
        if (owner is instance of GenericInstanceType) return FieldReference(declared.Name, declared.FieldType, owner);
        return declared;
    }

    @explicitThis fun classMethodOf(this, method: ClassMethod): MethodReference {
        let owner = this.ownerOfMember(method.Owner);
        let declared = this.definedMethods[method];
        if (owner is instance of GenericInstanceType) return this.onType(declared, owner);
        return declared;
    }

    @explicitThis fun classConstructorOf(this, constructor: ClassConstructor): MethodReference {
        let owner = this.ownerOfMember(constructor.Owner);
        let declared = this.definedConstructors[constructor];
        if (owner is instance of GenericInstanceType) return this.onType(declared, owner);
        return declared;
    }

    // ---- Statements

    @explicitThis fun statement(this, body: Body, statement: LIRStatement) {
        if (statement is block of LIRBlock) {
            let index = 0;
            while (index < block.Locals.Count) {
                body.locals[block.Locals[index]] = body.local(this.type(block.Locals[index].Type));
                index = index + 1;
            }
            index = 0;
            while (index < block.Statements.Count) {
                this.statement(body, block.Statements[index]);
                index = index + 1;
            }
            return;
        }
        if (statement is evaluated of Evaluate) {
            this.expression(body, evaluated.Expression);
            if (this.typeOf(evaluated.Expression) != null) body.il.Emit(OpCodes.Pop);
            return;
        }
        if (statement is assigned of SetLocal) {
            this.expression(body, assigned.Value);
            if (assigned.Variable is parameter of LIRParameter) body.il.Emit(OpCodes.Starg, body.parameter(parameter));
            else body.il.Emit(OpCodes.Stloc, body.variable(assigned.Variable));
            return;
        }
        if (statement is assignedGlobal of SetGlobal) {
            this.expression(body, assignedGlobal.Value);
            body.il.Emit(OpCodes.Stsfld, this.global(assignedGlobal.Global));
            return;
        }
        if (statement is written of SetField) {
            this.structureOf(written.Target);
            this.fieldOwner(body, written.Target);
            this.expression(body, written.Value);
            body.il.Emit(OpCodes.Stfld, this.fieldOf(written.Field));
            return;
        }
        if (statement is stored of Store) {
            this.expression(body, stored.Reference);
            this.expression(body, stored.Value);
            body.il.Emit(OpCodes.Stfld, this.boxValue(this.type(this.typeOf(stored.Reference))));
            return;
        }
        if (statement is branch of LIRIf) {
            let otherwise = body.label();
            let end = body.label();
            this.expression(body, branch.Condition);
            body.il.Emit(OpCodes.Brfalse, otherwise);
            this.statement(body, branch.Then);
            body.il.Emit(OpCodes.Br, end);
            body.il.Append(otherwise);
            if (branch.Else != null) this.statement(body, branch.Else);
            body.il.Append(end);
            return;
        }
        if (statement is repeated of LIRLoop) {
            let start = body.label();
            let end = body.label();
            body.il.Append(start);
            body.starts[repeated] = start;
            body.ends[repeated] = end;
            this.statement(body, repeated.Body);
            body.starts.Remove(repeated);
            body.ends.Remove(repeated);
            body.il.Emit(OpCodes.Br, start);
            body.il.Append(end);
            return;
        }
        if (statement is left of LIRBreak) {
            body.il.Emit(OpCodes.Br, body.ends[left.Loop]);
            return;
        }
        if (statement is next of LIRContinue) {
            body.il.Emit(OpCodes.Br, body.starts[next.Loop]);
            return;
        }
        if (statement is returned of LIRReturn) {
            if (returned.Value != null) this.expression(body, returned.Value);
            body.il.Emit(OpCodes.Ret);
            return;
        }
        fail("The Cecil platform can't write a " + statement.GetType().Name + " yet.");
    }

    // ---- Expressions

    @explicitThis fun typeOf(this, expression: LIRExpression): LIRType {
        if (expression is constant of Constant) return constant.Type;
        if (expression is local of GetLocal) return local.Variable.Type;
        if (expression is global of GetGlobal) return global.Global.Type;
        if (expression is field of GetField) return field.Field.Type;
        if (expression is pointer of GetFunction) return LIRFunctionType(pointer.Callable.Signature);
        if (expression is call of LIRCall) return call.Callable.Signature.ReturnType;
        if (expression is indirect of CallIndirect) return indirect.Signature.ReturnType;
        if (expression is made of LIRNew) return made.Structure;
        if (expression is allocated of Allocate) return LIRReference(this.typeOf(allocated.Value));
        if (expression is loaded of LIRLoad) return (this.typeOf(loaded.Reference) as LIRReference).Target;
        if (expression is sequence of Sequence) return this.typeOf(sequence.Result);
        if (expression is address of AddressOf) return address.Type;
        return fail("The Cecil platform can't type a " + expression.GetType().Name + ".") as LIRType;
    }

    @explicitThis fun expression(this, body: Body, expression: LIRExpression) {
        if (expression is constant of Constant) {
            this.constant(body, constant);
            return;
        }
        if (expression is local of GetLocal) {
            if (local.Variable is parameter of LIRParameter) body.il.Emit(OpCodes.Ldarg, body.parameter(parameter));
            else body.il.Emit(OpCodes.Ldloc, body.variable(local.Variable));
            return;
        }
        if (expression is global of GetGlobal) {
            body.il.Emit(OpCodes.Ldsfld, this.global(global.Global));
            return;
        }
        if (expression is pointer of GetFunction) {
            if (pointer.Callable is target of LIRFunction) {
                body.il.Emit(OpCodes.Ldftn, this.function(target));
                return;
            }
            fail("Only a function defined in LIR can be used as a value.");
        }
        if (expression is call of LIRCall) {
            this.call(body, call);
            return;
        }
        if (expression is indirect of CallIndirect) {
            this.callIndirect(body, indirect);
            return;
        }
        if (expression is field of GetField) {
            this.structureOf(field.Target);
            this.fieldOwner(body, field.Target);
            body.il.Emit(OpCodes.Ldfld, this.fieldOf(field.Field));
            return;
        }
        if (expression is made of LIRNew) {
            this.construct(body, made);
            return;
        }
        if (expression is allocated of Allocate) {
            this.expression(body, allocated.Value);
            body.il.Emit(OpCodes.Newobj, this.boxConstructor(this.type(this.typeOf(allocated))));
            return;
        }
        if (expression is loaded of LIRLoad) {
            this.expression(body, loaded.Reference);
            body.il.Emit(OpCodes.Ldfld, this.boxValue(this.type(this.typeOf(loaded.Reference))));
            return;
        }
        if (expression is sequence of Sequence) {
            this.statement(body, sequence.Block);
            this.expression(body, sequence.Result);
            return;
        }
        if (expression is address of AddressOf) {
            this.address(body, address.Variable);
            return;
        }
        fail("The Cecil platform can't write a " + expression.GetType().Name + " yet.");
    }

    // A constant is what it is in IL. A value that only the running process has, an
    // object, has no meaning in an assembly, so it can't be written.
    @explicitThis fun constant(this, body: Body, constant: Constant) {
        let value = constant.Value;
        if (value == null) {
            this.zero(body, constant.Type);
            return;
        }
        if (value is text of String) {
            body.il.Emit(OpCodes.Ldstr, text);
            return;
        }
        if (value is flag of Boolean) {
            if (flag) body.il.Emit(OpCodes.Ldc_I4_1);
            else body.il.Emit(OpCodes.Ldc_I4_0);
            return;
        }
        if (value is number of Int32) {
            body.il.Emit(OpCodes.Ldc_I4, number);
            return;
        }
        if (value is long of Int64) {
            body.il.Emit(OpCodes.Ldc_I8, long);
            return;
        }
        if (value is type of Type) {
            body.il.Emit(OpCodes.Ldtoken, this.type(this.reflected.Type(type)));
            let core: Type = Object;
            body.il.Emit(OpCodes.Call, this.method(this.reflected.Method(core.Assembly.GetType("System.Type").GetMethod("GetTypeFromHandle"))));
            return;
        }

        let kind = value.GetType();
        if (kind.IsEnum or kind.IsPrimitive) {
            let underlying = kind;
            if (kind.IsEnum) underlying = Enum.GetUnderlyingType(kind);
            if (underlying.Name == "Single") {
                body.il.Emit(OpCodes.Ldc_R4, Convert.ToSingle(value));
                return;
            }
            if (underlying.Name == "Double") {
                body.il.Emit(OpCodes.Ldc_R8, Convert.ToDouble(value));
                return;
            }
            body.il.Emit(OpCodes.Ldc_I8, Convert.ToInt64(value));
            if (underlying.Name != "Int64") {
                if (underlying.Name != "UInt64") body.il.Emit(OpCodes.Conv_I4);
            }
            return;
        }

        fail("A value only the running process has, a " + kind.Name + ", can't be written in an assembly.");
    }

    // The default value of a type: null, or a value type's zero.
    @explicitThis fun zero(this, body: Body, type: LIRType) {
        if (this.isValueType(type) == false) {
            body.il.Emit(OpCodes.Ldnull);
            return;
        }
        let written = this.type(type);
        let temporary = body.local(written);
        body.il.Emit(OpCodes.Ldloca, temporary);
        body.il.Emit(OpCodes.Initobj, written);
        body.il.Emit(OpCodes.Ldloc, temporary);
    }

    @explicitThis fun call(this, body: Body, call: LIRCall) {
        let index = 0;
        while (index < call.Arguments.Count) {
            if (index == 0) {
                if (this.takesAddress(call.Callable)) this.address(body, call.Arguments[index]);
                else this.expression(body, call.Arguments[index]);
            }
            else this.expression(body, call.Arguments[index]);
            index = index + 1;
        }
        this.invoke(body, call.Callable);
    }

    @explicitThis fun takesAddress(this, callable: ICallable): Boolean {
        if (callable is clr of ClrMethod) return clr.TakesAddress;
        if (callable is member of InstanceMember) return this.takesAddress(member.Member);
        if (callable is instance of MethodInstance) return this.takesAddress(instance.Method);
        return false;
    }

    // Calls what is called, its arguments on the stack.
    @explicitThis fun invoke(this, body: Body, callable: ICallable) {
        if (callable is function of LIRFunction) {
            if (this.operation(body, function)) return;
            let declared = this.function(function);
            if (this.instanceMethods.ContainsKey(function)) body.il.Emit(OpCodes.Callvirt, declared);
            else body.il.Emit(OpCodes.Call, declared);
            return;
        }
        if (callable is clr of ClrMethod) {
            this.callClr(body, clr, this.method(clr));
            return;
        }
        if (callable is code of ClrCode) {
            this.code(body, code);
            return;
        }
        if (callable is defined of ClassMethod) {
            if (defined.IsStatic) body.il.Emit(OpCodes.Call, this.classMethodOf(defined));
            else body.il.Emit(OpCodes.Callvirt, this.classMethodOf(defined));
            return;
        }
        if (callable is constructor of ClassConstructor) {
            body.il.Emit(OpCodes.Newobj, this.classConstructorOf(constructor));
            return;
        }
        if (callable is access of ClassFieldAccess) {
            if (access.IsSet) body.il.Emit(OpCodes.Stfld, this.classFieldOf(access.Field));
            else body.il.Emit(OpCodes.Ldfld, this.classFieldOf(access.Field));
            return;
        }
        if (callable is member of InstanceMember) {
            let owner = this.type(member.Owner);
            if (member.Member is onDefinition of ClrMethod) {
                this.callClr(body, onDefinition, this.onType(this.references.method(onDefinition), owner));
                return;
            }
            if (member.Member is method of ClassMethod) {
                this.classMethodOf(method);
                let reference = this.onType(this.definedMethods[method], owner);
                if (method.IsStatic) body.il.Emit(OpCodes.Call, reference);
                else body.il.Emit(OpCodes.Callvirt, reference);
                return;
            }
            if (member.Member is made of ClassConstructor) {
                this.classConstructorOf(made);
                body.il.Emit(OpCodes.Newobj, this.onType(this.definedConstructors[made], owner));
                return;
            }
            if (member.Member is field of ClassFieldAccess) {
                this.classFieldOf(field.Field);
                let declared = this.definedFields[field.Field];
                let reference = FieldReference(declared.Name, declared.FieldType, owner);
                if (field.IsSet) body.il.Emit(OpCodes.Stfld, reference);
                else body.il.Emit(OpCodes.Ldfld, reference);
                return;
            }
        }
        if (callable is instance of MethodInstance) {
            if (instance.Method is generic of ClrMethod) {
                this.callClr(body, generic, this.instantiated(this.method(generic), instance));
                return;
            }
            let isStatic = false;
            let element: MethodReference = null;
            if (instance.Method is own of ClassMethod) {
                element = this.classMethodOf(own);
                isStatic = own.IsStatic;
            }
            if (instance.Method is onInstance of InstanceMember) {
                if (onInstance.Member is inner of ClassMethod) {
                    this.classMethodOf(inner);
                    element = this.onType(this.definedMethods[inner], this.type(onInstance.Owner));
                    isStatic = inner.IsStatic;
                }
            }
            if (element != null) {
                if (isStatic) body.il.Emit(OpCodes.Call, this.instantiated(element, instance));
                else body.il.Emit(OpCodes.Callvirt, this.instantiated(element, instance));
                return;
            }
        }
        fail("The Cecil platform can't call a " + callable.GetType().Name + " yet.");
    }

    @explicitThis fun instantiated(this, element: MethodReference, instance: MethodInstance): MethodReference {
        let made = GenericInstanceMethod(element);
        let index = 0;
        while (index < instance.Arguments.Count) {
            made.GenericArguments.Add(this.type(instance.Arguments[index]));
            index = index + 1;
        }
        return made;
    }

    // A constructor makes its object; a value type's own method is called directly, on
    // the address of its object; any other method of an object is called through it.
    @explicitThis fun callClr(this, body: Body, clr: ClrMethod, reference: MethodReference) {
        if (clr.IsConstructor) body.il.Emit(OpCodes.Newobj, reference);
        else if (clr.IsStatic or clr.TakesAddress) body.il.Emit(OpCodes.Call, reference);
        else body.il.Emit(OpCodes.Callvirt, reference);
    }

    // A function that is an operation of the platform, as a cast, is emitted in place.
    @explicitThis fun operation(this, body: Body, function: LIRFunction): Boolean {
        let index = 0;
        while (index < function.Metadata.Count) {
            let metadata = function.Metadata[index];
            if (metadata is cast of CastDown) {
                body.il.Emit(OpCodes.Unbox_Any, this.type(cast.Target));
                return true;
            }
            if (metadata is test of TypeTest) {
                body.il.Emit(OpCodes.Isinst, this.type(test.Target));
                body.il.Emit(OpCodes.Ldnull);
                body.il.Emit(OpCodes.Cgt_Un);
                return true;
            }
            index = index + 1;
        }
        return false;
    }

    @explicitThis fun isOperation(this, function: LIRFunction): Boolean {
        let index = 0;
        while (index < function.Metadata.Count) {
            if (function.Metadata[index] is cast of CastDown) return true;
            if (function.Metadata[index] is test of TypeTest) return true;
            index = index + 1;
        }
        return false;
    }

    // A function whose code is IL, emitted in place of the call.
    @explicitThis fun code(this, body: Body, code: ClrCode) {
        let locals = List[VariableDefinition]();
        let index = 0;
        while (index < code.Locals.Count) {
            locals.Add(body.local(this.type(code.Locals[index])));
            index = index + 1;
        }

        index = 0;
        while (index < code.Instructions.Count) {
            let instruction = code.Instructions[index];
            if (this.opcodes.ContainsKey(instruction.OpCode) == false) fail("There is no instruction with the opcode " + instruction.OpCode + ".");
            let opcode = this.opcodes[instruction.OpCode];
            let operand = instruction.Operand;

            if (operand == null) body.il.Emit(opcode);
            else if (operand is number of Int32) body.il.Emit(opcode, number);
            else if (operand is long of Int64) body.il.Emit(opcode, long);
            else if (operand is text of String) body.il.Emit(opcode, text);
            else if (operand is local of ClrLocal) body.il.Emit(opcode, locals[local.Index]);
            else if (operand is field of ClrField) body.il.Emit(opcode, this.field(field));
            else if (operand is method of ClrMethod) body.il.Emit(opcode, this.method(method));
            else if (operand is function of LIRFunction) body.il.Emit(opcode, this.function(function));
            else if (operand is defined of ClassMethod) body.il.Emit(opcode, this.classMethodOf(defined));
            // The token of a class is of its definition, as a type object of a generic
            // class with no arguments is.
            else if (operand is token of ClassDefinition) {
                if (opcode.Name == "ldtoken") body.il.Emit(opcode, this.classDefinition(token) as TypeReference);
                else body.il.Emit(opcode, this.type(token));
            }
            else if (operand is type of LIRType) body.il.Emit(opcode, this.type(type));
            else fail("The Cecil platform can't write an instruction whose operand is a " + operand.GetType().Name + " yet.");
            index = index + 1;
        }
    }

    @explicitThis fun callIndirect(this, body: Body, call: CallIndirect) {
        // The callee goes on top of the stack, but is evaluated first.
        let callee = body.local(this.system("System.IntPtr"));
        this.expression(body, call.Callee);
        body.il.Emit(OpCodes.Stloc, callee);

        let index = 0;
        while (index < call.Arguments.Count) {
            this.expression(body, call.Arguments[index]);
            index = index + 1;
        }

        let site = CallSite(this.type(call.Signature.ReturnType));
        index = 0;
        while (index < call.Signature.Parameters.Count) {
            site.Parameters.Add(ParameterDefinition(this.type(call.Signature.Parameters[index].Type)));
            index = index + 1;
        }

        body.il.Emit(OpCodes.Ldloc, callee);
        body.il.Emit(OpCodes.Calli, site);
    }

    // The address of a variable, or of a temporary holding a value that isn't one.
    @explicitThis fun address(this, body: Body, expression: LIRExpression) {
        if (expression is local of GetLocal) {
            if (local.Variable is parameter of LIRParameter) body.il.Emit(OpCodes.Ldarga, body.parameter(parameter));
            else body.il.Emit(OpCodes.Ldloca, body.variable(local.Variable));
            return;
        }
        if (expression is global of GetGlobal) {
            body.il.Emit(OpCodes.Ldsflda, this.global(global.Global));
            return;
        }
        if (expression is field of GetField) {
            this.fieldOwner(body, field.Target);
            body.il.Emit(OpCodes.Ldflda, this.fieldOf(field.Field));
            return;
        }
        if (expression is loaded of LIRLoad) {
            this.expression(body, loaded.Reference);
            body.il.Emit(OpCodes.Ldflda, this.boxValue(this.type(this.typeOf(loaded.Reference))));
            return;
        }
        if (expression is sequence of Sequence) {
            if (AddressOf.Has(sequence.Result)) {
                this.statement(body, sequence.Block);
                this.address(body, sequence.Result);
                return;
            }
        }

        let temporary = body.local(this.type(this.typeOf(expression)));
        this.expression(body, expression);
        body.il.Emit(OpCodes.Stloc, temporary);
        body.il.Emit(OpCodes.Ldloca, temporary);
    }
}
