# Modules and documents

## Modules

A module holds definitions: its globals, which are its `let`s, its functions and its
classes. A global is initialized in its order, as the module is built, and what is in a
module reaches the module's other members by their names.

```zs
import "std:prelude";
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

module Shop {
    let tax: Int32 = 10;
    let sold = 0;
    let label: String = "shop with tax " + tax;

    def Item {
        let price: Int32;
        new(this, price: Int32) { this.price = price; }
        fun total(): Int32 { return price + price * tax / 100; }
    }

    fun sell(item: Item): Int32 {
        sold = sold + 1;
        return item.total();
    }
}

print(Shop.label);
print("total " + Shop.sell(Shop.Item(200)));
Shop.tax = 20;
print("total " + Shop.sell(Shop.Item(100)) + ", sold " + Shop.sold);
```

```output
shop with tax 10
total 220
total 120, sold 2
```

A module holds no module. A library is a module too, which a project compiles from its
files.

## Documents

A document is a file of Z#, which another one imports by its path, relative to itself. It
gives what it marks with `@export`, from `core:zsharp`:

```zs fragment
// shapes.zs
import "std:prelude";
import { export } from "core:zsharp";
import { Int32 } from "core:runtime";

@export
def Square {
    let side: Int32;
    new(this, side: Int32) { this.side = side; }
    fun area(): Int32 { return side * side; }
}
```

```zs fragment
// main.zs
import "std:prelude";
import { print } from "std:io";
import { Square } from "shapes.zs";

print(Square(4).area());
```

`@export("Name")` exports a definition under another name. A document runs once, however
often it is imported, and two documents that import each other are an error.
