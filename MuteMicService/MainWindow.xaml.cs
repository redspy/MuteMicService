using MuteMicService.Controls;
using MuteMicService.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MuteMicService
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the ViewModel
            _viewModel = new MainViewModel();

            // Set DataContext
            DataContext = _viewModel;

            // Close event to cleanup resources
            Closed += (s, e) => _viewModel.Dispose();
        }
    }

    public class BoolToMuteTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMuted)
            {
                return isMuted ? "Muted" : "Not Muted";
            }

            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToButtonBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEnabled)
            {
                return isEnabled ? new SolidColorBrush(Color.FromRgb(232, 87, 87)) : new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StatusStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusState state)
            {
                switch (state)
                {
                    case StatusState.Success:
                        return Brushes.Green;
                    case StatusState.Warning:
                        return Brushes.Orange;
                    case StatusState.Error:
                        return Brushes.Red;
                    case StatusState.Active:
                        return Brushes.Blue;
                    case StatusState.Inactive:
                        return Brushes.Gray;
                    case StatusState.Neutral:
                    default:
                        return Brushes.Gray;
                }
            }

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
