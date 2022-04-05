namespace mrQuickProperties.Models;

/// <summary>
/// Control type for <see cref="SummaryProperty.DisplayValue"/>
/// </summary>
public enum DisplayValueControlType
{
    /// <summary>
    /// String via TextBox
    /// </summary>
    String = 0,

    /// <summary>
    /// Integer via NumericBox
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Double via NumericBox
    /// </summary>
    Double = 2,

    /// <summary>
    /// Values via ListBox
    /// </summary>
    List = 3
}