namespace ZSharp.Compiler
{
    public enum AssignmentType
    {
        Incompatible = 0,
        Unspecified = 1,
        Exact = 2,
        Polymorphic = 3,
        ImplicitCast = 4,
        ExplicitCast = 5,
        Any = -1,
    }

    public abstract class Assignment
    {
        public abstract AssignmentType AssignmentType { get; }

        public static Assignment Incompatible { get; } = new Incompatible();

        public static Assignment Unspecified { get; } = new Unspecified();

        public static Assignment Exact { get; } = new Polymorphic(0);

        public static Assignment Any { get; } = new Polymorphic(-1);

        public static Polymorphic Polymorphic(int distance)
            => new(distance);

        public static ImplicitCast ImplicitCast(CTObject castFunction)
            => new(castFunction);

        public static ExplicitCast ExplicitCast(CTObject castFunction)
            => new(castFunction);
    }

    public sealed class Incompatible : Assignment
    {
        public override AssignmentType AssignmentType => AssignmentType.Incompatible;

        internal Incompatible() { }
    }

    public sealed class Unspecified : Assignment
    {
        public override AssignmentType AssignmentType => AssignmentType.Unspecified;

        internal Unspecified() { }
    }

    public sealed class Polymorphic(int distance) : Assignment
    {
        public override AssignmentType AssignmentType
            => Distance == 0 ? AssignmentType.Exact : AssignmentType.Polymorphic;

        public int Distance { get; } = distance;
    }

    public abstract class Cast(CTObject castFunction, AssignmentType assignmentType) : Assignment
    {
        public override AssignmentType AssignmentType => assignmentType;

        public CTObject CastFunction { get; } = castFunction;
    }

    public sealed class ImplicitCast(CTObject castFunction) 
        : Cast(castFunction, AssignmentType.ImplicitCast);

    public sealed class ExplicitCast(CTObject castFunction) 
        : Cast(castFunction, AssignmentType.ExplicitCast);
}
