namespace mrQuickProperties;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Title = ModPlusAPI.Language.GetPluginLocalName(ModPlusConnector.Instance);
    }

    private void DataGridCell_Selected(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is DataGridCell { IsReadOnly: false })
        {
            var grd = (DataGrid)sender;
            grd.BeginEdit(e);
        }
    }

    private void OnDataGridPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        if (e.Column is DataGridTemplateColumn)
        {
            var element = e.EditingElement;

            element.Dispatcher.BeginInvoke(
                DispatcherPriority.Input, new Action(() => element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First))));
        }
    }
}