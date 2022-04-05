namespace mrQuickProperties.Models;

using System;
using ModPlusAPI;

/// <summary>
/// Object group
/// </summary>
public class ObjectGroup
{
    public ObjectGroup(Guid objectType)
    {
        ObjectType = objectType;
        Name = objectType == Guid.Empty ? Language.GetCommonItem("all") : GetDisplayNameByObjectType(objectType);
    }

    /// <summary>
    /// Id
    /// </summary>
    public Guid ObjectType { get; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; }

    private string GetDisplayNameByObjectType(Guid objectType)
    {
        return ModPlus.Helpers.Localization.RengaObjectType(objectType);
    }
}