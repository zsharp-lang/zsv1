class Foo {
    fun bar(): Bar; // error. document level is linear and does not support forward references
}

class Bar {

}


module M;

// From here on, we're in module M


class Foo {
    fun bar(): Bar; // ok. inside module, forward references are supported

    fun main(): Void {
        let b = this.bar();

        b.baz(this);

        return;
    }

    fun inner(): Bar.Inner {
        return Bar.Inner();
    }

    class Inner {}
}

class Baz : Bar.Inner {}

class Bar {
    class Inner : Foo.Inner {}

    fun baz(foo: Foo) {
        return foo.bar() == this;
    }
}



class Resource : Provider[Resource] {
    fun provide(): Resource {
        return this;
    }
}
