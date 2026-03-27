class UniqueIDMixin[T] of Mixins
{
    @primaryKey // No default because we create DB models from existing domain models
    let id: T;
}
