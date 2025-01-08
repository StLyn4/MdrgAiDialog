// This is a temporary solution for the new Unity engine version.
// These attributes are required to handle nullable reference types correctly.
// They will be removed once no longer needed.
// P.S. Maybe we should just use `<Nullable>disable</Nullable>` in the project file?

namespace System.Runtime.CompilerServices;

/// <summary>
/// Specifies nullability of elements in array or generic type
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
internal sealed class NullableAttribute : Attribute {
  /// <summary>
  /// Initializes attribute with single flag
  /// </summary>
  /// <param name="flags">Nullability flag</param>
  public NullableAttribute(byte flags) {
    nullableFlags = [flags];
  }

  /// <summary>
  /// Initializes attribute with array of flags
  /// </summary>
  /// <param name="flags">Array of nullability flags</param>
  public NullableAttribute(byte[] flags) {
    nullableFlags = flags;
  }

  /// <summary>
  /// Array of nullability flags
  /// </summary>
  public readonly byte[] nullableFlags;
}

/// <summary>
/// Specifies default nullability in the scope
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NullableContextAttribute(byte flags) : Attribute {
  public readonly byte flag = flags;
}
