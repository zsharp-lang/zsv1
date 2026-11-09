// Z#'s interfaces, static members and type objects, from std:oop: a call through an
// interface, a cast down and a type test, and an interface's class methods, which each
// class implementing it has, reached through a class used as a value.
import "std:prelude";
import { print } from "std:io";
import { Interface, static, class } from "std:oop";
import { String, Int32 } from "core:runtime";

def Shape of Interface {
    @class fun unit(): Shape;
    @class fun kind(): String;
    fun area(): Int32;
}

def Square : Shape {
    @static let made: Int32 = 0;
    let side: Int32;

    new(this, side: Int32) { made = made + 1; this.side = side; }

    @class fun unit(): Shape { return Square(1); }
    @class fun kind(): String { return "square"; }
    fun area(): Int32 { return side * side; }
}

def Circle : Shape {
    let radius: Int32;

    new(this, radius: Int32) { this.radius = radius; }

    @class fun unit(): Shape { return Circle(1); }
    @class fun kind(): String { return "circle"; }
    fun area(): Int32 { return 3 * radius * radius; }
}

fun describe(kind: Shape.Class): String { return "a unit " + kind.kind() + " has area " + kind.unit().area(); }

fun side(shape: Shape): Int32 {
    if (shape is square of Square) return square.side;
    return 0;
}

print(describe(Square));
print(describe(Circle));

let shape: Shape = Square(4);
print("area " + shape.area() + ", side " + side(shape) + ", squares made " + Square.made);
