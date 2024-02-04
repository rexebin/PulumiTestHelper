using System.Reflection;

namespace PulumiTestHelper.Internals;

internal static class InternalExtensions
{
    internal static IEnumerable<string> GetResourceCustomAttributeTypes(this MemberInfo resourceType)
    {
        return resourceType.GetCustomAttributes().Select(x =>
            x.GetType().GetProperty("Type")?.GetValue(x, null) as string).Where(x => !string.IsNullOrWhiteSpace(x))!;
    }

    internal static bool IsCallMockType(this Type type, string? token)
    {
        return token?.ToLower().Contains(type.Name.ToLower()) ?? false;
    }
}