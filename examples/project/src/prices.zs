let discount = 2;

fun price(name: String): Int32 {
    if (name == "apple") return 5 - discount;
    return 10 - discount;
}
