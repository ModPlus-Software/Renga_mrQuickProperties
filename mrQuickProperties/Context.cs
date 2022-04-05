namespace mrQuickProperties;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Models;
using ModPlusAPI.Mvvm;
using ModPlusAPI.Windows;
using Renga;

/// <summary>
/// Main context
/// </summary>
public class Context : ObservableObject
{
    private readonly Application _rengaApplication;
    private readonly ISelection _selection;
    private readonly SelectionEventSource _selectionEventSource;
    private bool _isEnabled;
    private ObjectGroup _selectedObjectGroup;

    /// <summary>
    /// Initializes a new instance of the <see cref="Context"/> class.
    /// </summary>
    public Context()
    {
        ObjectGroups = new ObservableCollection<ObjectGroup> { new (Guid.Empty) };
        _selectedObjectGroup = ObjectGroups.First();
        Properties = new ObservableCollection<SummaryProperty>();
        _rengaApplication = new Application();
        _selection = _rengaApplication.Selection;
        _selectionEventSource = new SelectionEventSource(_rengaApplication.Selection);
        _selectionEventSource.ModelSelectionChanged += OnModelSelectionChanged;

        if (_rengaApplication.ActiveView is IModelView)
        {
            IsEnabled = true;
        }
    }

    /// <summary>
    /// Is plugin work enabled
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value)
                return;
            _isEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Groups
    /// </summary>
    public ObservableCollection<ObjectGroup> ObjectGroups { get; }

    /// <summary>
    /// Selected group
    /// </summary>
    public ObjectGroup SelectedObjectGroup
    {
        get => _selectedObjectGroup;
        set
        {
            if (_selectedObjectGroup == value)
                return;
            _selectedObjectGroup = value;
            OnPropertyChanged();
            if (value != null)
                GetCurrentProperties();
        }
    }

    /// <summary>
    /// Properties
    /// </summary>
    public ObservableCollection<SummaryProperty> Properties { get; }

    /// <summary>
    /// Selected elements count
    /// </summary>
    public int SelectedElementsCount { get; private set; }

    /// <summary>
    /// Собрать свойства элементов, которые были выбраны на момент запуска
    /// </summary>
    public void GetCurrentProperties()
    {
        try
        {
            ReadCurrentSelection();
        }
        catch (Exception exception)
        {
            exception.ShowInExceptionBox();
        }
    }

    /// <summary>
    /// Stop events
    /// </summary>
    public void StopEvents()
    {
        _selectionEventSource?.Dispose();
    }

    private void OnModelSelectionChanged(object sender, EventArgs args)
    {
        GetCurrentProperties();
    }

    private void ReadCurrentSelection()
    {
        foreach (var summaryProperty in Properties)
        {
            summaryProperty.PropertyChanged -= SummaryPropertyOnPropertyChanged;
        }

        Properties.Clear();
        if (_rengaApplication.ActiveView is not IModelView)
        {
            IsEnabled = false;
            return;
        }

        IsEnabled = true;
        var properties = new List<SummaryProperty>();
        var ids = (int[])_selection.GetSelectedObjects();

        if (ids.Any())
        {
            var project = _rengaApplication.Project;
            var modelObjectCollection = project.Model.GetObjects();
            foreach (var id in ids)
            {
                var modelObject = modelObjectCollection.GetById(id);

                if (SelectedObjectGroup.ObjectType != Guid.Empty)
                {
                    if (SelectedObjectGroup.ObjectType != modelObject.ObjectType)
                        continue;
                }
                else
                {
                    if (ObjectGroups.All(g => g.ObjectType != modelObject.ObjectType))
                        ObjectGroups.Add(new ObjectGroup(modelObject.ObjectType));
                }

                var propertyContainer = modelObject.GetProperties();

                var guidCollection = propertyContainer.GetIds();
                for (var j = 0; j < guidCollection.Count; j++)
                {
                    var property = propertyContainer.Get(guidCollection.Get(j));

                    var summaryProperty = properties.FirstOrDefault(p => p.Id == property.Id);
                    if (summaryProperty == null)
                    {
                        summaryProperty = new SummaryProperty(property, project);
                        properties.Add(summaryProperty);
                    }
                    else
                    {
                        summaryProperty.AddProperty(property);
                    }
                }
            }

            foreach (var summaryProperty in properties.OrderBy(p => p.Name))
            {
                summaryProperty.Calculate();
                summaryProperty.PropertyChanged += SummaryPropertyOnPropertyChanged;
                Properties.Add(summaryProperty);
            }
        }
        else
        {
            for (var i = ObjectGroups.Count - 1; i >= 1; i--)
            {
                ObjectGroups.RemoveAt(i);
            }

            _selectedObjectGroup = ObjectGroups.First();
            OnPropertyChanged(nameof(SelectedObjectGroup));
        }

        SelectedElementsCount = ids.Length;
        OnPropertyChanged(nameof(SelectedElementsCount));
    }

    private void SummaryPropertyOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SummaryProperty.DisplayValue) && sender is SummaryProperty changedProperty)
        {
            foreach (var summaryProperty in Properties)
            {
                if (summaryProperty == changedProperty)
                    continue;
                summaryProperty.PropertyChanged -= SummaryPropertyOnPropertyChanged;
                summaryProperty.Calculate();
                summaryProperty.PropertyChanged += SummaryPropertyOnPropertyChanged;
            }
        }
    }
}