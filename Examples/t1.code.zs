let module = Module("Main");

let builder = ModuleBuilder(module);

let scope = Scope();


{
    scope.Add(
        "Derived", 
        DefaultMetaclass(fun(instance) {
            return Specification() {
                name: "Derived",
                bases: [scope.get("Base")],
                body: [
                    Local(
                        name: "x", 
                        type: I32,
                        init: null
                    ),
                    Local(
                        name: InterpolatedString(
                            strings: ["_", ""],
                            values: ["y"]
                        ),
                        type: fun() { return I32; }()
                    )
                ]
            };
        })
    );
}

{
    scope.Add(
        "Base", 
        DefaultMetaclass(fun(instance) {
            return Specification() {
                name: "Base",
                bases: [],
                body: []
            };
        })
        |> printAndReturn
    );
}
