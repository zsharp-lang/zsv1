// std:zsharp/clr — the capabilities every CLR platform has, given to them by traits: the
// running process, or an assembly written for a version of .NET. Each marks LIR with the
// CLR's metadata, which every CLR platform realizes.

import { Polymorphism, PlatformInterfaces } from "std:zsharp/capabilities";
import { Implementation, implement } from "std:traits";
import { entrypoint } from "std:framework";
import { ClrPlatform, Polymorphic, MethodOf, CastDown, TypeTest, explicitThis } from "core:runtime";
import { Structure, Function, Type as LIRType } from "std:zsharp/lir";

// The CLR has polymorphism: a polymorphic structure is a CLR class or interface, and a
// method of one is a method of the class.
def ClrPolymorphism of Implementation[Polymorphism, ClrPlatform] {
    let value: ClrPlatform;

    @explicitThis new(this, value: ClrPlatform) { this.value = value; }

    @explicitThis fun objects(this, structure: Structure) {
        structure.Metadata.Add(Polymorphic());
    }

    @explicitThis fun interfaces(this, structure: Structure) {
        let mark = Polymorphic();
        mark.IsInterface = true;
        structure.Metadata.Add(mark);
    }

    @explicitThis fun implements(this, structure: Structure, implemented: Structure) {
        let index = 0;
        while (index < structure.Metadata.Count) {
            if (structure.Metadata[index] is mark of Polymorphic) mark.Interfaces.Add(implemented);
            index = index + 1;
        }
    }

    @explicitThis fun method(this, function: Function, owner: Structure) {
        function.Metadata.Add(MethodOf(owner));
    }

    @explicitThis fun cast(this, function: Function, to: Structure) {
        function.Metadata.Add(CastDown(to));
    }

    @explicitThis fun test(this, function: Function, target: Structure) {
        function.Metadata.Add(TypeTest(target));
    }
}

// A CLR class can implement the CLR's own interfaces, as the toolchain's are: the class
// a polymorphic structure is implements them.
def ClrInterfaces of Implementation[PlatformInterfaces, ClrPlatform] {
    let value: ClrPlatform;

    @explicitThis new(this, value: ClrPlatform) { this.value = value; }

    @explicitThis fun implements(this, structure: Structure, implemented: LIRType) {
        let index = 0;
        while (index < structure.Metadata.Count) {
            if (structure.Metadata[index] is mark of Polymorphic) mark.Implements.Add(implemented);
            index = index + 1;
        }
    }
}

// Importing it makes the implementations known, as loading their definitions does where
// it is built.
@entrypoint
fun install() {
    implement(Polymorphism, ClrPlatform, ClrPolymorphism);
    implement(PlatformInterfaces, ClrPlatform, ClrInterfaces);
}
