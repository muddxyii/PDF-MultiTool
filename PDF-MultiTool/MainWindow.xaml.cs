﻿using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using iText.Forms;
using iText.Kernel.Pdf;
using Microsoft.Win32;
using Microsoft.Extensions.Configuration;

namespace PDF_MultiTool;

public partial class MainWindow
{
    private string? _selectedPdfPath;

    private readonly IConfiguration _configuration;
    private string? _selectedPartialPdfPath;
    private HashSet<String> _ignoreFields = new HashSet<String>(StringComparer.Ordinal);
    
    private string? _oldPdfPath;
    private string? _newPdfPath;
    
    public MainWindow()
    {
        InitializeComponent();

        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        OperationComboBox.SelectedIndex = 0;
        UpdatePanelVisibility();
        
        LoadIgnoreFields();
    }

    private void OperationComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePanelVisibility();
    }
    
    private void UpdatePanelVisibility()
    {
        var selectedItem = (OperationComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        ClearPdfPanel.Visibility = selectedItem == "Clear PDF" ? Visibility.Visible : Visibility.Collapsed;
        PartialClearPdfPanel.Visibility = selectedItem == "Partial Clear PDF" ? Visibility.Visible : Visibility.Collapsed;
        ConvertPdfPanel.Visibility = selectedItem == "Convert PDF Details" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SelectPDF_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _selectedPdfPath = openFileDialog.FileName;
            SelectedPdfPath.Text = $"Selected: {_selectedPdfPath}";
        }
    }

    private void ClearPDF_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedPdfPath))
        {
            MessageBox.Show("Please select a PDF file first.");
            return;
        }

        try
        {
            string outputPath = Path.Combine(
                Path.GetDirectoryName(_selectedPdfPath) ?? throw new InvalidOperationException(),
                Path.GetFileNameWithoutExtension(_selectedPdfPath) + "_cleared.pdf");

            using (var reader = new PdfReader(_selectedPdfPath))
            using (var writer = new PdfWriter(outputPath))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                var form = PdfAcroForm.GetAcroForm(pdfDoc, false);

                if (form != null && form.GetFormFields().Count > 0)
                {
                    int fieldsCleared = 0;
                    
                    foreach (var field in form.GetFormFields())
                    {
                        var fieldType = field.Value.GetFormType();
                        switch (fieldType.ToString())
                        {
                            case "/Tx":
                                field.Value.SetValue("");
                                fieldsCleared++;
                                break;
                            case "/Ch":
                                field.Value.SetValue("");
                                fieldsCleared++;
                                break;
                            case "/Btn":
                                field.Value.SetValue("Off");
                                fieldsCleared++;
                                break;
                        }
                    }
                    
                    MessageBox.Show($"Successfully cleared {fieldsCleared} fields.\n\n" +
                                    $"New version saved to:\n{outputPath}", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No fillable fields found in the PDF.", "Information", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error clearing PDF fields: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void LoadIgnoreFields()
    {
        _ignoreFields = new HashSet<string>(StringComparer.Ordinal);
        
        // Load from settings
        var ignoreFields = _configuration.GetSection("PdfSettings:IgnoreFields")
            .GetChildren()
            .Select(c => c.Value)
            .ToArray();
        
        if (ignoreFields.Any())
        {
            foreach (var field in ignoreFields)
            {
                _ignoreFields.Add(field ?? throw new InvalidOperationException());
            }
        }
    }
    
    private void SaveIgnoreFields()
    {
        string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        
        // Read existing config
        var jsonString = File.ReadAllText(configPath);
        var jsonDoc = JsonDocument.Parse(jsonString);
        var root = jsonDoc.RootElement;
        
        // Create new config object
        var newConfig = new Dictionary<string, object>
        {
            {
                "PdfSettings", new Dictionary<string, object>
                {
                    { "IgnoreFields", _ignoreFields.ToArray() }
                }
            }
        };

        // Preserve other settings if they exist
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name != "PdfSettings")
            {
                newConfig[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText()) ?? throw new InvalidOperationException();
            }
        }

        // Save back to file
        var options = new JsonSerializerOptions { WriteIndented = true };
        string newJson = JsonSerializer.Serialize(newConfig, options);
        File.WriteAllText(configPath, newJson);
    }
    
    public void AddIgnoreField(String fieldName)
    {
        _ignoreFields.Add(fieldName);
    }

    public void RemoveIgnoreField(String fieldName)
    {
        _ignoreFields.Remove(fieldName);
    }

    public void ClearIgnoreFields()
    {
        _ignoreFields.Clear();
    }
    
    
    private void SelectPartialPDF_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _selectedPartialPdfPath = openFileDialog.FileName;
            SelectedPartialPdfPath.Text = $"Selected: {_selectedPartialPdfPath}";
        }
    }
    
    private void PartialClearPDF_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedPartialPdfPath))
        {
            MessageBox.Show("Please select a PDF file first.");
            return;
        }
        
        try
        {
            string outputPath = Path.Combine(
                Path.GetDirectoryName(_selectedPartialPdfPath) ?? throw new InvalidOperationException(),
                Path.GetFileNameWithoutExtension(_selectedPartialPdfPath) + "_cleared.pdf");

            using (var reader = new PdfReader(_selectedPartialPdfPath))
            using (var writer = new PdfWriter(outputPath))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                var form = PdfAcroForm.GetAcroForm(pdfDoc, false);

                if (form != null && form.GetFormFields().Count > 0)
                {
                    int fieldsCleared = 0;
                    
                    foreach (var field in form.GetFormFields())
                    {
                        if (!_ignoreFields.Contains(field.Value.GetFieldName().ToString()))
                        {
                            var fieldType = field.Value.GetFormType();
                            switch (fieldType.ToString())
                            {
                                case "/Tx":
                                    field.Value.SetValue("");
                                    fieldsCleared++;
                                    break;
                                case "/Ch":
                                    field.Value.SetValue("");
                                    fieldsCleared++;
                                    break;
                                case "/Btn":
                                    field.Value.SetValue("Off");
                                    fieldsCleared++;
                                    break;
                            }
                        }
                    }
                    
                    MessageBox.Show($"Successfully cleared {fieldsCleared} fields.\n\n" +
                                    $"New version saved to:\n{outputPath}", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No fillable fields found in the PDF.", "Information", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error clearing PDF fields: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SelectOldPDF_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _oldPdfPath = openFileDialog.FileName;
            OldPdfPath.Text = $"Selected: {_oldPdfPath}";
        }
    }

    private void SelectNewPDF_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            _newPdfPath = openFileDialog.FileName;
            NewPdfPath.Text = $"Selected: {_newPdfPath}";
        }
    }

    private void ConvertPDF_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_oldPdfPath) || string.IsNullOrEmpty(_newPdfPath))
        {
            MessageBox.Show("Please select both PDF files first.");
            return;
        }

        try
        {
            // create new output path, to save the old pdf
            string outputPath = Path.Combine(
                Path.GetDirectoryName(_newPdfPath) ?? throw new InvalidOperationException(),
                Path.GetFileNameWithoutExtension(_newPdfPath) + "_converted.pdf");
            
            // get old pdf values
            Dictionary<string, string> oldValues = new Dictionary<string, string>();
            using (var reader = new PdfReader(_oldPdfPath))
            using (var oldDoc = new PdfDocument(reader))
            {
                var oldForm = PdfAcroForm.GetAcroForm(oldDoc, false);
                if (oldForm != null)
                {
                    foreach (var field in oldForm.GetFormFields())
                    {
                        oldValues[field.Key] = field.Value.GetValueAsString();
                    }
                }
            }
            
            // write old pdf values to new pdf
            using (var reader = new PdfReader(_newPdfPath))
            using (var writer = new PdfWriter(outputPath))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                var newForm = PdfAcroForm.GetAcroForm(pdfDoc, false);
                int fieldsConverted = 0;
                
                if (newForm != null && newForm.GetFormFields().Count > 0)
                {
                    foreach (var field in newForm.GetFormFields())
                    {
                        if (oldValues.ContainsKey(field.Key) && !string.IsNullOrEmpty(oldValues[field.Key]))
                        {
                            field.Value.SetValue(oldValues[field.Key]);
                            fieldsConverted++;
                        }
                    }
                    MessageBox.Show($"Successfully converted {fieldsConverted} fields.\n\n" +
                                    $"New version saved to:\n{outputPath}", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No fillable fields found in the PDF.", "Information", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error clearing PDF fields: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}