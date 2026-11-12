using System.Reflection;

namespace ZSharp.Importer.ILLoader.Objects
{
    /// <summary>
    /// A CLR event, optionally on an object. The CLR describes an event as its accessors,
    /// so it is them: <c>add</c> subscribes a handler and <c>remove</c> unsubscribes it,
    /// each a method that takes a delegate of the event's type. Raising it belongs to the
    /// type that declares it.
    /// </summary>
    internal sealed class Event(ILLoader loader, EventInfo @event, CompilerObject? receiver)
        : CompilerObject
        , ICTGetMember<MemberName>
    {
        public IResult Member(MemberName member)
            => (member == "add" ? @event.AddMethod : member == "remove" ? @event.RemoveMethod : null) is { IsPublic: true } accessor
                ? Result.Ok(new Method(loader, accessor, receiver))
                : Result.Error(new UnsupportedError($"Event {@event.Name} has only add and remove."));
    }
}
