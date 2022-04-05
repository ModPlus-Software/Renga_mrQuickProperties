namespace mrQuickProperties.Models;

/// <summary>
/// Значение для перечисления
/// </summary>
public class EnumerationValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumerationValue"/> class.
    /// </summary>
    /// <param name="displayName">Display name</param>
    /// <param name="value">Value</param>
    public EnumerationValue(string displayName, object value)
    {
        DisplayName = displayName;
        Value = value;
    }

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; }
}