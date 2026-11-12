// std:zsharp/lowering — how a definition lowers into the definition that holds it.
//
// What holds definitions, as a module, lowers them into what it lowers to: it binds each
// of their COs to itself as it is loaded, and lowers it into its LIR definition with
// `ir.CompileDefinition`. A member lowered first by itself, as a function a call
// reaches, asks what holds it. Belonging is how a definition is lowered, so it is in the
// COs, and the nodes stay structure.

import { Object, Boolean, Int32, List, DotNet_Interface, DotNet_Class } from "core:runtime"; import { static, public } from "std:zsharp/package/dotnet";
import { Module as LIRModule, Definition as LIRDefinition, Statement as LIRStatement, Initialized, Metadata, KeptAnnotation } from "std:zsharp/lir";
import { Annotated, Kept } from "std:zsharp/hir";

// What holds definitions, as its CO: they lower into its LIR module for a target, which
// it lowers with them.
def Holder of DotNet_Interface {
    fun module(target: Object): LIRModule;
}

// A definition that what holds it lowers, as its CO.
def Held of DotNet_Interface {
    fun hold(holder: Holder);
}

@public def Initializing of DotNet_Class {
    // Adds code to what a definition is lowered into runs to initialize it, if it has an
    // initializer, which is a capability of it. Whether it does.
    @public @static fun into(owner: LIRDefinition, statement: LIRStatement): Boolean {
        if (owner is let initialized: Initialized) {
            initialized.Initializer.Block.Statements.Add(statement);
            return true;
        }
        return false;
    }
}

// Keeps what is said of a node to keep, an annotation whose type is kept, on the metadata
// of what it lowers to, which a platform writes its own way.
fun keep(node: Object, metadata: List[Metadata]) {
    if (node is let annotated: Annotated) {
        let index = 0;
        while (index < annotated.Annotations.Count) {
            if (annotated.Annotations[index] is let kept: Kept) metadata.Add(KeptAnnotation(kept));
            index = index + 1;
        }
    }
}
