namespace mrQuickProperties;

using System;
using ModPlus;
using ModPlusAPI.Windows;

/// <inheritdoc/>
public class PluginStarter : IRengaFunction
{
    private static MainWindow _mainWindow;
    private static Renga.ApplicationEventSource _applicationEventSource;

    /// <inheritdoc/>
    public void Start()
    {
#if !DEBUG
        ModPlusAPI.Statistic.SendCommandStarting(ModPlusConnector.Instance);
#endif
        try
        {
            if (_applicationEventSource == null)
            {
                _applicationEventSource = new Renga.ApplicationEventSource(new Renga.Application());
                _applicationEventSource.ProjectClose += ApplicationEventSourceOnProjectClose;
            }

            if (_mainWindow != null)
            {
                _mainWindow.Activate();
            }
            else
            {
                var context = new Context();
                _mainWindow = new MainWindow { DataContext = context };
                _mainWindow.ContentRendered += (_, _) => context.GetCurrentProperties();
                _mainWindow.Closed += (_, _) =>
                {
                    context.StopEvents();
                    _mainWindow = null;
                };
                ModPlus.ShowModeless(_mainWindow);
            }
        }
        catch (Exception exception)
        {
            exception.ShowInExceptionBox();
        }
    }

    private void ApplicationEventSourceOnProjectClose(object sender, Renga.ProjectCloseEventArgs e)
    {
        _mainWindow?.Close();
        _mainWindow = null;
    }
}