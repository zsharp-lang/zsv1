# Modules

As a script grows, its definitions start to belong together: the prices and items of a
shop, the parts of a parser. A module groups them under one name, so they reach each
other by name and the rest of the script reaches them through the module.

## Defining one

`module` defines one. It holds definitions: its globals, which are its `let`s, its
functions and its classes:

```zs
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

Inside the module, each member reaches the others by name: `sell` uses `sold`, and
`Item`'s `total` uses `tax`. Outside it, they are the module's members, as
`Shop.sell` and `Shop.Item`.

A global is initialized in its order, as the module is built, so `label` sees the `tax`
before it. A module holds no module.

## Modules and libraries

A library is a module too: the standard library's are, and so is what a
[project](projects.md) builds. A module's functions are also what a function value is made
of, as [functions](functions.md#functions-as-values) shows.

Splitting a script into several files is a matter of [documents](documents.md), which is
the next page.
