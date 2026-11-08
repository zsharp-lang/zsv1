def Directory { // we assume Z# class is the default now
    let path: Path;

    new cwd() {} // construct a new directory, pointing at the working directory

    @operator((/)) // direct operator reference
    fun(other: ItemName): Item {
        return resolve(path / other);
    }
}
