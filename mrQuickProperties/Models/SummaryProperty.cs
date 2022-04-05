namespace mrQuickProperties.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ModPlusAPI;
using ModPlusAPI.Mvvm;
using Renga;

/// <summary>
/// Суммарное свойство
/// </summary>
public class SummaryProperty : ObservableObject
{
    private const string DoubleFormat = "N2";
    private readonly CultureInfo _numbersCulture;
    private readonly IProperty _initProperty;
    private readonly IProject _project;
    private readonly List<IProperty> _properties;
    private string _displayValue;
    private int _integerValue;
    private double _doubleValue;
    private EnumerationValue _selectedEnumerationValue;
    private readonly IOperation _operation;

    public SummaryProperty(IProperty initProperty, IProject project)
    {
        _initProperty = initProperty;
        _properties = new List<IProperty> { initProperty };
        _project = project;
        _operation = project.Model.CreateOperation();
        Name = initProperty.Name;
        Id = initProperty.Id;
        PropertyType = initProperty.Type;
        _numbersCulture = CultureInfo.CurrentCulture;
        EnumerationValues = new ObservableCollection<EnumerationValue>();

        if (PropertyType == PropertyType.PropertyType_String)
            DisplayValueControlType = DisplayValueControlType.String;
        else if (PropertyType == PropertyType.PropertyType_Integer)
            DisplayValueControlType = DisplayValueControlType.Integer;
        else if (IsDoubleValuePropertyType())
            DisplayValueControlType = DisplayValueControlType.Double;
        else if (IsEnumerationValuePropertyType())
            DisplayValueControlType = DisplayValueControlType.List;
        
        if (PropertyType == PropertyType.PropertyType_Boolean)
        {
            EnumerationValues.Add(new EnumerationValue(Language.GetCommonItem("yes"), true));
            EnumerationValues.Add(new EnumerationValue(Language.GetCommonItem("no"), false));
        }
        else if (PropertyType == PropertyType.PropertyType_Logical)
        {
            EnumerationValues.Add(new EnumerationValue(Language.GetCommonItem("yes"), Logical.Logical_True));
            EnumerationValues.Add(new EnumerationValue(Language.GetCommonItem("no"), Logical.Logical_False));
            EnumerationValues.Add(new EnumerationValue(Language.GetItem("unknown"), Logical.Logical_Indeterminate));
        }
        else if (PropertyType == PropertyType.PropertyType_Length)
        {
            Unit = Language.GetCommonItem("mm");
        }
        else if (PropertyType == PropertyType.PropertyType_Area)
        {
            Unit = Language.GetCommonItem("m2");
        }
        else if (PropertyType == PropertyType.PropertyType_Volume)
        {
            Unit = Language.GetCommonItem("m3");
        }
        else if (PropertyType == PropertyType.PropertyType_Mass)
        {
            Unit = Language.GetCommonItem("kg");
        }
        else if (PropertyType == PropertyType.PropertyType_Angle)
        {
            Unit = Language.GetCommonItem("degree");
        }
    }

    /// <summary>
    /// Property id
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Единицы измерения
    /// </summary>
    public string Unit { get; }

    /// <summary>
    /// Видимость единиц измерения
    /// </summary>
    public bool IsVisibleUnit => !string.IsNullOrEmpty(Unit);

    /// <summary>
    /// Type
    /// </summary>
    public PropertyType PropertyType { get; }

    /// <summary>
    /// Display value control type
    /// </summary>
    public DisplayValueControlType DisplayValueControlType { get; }

    /// <summary>
    /// Elements count
    /// </summary>
    public int ElementsCount { get; private set; }

    /// <summary>
    /// Display value
    /// </summary>
    public string DisplayValue
    {
        get => _displayValue;
        set
        {
            if (_displayValue == value)
                return;
            _displayValue = value;
            OnPropertyChanged();
            SetValueToProperties();
        }
    }

    /// <summary>
    /// Integer value
    /// </summary>
    public int IntegerValue
    {
        get => _integerValue;
        set
        {
            if (_integerValue == value)
                return;
            _integerValue = value;
            OnPropertyChanged();
            DisplayValue = value.ToString();
        }
    }

    /// <summary>
    /// Double value
    /// </summary>
    public double DoubleValue
    {
        get => _doubleValue;
        set
        {
            if (Math.Abs(_doubleValue - value) < 0.001)
                return;
            _doubleValue = value;
            OnPropertyChanged();
            DisplayValue = value.ToString(DoubleFormat, _numbersCulture);
        }
    }

    /// <summary>
    /// Enumeration values
    /// </summary>
    public ObservableCollection<EnumerationValue> EnumerationValues { get; }

    /// <summary>
    /// Selected enumeration value
    /// </summary>
    public EnumerationValue SelectedEnumerationValue
    {
        get => _selectedEnumerationValue;
        set
        {
            if (_selectedEnumerationValue == value)
                return;
            _selectedEnumerationValue = value;
            OnPropertyChanged();
            DisplayValue = value?.DisplayName ?? string.Empty;
        }
    }
    
    /// <summary>
    /// Add property
    /// </summary>
    /// <param name="property">Property</param>
    public void AddProperty(IProperty property)
    {
        _properties.Add(property);
    }

    /// <summary>
    /// Calculate summary value
    /// </summary>
    public void Calculate()
    {
        var different = Language.GetItem("dif");
        if (PropertyType == PropertyType.PropertyType_String)
        {
            var values = _properties.Select(p => p.GetStringValue()).ToList();
            _displayValue = values.All(string.IsNullOrEmpty)
                ? string.Empty
                : values.Distinct().Count() != 1 ? different : values.First();
        }
        else if (PropertyType == PropertyType.PropertyType_Integer)
        {
            var values = _properties.Select(p => p.GetIntegerValue()).ToList();
            var distinctCount = values.Distinct().Count();
            _displayValue = distinctCount != 1 
                ? different 
                : values.First() is 0 && !_properties.First().HasValue() ? string.Empty : values.First().ToString();
            _integerValue = distinctCount != 1 ? 0 : values.First();
        }
        else if (IsDoubleValuePropertyType())
        {
            var values = GetDoubleValues();
            var distinctCount = values.Distinct().Count();
            _displayValue = distinctCount != 1 
                ? different
                : values.First() is 0.00 && !_properties.First().HasValue() ? string.Empty : values.First().ToString(DoubleFormat, _numbersCulture);
            _doubleValue = distinctCount != 1 ? 0 : values.First();
        }
        else if (PropertyType == PropertyType.PropertyType_Boolean)
        {
            var values = _properties.Select(p => p.GetBooleanValue()).ToList();
            var distinctCount = values.Distinct().Count();
            if (distinctCount == 1)
            {
                var first = values.First();
                _selectedEnumerationValue = EnumerationValues.First(v => (bool)v.Value == first);
                _displayValue = _selectedEnumerationValue.DisplayName;
            }
            else
            {
                _displayValue = different;
            }
        }
        else if (PropertyType == PropertyType.PropertyType_Logical)
        {
            var values = _properties.Select(p => p.GetLogicalValue()).ToList();
            var distinctCount = values.Distinct().Count();
            if (distinctCount == 1)
            {
                var first = values.First();
                _selectedEnumerationValue = EnumerationValues.First(v => (Logical)v.Value == first);
                _displayValue = _selectedEnumerationValue.DisplayName;
            }
            else
            {
                _displayValue = different;
            }
        }
        else if (PropertyType == PropertyType.PropertyType_Enumeration)
        {
            foreach (string item in _project.PropertyManager.GetPropertyDescription2(_initProperty.Id).GetEnumerationItems())
            {
                EnumerationValues.Add(new EnumerationValue(item, item));
            }

            var values = _properties.Select(p => p.GetEnumerationValue()).ToList();
            var distinctCount = values.Distinct().Count();
            if (distinctCount == 1)
            {
                var first = values.First();
                _selectedEnumerationValue = EnumerationValues.First(v => (string)v.Value == first);
                _displayValue = _selectedEnumerationValue.DisplayName;
            }
            else
            {
                _displayValue = different;
            }
        }

        OnPropertyChanged(nameof(DisplayValue));
        OnPropertyChanged(nameof(IntegerValue));
        OnPropertyChanged(nameof(DoubleValue));
        OnPropertyChanged(nameof(SelectedEnumerationValue));
        ElementsCount = _properties.Count;
    }

    private void SetValueToProperties()
    {
        _operation.Start();
        
        if (PropertyType == PropertyType.PropertyType_String)
        {
            _properties.ForEach(p => p.SetStringValue(DisplayValue));
        }
        else if (PropertyType == PropertyType.PropertyType_Integer)
        {
            _properties.ForEach(p => p.SetIntegerValue(IntegerValue));
        }
        else if (PropertyType == PropertyType.PropertyType_Double)
        {
            _properties.ForEach(p => p.SetDoubleValue(DoubleValue));
        }
        else if (PropertyType == PropertyType.PropertyType_Mass)
        {
            _properties.ForEach(p => p.SetMassValue(DoubleValue, MassUnit.MassUnit_Kilograms));
        }
        else if (PropertyType == PropertyType.PropertyType_Volume)
        {
            _properties.ForEach(p => p.SetVolumeValue(DoubleValue, VolumeUnit.VolumeUnit_Meters3));
        }
        else if (PropertyType == PropertyType.PropertyType_Area)
        {
            _properties.ForEach(p => p.SetAreaValue(DoubleValue, AreaUnit.AreaUnit_Meters2));
        }
        else if (PropertyType == PropertyType.PropertyType_Angle)
        {
            _properties.ForEach(p => p.SetAngleValue(DoubleValue, AngleUnit.AngleUnit_Degrees));
        }
        else if (PropertyType == PropertyType.PropertyType_Length)
        {
            _properties.ForEach(p => p.SetLengthValue(DoubleValue, LengthUnit.LengthUnit_Meters));
        }
        else if (PropertyType == PropertyType.PropertyType_Boolean)
        {
            _properties.ForEach(p => p.SetBooleanValue((bool)SelectedEnumerationValue.Value));
        }
        else if (PropertyType == PropertyType.PropertyType_Logical)
        {
            _properties.ForEach(p => p.SetLogicalValue((Logical)SelectedEnumerationValue.Value));
        }
        else if (PropertyType == PropertyType.PropertyType_Enumeration)
        {
            _properties.ForEach(p => p.SetEnumerationValue((string)SelectedEnumerationValue.Value));
        }

        _operation.Apply();
    }

    private bool IsDoubleValuePropertyType()
    {
        return PropertyType is 
            PropertyType.PropertyType_Double or
            PropertyType.PropertyType_Angle or
            PropertyType.PropertyType_Area or 
            PropertyType.PropertyType_Length or
            PropertyType.PropertyType_Mass or 
            PropertyType.PropertyType_Volume;
    }

    private bool IsEnumerationValuePropertyType()
    {
        return PropertyType is
            PropertyType.PropertyType_Enumeration or
            PropertyType.PropertyType_Boolean or
            PropertyType.PropertyType_Logical;
    }

    private List<double> GetDoubleValues()
    {
        return PropertyType switch
        {
            PropertyType.PropertyType_Double => _properties.Select(p => p.GetDoubleValue()).ToList(),
            PropertyType.PropertyType_Angle => _properties.Select(p => p.GetAngleValue(AngleUnit.AngleUnit_Degrees)).ToList(),
            PropertyType.PropertyType_Area => _properties.Select(p => p.GetAreaValue(AreaUnit.AreaUnit_Meters2)).ToList(),
            PropertyType.PropertyType_Length => _properties.Select(p => p.GetLengthValue(LengthUnit.LengthUnit_Meters)).ToList(),
            PropertyType.PropertyType_Mass => _properties.Select(p => p.GetMassValue(MassUnit.MassUnit_Kilograms)).ToList(),
            PropertyType.PropertyType_Volume => _properties.Select(p => p.GetVolumeValue(VolumeUnit.VolumeUnit_Meters3)).ToList(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}