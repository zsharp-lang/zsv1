// What a CLR handle stands for in the assemblies an assembly is written against: the
// running process's, if it targets no version, or a version's reference assemblies, which
// are read for it. What they don't have, as the toolchain's own assemblies, is read from
// the running process's, by the file of the assembly that defines it, and what their
// signatures name is the version's. Each gives the definition, imported into the assembly
// being written; a member of a generic instance is the platform's to make of it.

// Fails the platform's work, with why.
fun fail(message: String): Object {
    let failed: IResult[Object, Error] = Result[Object].Error(message);
    return failed.Unwrap();
}

def References of DotNet_Class {
    let main: ModuleDefinition;
    let version: String;
    let index: Dictionary[String, TypeDefinition];
    let read: Dictionary[String, Boolean];
    let types: Dictionary[ClrType, TypeReference];
    let methods: Dictionary[ClrMethod, MethodReference];
    let fields: Dictionary[ClrField, FieldReference];

    @explicitThis new(this, main: ModuleDefinition, version: String) {
        this.main = main;
        this.version = version;
        this.index = Dictionary[String, TypeDefinition]();
        this.read = Dictionary[String, Boolean]();
        this.types = Dictionary[ClrType, TypeReference]();
        this.methods = Dictionary[ClrMethod, MethodReference]();
        this.fields = Dictionary[ClrField, FieldReference]();
        if (version != null) this.pack(version);
    }

    @explicitThis fun live(this): Boolean { return this.version == null; }

    // ---- The assemblies of a version

    // The reference assemblies of a version, as `net8.0`: those of the newest pack of it
    // installed with the running .NET.
    @explicitThis fun pack(this, version: String) {
        let root = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Object.Assembly.Location))));
        let packs = Path.Combine(root, "packs", "Microsoft.NETCore.App.Ref");
        let wanted = version.Substring(3) + ".";

        let chosen: String = null;
        let best: Version = null;
        let candidates = Directory.GetDirectories(packs);
        let index = 0;
        while (index < candidates.Length) {
            let name = Path.GetFileName(candidates[index]);
            if (name.StartsWith(wanted)) {
                if (name.Contains("-") == false) {
                    let parsed = Version.Parse(name);
                    if (best == null or parsed.CompareTo(best) > 0) {
                        best = parsed;
                        chosen = candidates[index];
                    }
                }
            }
            index = index + 1;
        }

        if (chosen == null) fail("No reference pack of " + version + " is installed in " + packs + ".");

        let files = Directory.GetFiles(Path.Combine(chosen, "ref", version), "*.dll");
        index = 0;
        while (index < files.Length) {
            this.add(ModuleDefinition.ReadModule(files[index]));
            index = index + 1;
        }
    }

    // Indexes the types of a module by their full names, the first of a name kept.
    @explicitThis fun add(this, source: ModuleDefinition) {
        let index = 0;
        while (index < source.Types.Count) {
            this.addType(source.Types[index]);
            index = index + 1;
        }
    }

    @explicitThis fun addType(this, type: TypeDefinition) {
        if (this.index.ContainsKey(type.FullName) == false) this.index[type.FullName] = type;
        let index = 0;
        while (index < type.NestedTypes.Count) {
            this.addType(type.NestedTypes[index]);
            index = index + 1;
        }
    }

    // Reads the file of an assembly of the running process, for what a version's
    // reference assemblies don't have. One the process made has no file, and can't be
    // referred to by an assembly.
    @explicitThis fun readLoaded(this, name: String) {
        if (this.read.ContainsKey(name)) return;
        this.read[name] = true;
        let file = Assemblies.FileOf(this.loaded(name));
        if (file == null) fail("The assembly " + name + " has no file to read.");
        this.add(ModuleDefinition.ReadModule(file));
    }

    @explicitThis fun loaded(this, name: String): Assembly {
        let assemblies = AppDomain.CurrentDomain.GetAssemblies();
        let index = 0;
        while (index < assemblies.Length) {
            if (assemblies[index].GetName().Name == name) {
                if (assemblies[index].IsDynamic) fail("The assembly " + name + " was made by the running process, so an assembly can't refer to it.");
                return assemblies[index];
            }
            index = index + 1;
        }
        return fail("No assembly " + name + " is loaded.") as Assembly;
    }

    @explicitThis fun definition(this, type: ClrType): TypeDefinition {
        let key = cecilName(type);
        if (this.index.ContainsKey(key) == false) this.readLoaded(type.Assembly);
        if (this.index.ContainsKey(key) == false) fail("The assemblies of " + this.version + " have no type " + key + ".");
        return this.index[key];
    }

    // ---- The running process

    @explicitThis fun system(this, type: ClrType): Type {
        return this.moduleOf(type).ResolveType(type.Token);
    }

    @explicitThis fun moduleOf(this, entity: ClrEntity): ReflectedModule {
        let modules = this.loaded(entity.Assembly).GetModules();
        let index = 0;
        while (index < modules.Length) {
            if (modules[index].ScopeName == entity.Module) return modules[index];
            index = index + 1;
        }
        return fail("The assembly " + entity.Assembly + " has no module " + entity.Module + ".") as ReflectedModule;
    }

    @explicitThis fun declared(this): BindingFlags {
        return BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    }

    // ---- Handles

    // The definition of the type a member is declared by, a generic instance's included.
    @explicitThis fun owner(this, type: LIRType): ClrType {
        if (type is let instance: GenericInstance) return instance.Definition as ClrType;
        return type as ClrType;
    }

    @explicitThis fun type(this, type: ClrType): TypeReference {
        if (this.types.ContainsKey(type)) return this.types[type];

        let result: TypeReference = null;
        if (this.live()) result = this.main.ImportReference(this.system(type));
        else result = this.main.ImportReference(this.definition(type));

        this.types[type] = result;
        return result;
    }

    // A method as it is defined: by its token in the running process, or else by its name
    // and declared signature.
    @explicitThis fun method(this, method: ClrMethod): MethodReference {
        if (this.methods.ContainsKey(method)) return this.methods[method];

        let owner = this.owner(method.DeclaringType);
        let result: MethodReference = null;

        if (this.live()) {
            let system = this.system(owner);
            if (method.IsConstructor) {
                let constructors = system.GetConstructors(this.declared());
                let index = 0;
                while (index < constructors.Length) {
                    if (constructors[index].MetadataToken == method.Token) result = this.main.ImportReference(constructors[index]);
                    index = index + 1;
                }
            }
            else {
                let candidates = system.GetMethods(this.declared());
                let index = 0;
                while (index < candidates.Length) {
                    if (candidates[index].MetadataToken == method.Token) result = this.main.ImportReference(candidates[index]);
                    index = index + 1;
                }
            }
        }
        else {
            let candidates = this.definition(owner).Methods;
            let index = 0;
            while (index < candidates.Count) {
                if (this.matches(candidates[index], method)) result = candidates[index];
                index = index + 1;
            }
        }

        if (result == null) fail("The assemblies an assembly is written against have no method " + method.Name + " of " + cecilName(owner) + " as it was used.");
        if (this.live() == false) result = this.retargetMethod(result);

        this.methods[method] = result;
        return result;
    }

    // ---- Retargeting

    // A type of a member's signature, as the version has it: a member read from the
    // running process's assemblies names the running .NET's types, which are the
    // version's, by their names.
    @explicitThis fun retarget(this, type: TypeReference): TypeReference {
        if (type is let parameter: GenericParameter) return type;
        if (type is let instance: GenericInstanceType) {
            let made = GenericInstanceType(this.retarget(instance.ElementType));
            let index = 0;
            while (index < instance.GenericArguments.Count) {
                made.GenericArguments.Add(this.retarget(instance.GenericArguments[index]));
                index = index + 1;
            }
            return made;
        }
        if (type is let array: ArrayType) return ArrayType(this.retarget(array.ElementType), array.Rank);
        if (type is let reference: ByReferenceType) return ByReferenceType(this.retarget(reference.ElementType));
        if (type is let pointer: PointerType) return PointerType(this.retarget(pointer.ElementType));
        if (type is let required: RequiredModifierType) return RequiredModifierType(this.retarget(required.ModifierType), this.retarget(required.ElementType));
        if (type is let optional: OptionalModifierType) return OptionalModifierType(this.retarget(optional.ModifierType), this.retarget(optional.ElementType));
        if (this.index.ContainsKey(type.FullName)) return this.main.ImportReference(this.index[type.FullName]);
        return this.main.ImportReference(type);
    }

    @explicitThis fun retargetMethod(this, method: MethodReference): MethodReference {
        if (method is let instance: GenericInstanceMethod) {
            let made = GenericInstanceMethod(this.retargetMethod(instance.ElementMethod));
            let index = 0;
            while (index < instance.GenericArguments.Count) {
                made.GenericArguments.Add(this.retarget(instance.GenericArguments[index]));
                index = index + 1;
            }
            return made;
        }

        let reference = MethodReference(method.Name, this.retarget(method.ReturnType), this.retarget(method.DeclaringType));
        reference.HasThis = method.HasThis;
        reference.ExplicitThis = method.ExplicitThis;
        reference.CallingConvention = method.CallingConvention;

        let index = 0;
        while (index < method.GenericParameters.Count) {
            reference.GenericParameters.Add(GenericParameter(method.GenericParameters[index].Name, reference));
            index = index + 1;
        }

        index = 0;
        while (index < method.Parameters.Count) {
            reference.Parameters.Add(ParameterDefinition(this.retarget(method.Parameters[index].ParameterType)));
            index = index + 1;
        }
        return reference;
    }

    @explicitThis fun matches(this, candidate: MethodDefinition, method: ClrMethod): Boolean {
        if (candidate.Name != method.Name) return false;
        if (candidate.GenericParameters.Count != method.GenericArity) return false;
        if (candidate.Parameters.Count != method.Parameters.Count) return false;

        let index = 0;
        while (index < candidate.Parameters.Count) {
            if (cecilKeyOf(candidate.Parameters[index].ParameterType) != keyOf(method.Parameters[index])) return false;
            index = index + 1;
        }
        return true;
    }

    @explicitThis fun field(this, field: ClrField): FieldReference {
        if (this.fields.ContainsKey(field)) return this.fields[field];

        let owner = this.owner(field.DeclaringType);
        let result: FieldReference = null;

        if (this.live()) {
            let candidates = this.system(owner).GetFields(this.declared());
            let index = 0;
            while (index < candidates.Length) {
                if (candidates[index].MetadataToken == field.Token) result = this.main.ImportReference(candidates[index]);
                index = index + 1;
            }
        }
        else {
            let candidates = this.definition(owner).Fields;
            let index = 0;
            while (index < candidates.Count) {
                if (candidates[index].Name == field.Name) result = candidates[index];
                index = index + 1;
            }
        }

        if (result == null) fail("The assemblies an assembly is written against have no field " + field.Name + " of " + cecilName(owner) + ".");
        if (this.live() == false) result = FieldReference(result.Name, this.retarget(result.FieldType), this.retarget(result.DeclaringType));

        this.fields[field] = result;
        return result;
    }
}
