using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ZSharp.Compiler.ILLoader.Objects;

namespace ZSharp.Compiler.ILLoader
{
    internal static class InternalILMemberExtensions
    {
        public static string AliasOrName(this MemberInfo member)
            => member.GetCustomAttribute<AliasAttribute>() is AliasAttribute alias
                ? alias.Name
                : member.Name;

        public static bool IsModuleScope(this Type type)
            => type.GetCustomAttribute<ModuleScopeAttribute>() is not null;

        public static bool IsOperator(this MethodInfo method)
            => method.GetCustomAttribute<OperatorAttribute>() is not null;

        public static bool IsOperator(this MethodInfo method, [NotNullWhen(true)] out OperatorAttribute? @operator)
            => (@operator = method.GetCustomAttribute<OperatorAttribute>()) is not null;

        public static string GetNamespaceOverride(this MemberInfo member, string rootNamespace)
        {
            if (member.GetCustomAttribute<SetNamespaceAttribute>() is SetNamespaceAttribute setNamespace)
                return setNamespace.Name;

            if (member.GetCustomAttribute<AddNamespaceAttribute>() is AddNamespaceAttribute addNamespace)
            {
                if (addNamespace.IsGlobalNamespace)
                    return rootNamespace;
                return $"{rootNamespace}.{addNamespace.Name}";
            }

            return rootNamespace;
        }

        public static bool OverridesNamespace(this MemberInfo member, string rootNamespace, [NotNullWhen(true)] out string? @namespace)
            => (@namespace = member.GetNamespaceOverride(rootNamespace)) != rootNamespace;
    }
}
