using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PDF_MultiTool;

public partial class IgnoreFieldsWindow : Window
{
    private readonly ObservableCollection<string> _fields;
    private readonly IConfiguration _configuration;
    
    public IgnoreFieldsWindow(IConfiguration configuration, HashSet<string> currentFields)
    {
        InitializeComponent();
        _configuration = configuration;
        _fields = new ObservableCollection<string>(currentFields);
        FieldsListView.ItemsSource = _fields;
    }

    private void NewFieldTextBow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddNewField();
        }
    }

    private void AddField_Click(object sender, RoutedEventArgs e)
    {
        AddNewField();
    }

    private void AddNewField()
    {
        string newField = NewFieldTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(newField) && !_fields.Contains(newField))
        {
            _fields.Add(newField);
            SaveFields();
            NewFieldTextBox.Clear();
        }
    }

    private void RemoveField_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string fieldName)
        {
            _fields.Remove(fieldName);
            SaveFields();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void SaveFields()
    {
        string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        
        var jsonString = File.ReadAllText(configPath);
        var jsonDoc = JsonDocument.Parse(jsonString);
        var root = jsonDoc.RootElement;

        var newConfig = new Dictionary<string, object>
        {
            {
                "PdfSettings", new Dictionary<string, object>
                {
                    { "IgnoreFields", _fields.ToArray() }
                }
            }
        };
        
        // preserve other settings
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name != "PdfSettings")
            {
                newConfig[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText()) ?? throw new InvalidOperationException();
            }
        }
        
        // save all changes to file
        var options = new JsonSerializerOptions { WriteIndented = true };
        string newJson = JsonSerializer.Serialize(newConfig, options);
        File.WriteAllText(configPath, newJson);
    }
}