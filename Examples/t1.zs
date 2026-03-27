fun zsSource(filename: String) {
    let result = "${filename}.zs";
    return result;
}

module Main;

import { printAndReturn } from zsSource("utils");


class Derived : Base
{
    let x: I32;
    let "_{"y"}": @ct fun() { return I32; }();
}


@printAndReturn
class Base
{
    @ct
    for (let name in [
        "x", "y"
    ])
        get "${name}": I32;

    @cached
    @route()
    fun getX(): I32 {
        return this.x;
    }
}

@ct
let BaseClass = Base;


class User of ORMModel 
{
    @primaryKey
    let id: UUID.4;

    let username: String[minLength=3, maxLength=20];
}
