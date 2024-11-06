﻿using System.IO;
using System.Windows;
using System.Windows.Controls;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using Microsoft.Win32;

namespace PDF_MultiTool;

public partial class MainWindow : Window
{
    private string _selectedPdfPath;
    private string _oldPdfPath;
    private string _newPdfPath;
    
    public MainWindow()
    {
        InitializeComponent();
        OperationComboBox.SelectedIndex = 0;
        UpdatePanelVisibility();
    }

    private void OperationComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdatePanelVisibility();
    }
    
    private void UpdatePanelVisibility()
    {
        var selectedItem = (OperationComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        ClearPdfPanel.Visibility = selectedItem == "Clear PDF" ? Visibility.Visible : Visibility.Collapsed;
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
                Path.GetDirectoryName(_selectedPdfPath),
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
                    return;
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

        // TODO: Implement PDF conversion logic here
        MessageBox.Show("PDF conversion operation is currently not implemented.");
    }
}