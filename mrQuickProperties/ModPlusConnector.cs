namespace mrQuickProperties;

using System.Collections.Generic;
using ModPlusAPI.Abstractions;
using ModPlusAPI.Enums;

/// <inheritdoc/>
public class ModPlusConnector : IModPlusPluginForRenga
{
    private static ModPlusConnector _instance;

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static ModPlusConnector Instance => _instance ??= new ModPlusConnector();

    /// <inheritdoc/>
    public SupportedProduct SupportedProduct => SupportedProduct.Renga;

    /// <inheritdoc/>
    public string Name => nameof(mrQuickProperties);

    /// <inheritdoc/>
    public string Price => "0";

    /// <inheritdoc/>
    public RengaFunctionUILocation UiLocation => RengaFunctionUILocation.PrimaryPanel;

    /// <inheritdoc/>
    public RengaContextMenuShowCase ContextMenuShowCase => RengaContextMenuShowCase.Scene;

    /// <inheritdoc/>
    public List<RengaViewType> ViewTypes => new ();

    /// <inheritdoc/>
    public bool IsAddingToMenuBySelf => false;
}