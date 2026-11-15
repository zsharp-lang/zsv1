# std:encoding

What a built module carries so it can be imported as what it was, and reflected on:
`encode(module)` writes its HIR as a blob, which a platform embeds in what it builds, and
`describe(value)` reads the node of what carries a definition's id.
