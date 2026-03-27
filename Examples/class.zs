class Point2D
{
    let x: F32,
        y: F32;

    fun distance(this, other: Point2D): F32 {
        let dx = this.x - other.x;
        let dy = this.y - other.y;
        return sqrt(dx * dx + dy * dy);
    }
}


fun createTransformer[T](coordinates: List[T -> F32], transform: F32 -> F32) {
    return fun(value: T) {
        return T(
            ...coordinates.map(value >> transform)
        );
    };
}

fun identity[T](value: T): T {
    return value;
}

let copyPoint2D = createTransformer([Point2D.x, Point2D.y], identity);

let X: Field[type: F32, owner: Point2D] = Point2D.x;

fun getX(point: Point2D): F32 {
    return X(point);
}

let getX: Point2D -> F32 = X;


class Field[type?, owner?] {
    get name(this): String;
    get type(this): Type;

    get owner(this): Type;

    if (type and owner) {
        @override(.Call)
        fun(this, object: owner): type {
            return this.value;
        }
    }
}
