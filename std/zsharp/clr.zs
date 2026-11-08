// std:zsharp/clr — the capabilities the CLR runtime has, given to it by traits. Each
// marks LIR with the runtime's own metadata, which the runtime realizes.

import { Polymorphism } from "std:zsharp/capabilities";
import { Implementation } from "std:traits";
import { Runtime, Polymorphic, MethodOf, explicitThis } from "core:runtime";
import { Structure, Function } from "std:zsharp/lir";

// The CLR has polymorphism: a polymorphic structure is a CLR class or interface, and a
// method of one is a method of the class.
def RuntimePolymorphism of Implementation[Polymorphism, Runtime] {
    let value: Runtime;

    @explicitThis new(this, value: Runtime) { this.value = value; }

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
}
