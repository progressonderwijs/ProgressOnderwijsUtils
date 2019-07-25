#nullable enable
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    static class BackingFieldDetector
    {
        const string BackingFieldPrefix = "<";
        const string BackingFieldSuffix = ">k__BackingField";
        const BindingFlags privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        const BindingFlags anyInstance = privateInstance | BindingFlags.Public;

        [NotNull]
        static string BackingFieldFromAutoPropName([NotNull] string propertyName)
            => BackingFieldPrefix + propertyName + BackingFieldSuffix;

        static string? AutoPropNameFromBackingField([NotNull] string fieldName)
            => fieldName.StartsWith(BackingFieldPrefix, StringComparison.Ordinal) && fieldName.EndsWith(BackingFieldSuffix, StringComparison.Ordinal)
                ? fieldName.Substring(BackingFieldPrefix.Length, fieldName.Length - BackingFieldPrefix.Length - BackingFieldSuffix.Length)
                : null;

        static bool IsCompilerGenerated([CanBeNull] MemberInfo member)
            => member != null && member.IsDefined(typeof(CompilerGeneratedAttribute), true);

        static bool IsAutoProp(PropertyInfo autoProperty)
            => IsCompilerGenerated(autoProperty.GetGetMethod(true));

        public static FieldInfo? BackingFieldOfPropertyOrNull([NotNull] PropertyInfo propertyInfo)
            => IsAutoProp(propertyInfo)
                && propertyInfo.DeclaringType.GetField(BackingFieldFromAutoPropName(propertyInfo.Name), privateInstance) is FieldInfo backingField
                && IsCompilerGenerated(backingField)
                    ? backingField
                    : null;

        [UsefulToKeep("for symmetry with BackingFieldOfPropertyOrNull")]
        public static PropertyInfo? AutoPropertyOfFieldOrNull(FieldInfo fieldInfo)
            => IsCompilerGenerated(fieldInfo)
                && AutoPropNameFromBackingField(fieldInfo.Name) is string autoPropertyName
                && fieldInfo.DeclaringType.GetProperty(autoPropertyName, anyInstance) is PropertyInfo autoProperty
                && IsAutoProp(autoProperty)
                    ? autoProperty
                    : null;
    }
}
