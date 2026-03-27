let name: type = value;                             // compile-time type matching

let (v1: t1, v2: t2) = value;                       // compile-time tuple destructuring with type matching

let [v1, v2, ...v3, v4]: type = value;              // compile-time array destructuring with rest pattern and homogeneous element type matching

let {k1, k2, ...k3}: type = value;                  // compile-time map destructuring with rest pattern and homogeneous property type matching

let {k1: t1, k2: t2} = value;                        // compile-time object destructuring with property type matching


/*  ════════════════════════════════════════════════════════════════════════════
    SYNTAX NOTES
    ════════════════════════════════════════════════════════════════════════════

    Pattern Grammar:
    ─────────────────────────────────────────────────────────────────────────────
    ValuePattern    ::= NamePattern | TuplePattern | ArrayPattern | ObjectPattern
    TypedPattern    ::= TypedNamePattern | TypedTuplePattern | TypedArrayPattern 
                              | TypedObjectPattern

    NamePattern     ::= Identifier
    TypedNamePattern::= Identifier ':' Type

    TuplePattern    ::= '(' [ Pattern (',' Pattern)* [','] ] ')'
    ArrayPattern    ::= '[' [ Pattern (',' Pattern)* [',' '...'? Pattern ] ] ']'
    ObjectPattern   ::= '{' [ PropertyPattern (',' PropertyPattern)* [',' '...'? ] ] '}'


    Pattern Classification:
    ─────────────────────────────────────────────────────────────────────────────
    ┌──────────┬──────────────┬───────────────┐
    │ Name     │ Member Kind  │ Member Type   │
    ├──────────┼──────────────┼───────────────┤
    │ Tuple    │ Positional   │ Heterogeneous │
    │ Array    │ Positional   │ Homogeneous   │
    │ Object   │ Named        │ Heterogeneous │
    │ Map      │ Named        │ Homogeneous   │
    └──────────┴──────────────┴───────────────┘

    ════════════════════════════════════════════════════════════════════════════ */
