# Concept - ORS Importer

The ORS system is an abstract representation system. As we saw, it
defines a small set of markers that together can be composed via
concrete types to form an abstract grapg.

Since the system itself doesn't care about the exact types, we can
and even encouraged to define our own types fitting in in relevant
slots. In fact, the entire language is built on this architecure.

So, an ORS importer is a thing that can create an ORS graph.
How, why, when and where, we don't really care. This is the design
behind the script runner.
