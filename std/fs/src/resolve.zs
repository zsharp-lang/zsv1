fun resolve(path: Path): Item {
    if (not path.exists()) return Item.Path(path);

    if (path.isDirectory()) {
        return Item.Directory(Directory(path));
    } else if (path.isFile()) {
        return Item.File(File(path));
    } else {
        // Handle other
    }
}