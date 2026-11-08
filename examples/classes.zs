import { print } from "std:io";
import { String, explicitThis, static } from "core:runtime";

module Program {
    class Greeter {
        let name: String;
        let friend: Other;

        @explicitThis
        new(self, name: String) {
            self.name = name;
            self.friend = Other();
        }

        @explicitThis
        fun greet(self): String {
            return self.friend.greet(self.name);
        }

        @static
        fun create(name: String): Greeter {
            return Greeter(name);
        }
    }

    class Other {
        fun greet(name: String): String {
            return name;
        }
    }
}

print(Program.Greeter.create("Hello from Z#").greet());
