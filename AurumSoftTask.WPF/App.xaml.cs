using AurumSoftTask.Services.Implementation;
using AurumSoftTask.WPF.ViewModels;
using AurumSoftTask.WPF.Views;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AurumSoftTask.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // dependency injection without a container
            var parser = new CsvParser();
            var validator = new WellValidator();
            var analyzer = new WellAnalyzer();

            var normalizer = new LMStudioDataNormalizationService();

            var exporter = new JsonExportService();

            var mainViewModel = new MainViewModel(parser, validator, analyzer, exporter, normalizer);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }

    }

}
