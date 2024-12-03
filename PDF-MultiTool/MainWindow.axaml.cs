using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using iText.Forms;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Configuration;
using MsBox.Avalonia;

namespace PDF_MultiTool;

public partial class MainWindow : Window
{
    private string? _selectedPdfPath;
    private string? _selectedPartialPdfPath;
    private string? _oldPdfPath;
    private string? _newPdfPath;
    
    public MainWindow()
    {
        InitializeComponent();
        
        if (this.FindControl<ComboBox>("OperationComboBox") is ComboBox operationComboBox)
        {
            operationComboBox.SelectedIndex = 0;
        }
        UpdatePanelVisibility();
    }

    private void OperationComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdatePanelVisibility();
    }
    
    private void UpdatePanelVisibility()
    {
        var operationComboBox = this.FindControl<ComboBox>("OperationComboBox");
        var selectedItem = (operationComboBox?.SelectedItem as ComboBoxItem)?.Content.ToString();

        if (this.FindControl<StackPanel>("ClearPdfPanel") is StackPanel clearPanel)
            clearPanel.IsVisible = selectedItem == "Clear PDF";
            
        if (this.FindControl<StackPanel>("PartialClearPdfPanel") is StackPanel partialPanel)
            partialPanel.IsVisible = selectedItem == "Partial Clear PDF";
            
        if (this.FindControl<StackPanel>("ConvertPdfPanel") is StackPanel convertPanel)
            convertPanel.IsVisible = selectedItem == "Convert PDF Details";
    }

    private async void SelectPDF_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "PDF files", Extensions = { "pdf" } },
                new FileDialogFilter { Name = "All files", Extensions = { "*" } }
            }
        };

        var result = await dialog.ShowAsync(this);
        if (result?.Length > 0)
        {
            _selectedPdfPath = result[0];
            if (this.FindControl<TextBlock>("SelectedPdfPath") is TextBlock textBlock)
            {
                textBlock.Text = $"Selected: {_selectedPdfPath}";
            }
        }
    }

    private async void ClearPDF_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedPdfPath))
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Warning", "You need to select a PDF file to clear.");
            await box.ShowAsync();
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

                if (form != null && form.GetAllFormFields().Count > 0)
                {
                    int fieldsCleared = 0;
                    
                    foreach (var field in form.GetAllFormFields())
                    {
                        var fieldType = field.Value.GetFormType();
                        switch (fieldType.ToString())
                        {
                            case "/Tx":
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
                    
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Success", $"Successfully cleared {fieldsCleared} fields.\n\nNew version saved to:\n{outputPath}");
                    await box.ShowAsync();
                }
                else
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Information", "No fillable fields found in the PDF.");
                    await box.ShowAsync();
                }
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error", $"Error clearing PDF fields: {ex.Message}");
            await box.ShowAsync();
        }
    }

    private async void SelectOldPDF_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "PDF files", Extensions = { "pdf" } },
                new FileDialogFilter { Name = "All files", Extensions = { "*" } }
            }
        };

        var result = await dialog.ShowAsync(this);
        if (result?.Length > 0)
        {
            _oldPdfPath = result[0];
            if (this.FindControl<TextBlock>("OldPdfPath") is TextBlock textBlock)
            {
                textBlock.Text = $"Selected: {_oldPdfPath}";
            }
        }
    }

    private async void SelectNewPDF_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "PDF files", Extensions = { "pdf" } },
                new FileDialogFilter { Name = "All files", Extensions = { "*" } }
            }
        };

        var result = await dialog.ShowAsync(this);
        if (result?.Length > 0)
        {
            _newPdfPath = result[0];
            if (this.FindControl<TextBlock>("NewPdfPath") is TextBlock textBlock)
            {
                textBlock.Text = $"Selected: {_newPdfPath}";
            }
        }
    }

    private async void ConvertPDF_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_oldPdfPath) || string.IsNullOrEmpty(_newPdfPath))
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Warning", "Please select both PDF files first.");
            await box.ShowAsync();
            return;
        }

        try
        {
            string outputPath = Path.Combine(
                Path.GetDirectoryName(_newPdfPath) ?? throw new InvalidOperationException(),
                Path.GetFileNameWithoutExtension(_newPdfPath) + "_converted.pdf");
            
            Dictionary<string, string> oldValues = new Dictionary<string, string>();
            using (var reader = new PdfReader(_oldPdfPath))
            using (var oldDoc = new PdfDocument(reader))
            {
                var oldForm = PdfAcroForm.GetAcroForm(oldDoc, false);
                if (oldForm != null)
                {
                    foreach (var field in oldForm.GetAllFormFields())
                    {
                        oldValues[field.Key] = field.Value.GetValueAsString();
                    }
                }
            }
            
            using (var reader = new PdfReader(_newPdfPath))
            using (var writer = new PdfWriter(outputPath))
            using (var pdfDoc = new PdfDocument(reader, writer))
            {
                var newForm = PdfAcroForm.GetAcroForm(pdfDoc, false);
                int fieldsConverted = 0;
                
                if (newForm != null && newForm.GetAllFormFields().Count > 0)
                {
                    foreach (var field in newForm.GetAllFormFields())
                    {
                        if (oldValues.ContainsKey(field.Key) && !string.IsNullOrEmpty(oldValues[field.Key]))
                        {
                            field.Value.SetValue(oldValues[field.Key]);
                            fieldsConverted++;
                        }
                    }
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Success", $"Successfully converted {fieldsConverted} fields.\n\nNew version saved to:\n{outputPath}");
                    await box.ShowAsync();
                }
                else
                {
                    var box = MessageBoxManager
                        .GetMessageBoxStandard("Information", "No fillable fields found in the PDF.");
                    await box.ShowAsync();
                }
            }
        }
        catch (Exception ex)
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error", $"Error clearing PDF fields: {ex.Message}");
            await box.ShowAsync();
        }
    }
}