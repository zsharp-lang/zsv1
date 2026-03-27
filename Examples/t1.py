from typing import Any, Callable


scope: dict[str, Any]

class InterpolatedString:
    def __init__(self, *, strings: list[str], values: list[object]) -> None: ...

class Module: ...
class ModuleBuilder: ...
class ClassBuilder:
    def add_base(self, base: object): ...
    def add_field(self, *, name: str, type: object | None, init: object | None): ...

    def get_specification(self): ...

class Type:
    def __init__(self, add_task: Callable) -> None: ...

DefaultMetaClass = Type

def printAndReturn[T](v: T) -> T:
    print(v)
    return v

class Main(Module):
    __builder__ = ModuleBuilder()

    @staticmethod
    def __build__Derived(build):
        __builder__ = ClassBuilder()

        __builder__.add_base(
            Main.Base
        )

        __builder__.add_field(
            name="x",
            type=scope["I32"],
            init=None
        )
        __builder__.add_field(
            name=str(
                InterpolatedString(
                    strings=["_", ""],
                    values=["y"]
                )
            ),
            type=lambda: scope["I32"],
            init=None
        )

        build(__builder__.get_specification())

    Derived = DefaultMetaClass(
        __build__Derived
    )

    @staticmethod
    def __build__Base(build):
        __builder__ = ClassBuilder()

        build(__builder__.get_specification())

    Base = printAndReturn(
        DefaultMetaClass(
            __build__Base
        )
    )

    BaseClass = Base
