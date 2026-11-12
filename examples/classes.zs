import { print } from "std:io";
import { String } from "core:runtime";
import { static, explicitThis } from "pkg:clr/hir";

module Program {
    def Greeter {
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

    def Other {
        fun greet(name: String): String {
            return name;
        }
    }
}

print(Program.Greeter.create("Hello from Z#").greet());
