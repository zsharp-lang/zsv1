// An item of the shop. Its price is in prices.zs, which it doesn't import: both are
// sources of the same module.
fun describe(name: String): String { return label(name, price(name)); }
