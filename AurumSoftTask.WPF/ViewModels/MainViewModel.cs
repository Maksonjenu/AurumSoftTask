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

        private IExportService _exportService;

        [ObservableProperty]
        private ObservableCollection<WellSummary> _summaries;

        [ObservableProperty]
        private ObservableCollection<ValidationError> _errors;

        [ObservableProperty]
        private bool _isLoading;

        public MainViewModel(ICsvParser parser, IWellValidator validator, IWellAnalyzer analyzer, IExportService exporter)
        {
            _summaries = new ObservableCollection<WellSummary>();
            _errors = new ObservableCollection<ValidationError>();

            _parser = parser;
            _wellValidator = validator;
            _wellAnalyzer = analyzer;

            _exportService = exporter;
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

                    //TODO: make mapper
                    List<Well> wells = validatorResult.ValidRows
                    .GroupBy(r => r.WellId)
                    .Select(g => new Well
                    {
                        WellId = g.Key,
                        X = g.First().X,
                        Y = g.First().Y,
                        Intervals = g.Select(r => new Interval
                        {
                            DepthFrom = r.DepthFrom,
                            DepthTo = r.DepthTo,
                            Rock = r.Rock,
                            Porosity = r.Porosity
                        }).ToList()
                    })
                    .ToList();

                    var analyzerResult = await Task.Run(() => _wellAnalyzer.CalculateSummary(wells));

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

        [RelayCommand]
        private async Task SaveFileAsync()
        {
            if (Summaries == null || !Summaries.Any()) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                FileName = $"WellSummary.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _exportService.ExportAsync(Summaries, saveFileDialog.FileName);
                    MessageBox.Show("Данные успешно экспортированы!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
                }
            }
        }
    }
}
