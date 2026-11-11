using System.Collections;
using System.Reflection;
using System.Text;

namespace ZSharp.Diagnostics
{
    /// <summary>
    /// Prints any HIR graph as indented text, for looking at code at each level of
    /// reification. The common <c>HIR.Code</c> nodes print as code, so reified code
    /// reads as the calls it makes. Anything else prints its public properties if it is
    /// from an HIR assembly or package, and its value otherwise. An object that isn't
    /// code, such as a local, a reference or a definition, gets a number the first time
    /// it is printed, and later appearances refer back to it.
    /// </summary>
    public static class HIRPrinter
    {
        public static string Print(object? node)
        {
            var printer = new Printer();

            printer.Write(node, 0);

            return printer.ToString();
        }

        private sealed class Printer
        {
            private readonly StringBuilder text = new();
            private readonly Dictionary<object, int> numbers = new(ReferenceEqualityComparer.Instance);
            private readonly HashSet<object> visiting = new(ReferenceEqualityComparer.Instance);

            public override string ToString() => text.ToString();

            public void Write(object? value, int indent)
            {
                switch (value)
                {
                    case null:
                        text.Append("null");
                        return;
                    case string @string:
                        text.Append('"').Append(@string).Append('"');
                        return;
                    case bool or char or sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal or Enum:
                        text.Append(value);
                        return;
                    case Type type:
                        text.Append("typeof(").Append(type.Name).Append(')');
                        return;
                    case Delegate:
                        text.Append("<function>");
                        return;
                }

                if (!IsHIR(value.GetType()))
                {
                    text.Append('<').Append(value.GetType().Name).Append(' ').Append(value).Append('>');
                    return;
                }

                if (value is IEnumerable items)
                {
                    WriteList(items, indent);
                    return;
                }

                if (!WriteCode(value, indent))
                    WriteNode(value, indent);
            }

            /// <summary>
            /// The nodes reified code is made of, printed as the code they stand for.
            /// </summary>
            private bool WriteCode(object node, int indent)
            {
                switch (node)
                {
                    case HIR.Code.Call call:
                        Write(call.Callee, indent);
                        text.Append('(');
                        for (var i = 0; i < call.Arguments.Count; i++)
                        {
                            if (i > 0)
                                text.Append(", ");
                            if (call.Arguments[i].Name is { } name)
                                text.Append(name).Append(": ");
                            Write(call.Arguments[i].Value, indent);
                        }
                        text.Append(')');
                        return true;
                    case HIR.Code.NamedMember { Target: HIR.Code.Get { Instance: var target } } member when Short(target) is { } owner:
                        text.Append(owner).Append('.').Append(member.Name);
                        return true;
                    case HIR.Code.NamedMember member:
                        Write(member.Target, indent);
                        text.Append('.').Append(member.Name);
                        return true;
                    case HIR.Code.Get get:
                        text.Append("Get(");
                        Write(get.Instance, indent);
                        text.Append(')');
                        return true;
                    case HIR.Code.Set set:
                        Write(set.Target, indent);
                        text.Append(" = ");
                        Write(set.Value, indent);
                        return true;
                    case HIR.Code.StringLiteral literal:
                        text.Append('"').Append(literal.Value).Append('"');
                        return true;
                    case HIR.Code.BooleanLiteral literal:
                        text.Append(literal.Value ? "true" : "false");
                        return true;
                    case HIR.Code.ExpressionStatement statement:
                        Write(statement.Expression, indent);
                        text.Append(';');
                        return true;
                    case HIR.Code.Return { Value: null }:
                        text.Append("return;");
                        return true;
                    case HIR.Code.Return @return:
                        text.Append("return ");
                        Write(@return.Value, indent);
                        text.Append(';');
                        return true;
                    case HIR.Code.Block block:
                        WriteStatements(block.Statements, indent);
                        return true;
                    case HIR.Code.Local local when numbers.TryGetValue(local, out var known):
                        text.Append("local#").Append(known).Append(" \"").Append(local.Name).Append('"');
                        return true;
                    case HIR.Code.Local local:
                        numbers[local] = numbers.Count + 1;
                        text.Append("local#").Append(numbers[local]).Append(" \"").Append(local.Name).Append("\": ");
                        Write(local.Type, indent);
                        return true;
                    default:
                        return false;
                }
            }

            private void WriteStatements(IEnumerable<HIR.Code.Statement> statements, int indent)
            {
                text.Append('{');

                foreach (var statement in statements)
                {
                    text.AppendLine();
                    Indent(indent + 1);
                    Write(statement, indent + 1);
                }

                text.AppendLine();
                Indent(indent);
                text.Append('}');
            }

            /// <summary>
            /// A type that only stands for itself is printed by its name.
            /// </summary>
            private static string? Short(object instance)
                => instance.GetType().Name is "Class" or "Struct" or "Interface" && Named(instance) is { } name
                && instance.GetType().GetProperty("Fields")?.GetValue(instance) is ICollection { Count: 0 }
                && instance.GetType().GetProperty("Methods")?.GetValue(instance) is ICollection { Count: 0 }
                    ? name[(name.LastIndexOf('.') + 1)..]
                    : null;

            private void WriteNode(object node, int indent)
            {
                var type = node.GetType();
                var name = Named(node);

                if (!IsCode(type))
                {
                    if (numbers.TryGetValue(node, out var known))
                    {
                        text.Append('^').Append(type.Name).Append('#').Append(known);
                        if (name is not null)
                            text.Append(" \"").Append(name).Append('"');
                        return;
                    }

                    numbers[node] = numbers.Count + 1;
                }

                text.Append(type.Name);

                if (numbers.TryGetValue(node, out var number))
                    text.Append('#').Append(number);

                if (name is not null)
                    text.Append(" \"").Append(name).Append('"');

                if (!visiting.Add(node))
                {
                    text.Append(" ...");
                    return;
                }

                foreach (var property in Properties(type))
                {
                    if (property.Name == "Name" && name is not null)
                        continue;

                    var value = property.GetValue(node);

                    if (value is null || value is ICollection { Count: 0 })
                        continue;

                    text.AppendLine();
                    Indent(indent + 1);
                    text.Append(property.Name).Append(": ");
                    Write(value, indent + 1);
                }

                visiting.Remove(node);
            }

            private void WriteList(IEnumerable items, int indent)
            {
                var any = false;

                text.Append('[');

                foreach (var item in items)
                {
                    any = true;
                    text.AppendLine();
                    Indent(indent + 1);
                    Write(item, indent + 1);
                }

                if (any)
                {
                    text.AppendLine();
                    Indent(indent);
                }

                text.Append(']');
            }

            private void Indent(int indent) => text.Append(' ', indent * 2);

            private static IEnumerable<PropertyInfo> Properties(Type type)
                => type
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => property.CanRead && property.GetIndexParameters().Length == 0);

            private static string? Named(object node)
                => node.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)?.GetValue(node) as string;

            /// <summary>
            /// HIR lives in the HIR assemblies and in packages that add to it.
            /// </summary>
            private static bool IsHIR(Type type)
                => type.Namespace is { } @namespace
                && (@namespace.StartsWith("ZSharp.HIR") || @namespace.StartsWith("Package."))
                || type.IsGenericType && type.GetGenericArguments().Any(IsHIR);

            /// <summary>
            /// Code has structure, not identity, so it isn't numbered.
            /// </summary>
            private static bool IsCode(Type type)
                => typeof(HIR.Code.Expression).IsAssignableFrom(type)
                || typeof(HIR.Code.Statement).IsAssignableFrom(type)
                || typeof(HIR.Code.Argument).IsAssignableFrom(type)
                || typeof(HIR.Code.Code).IsAssignableFrom(type);
        }
    }
}
