// Z#'s own modules, from std:modules: globals initialized in their order, and functions
// and classes that belong to the module they are defined in. A module is defined only in
// a document, and one uses another's members as anything does.
import "std:prelude";
import { print } from "std:io";
import { String, Int32 } from "core:runtime";

module Shop {
    let tax: Int32 = 10;
    let sold = 0;
    let label: String = "shop " + tax;

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

module Greetings {
    let greeting = "hello";
    fun hello(): String { return greeting + " " + Shop.tax; }
}

print(Shop.label);
print("total " + Shop.sell(Shop.Item(200)));
print("sold " + Shop.sold);
Shop.tax = 20;
print("total " + Shop.sell(Shop.Item(100)) + ", sold " + Shop.sold);
print(Greetings.hello());
