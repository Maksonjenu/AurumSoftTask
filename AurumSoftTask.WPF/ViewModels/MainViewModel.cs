using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;


namespace AurumSoftTask.WPF.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {

        private ICsvParser _parser;
        private IWellValidator _wellValidator;
        private IWellAnalyzer _wellAnalyzer;

        [ObservableProperty]
        private ObservableCollection<WellSummary> _summaries;

        [ObservableProperty]
        private ObservableCollection<ValidationError> _errors;

        [ObservableProperty]
        private bool _isLoading;

        public MainViewModel(ICsvParser parser, IWellValidator validator, IWellAnalyzer analyzer)
        {
            _summaries = new ObservableCollection<WellSummary>();
            _errors = new ObservableCollection<ValidationError>();

            _parser = parser;
            _wellValidator = validator;
            _wellAnalyzer = analyzer;
        }

        [RelayCommand]
        private async Task LoadFileAsync()
        {
            OpenFileDialog openFileDialog
                = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
                };

            if (openFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                try
                {
                    var parseResult = await Task.Run(() => _parser.Parse(openFileDialog.FileName));

                    var validatorResult = await Task.Run(() => _wellValidator.Validate(parseResult.Rows));

                    var analyzerResult = await Task.Run(() => _wellAnalyzer.CalculateSummary(validatorResult.ValidRows));

                    Summaries = new ObservableCollection<WellSummary>(analyzerResult);
                    var allErrors = parseResult.ParseErrors.Concat(validatorResult.ValidationErrors);
                    Errors = new ObservableCollection<ValidationError>(allErrors);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }
}
