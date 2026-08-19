# Decorators


The AST compiler produces an ORS graph. When a node is decorated,
the compiler should:
1) Compile the decorated value
2) Construct a `Call` ORS object with the decorator and pass the compiled decorated value as an injected value.
3) Evaluate the `Call` object.
4) Return the result of evaluation. If it is `Void`, we return the original compiled decorated value.

Decorators should always operate on ORS objects. That's why we
need to pass in the compiled value not as an expression, but as
a literal, injected value. This way the decorator gets access
to the actual object the compiler sees and can modify it or even
return a different object.
