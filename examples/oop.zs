// Z#'s own classes, from std:oop: fields reached by name in methods, `this` when a
// method names it, and named constructors.
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

def Counter {
    let count: Int32 = 0;

    new() { }
    new from(this, start: Int32) { this.count = start; }

    fun next(): Int32 {
        count = count + 1;
        return count;
    }
}

let counter = Counter.from(41);
print("The answer is " + counter.next());
