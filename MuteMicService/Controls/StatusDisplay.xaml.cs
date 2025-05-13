using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MuteMicService.Controls
{
    public partial class StatusDisplay : UserControl
    {
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(StatusDisplay), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DetailTextProperty =
            DependencyProperty.Register("DetailText", typeof(string), typeof(StatusDisplay), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty StatusColorProperty =
            DependencyProperty.Register("StatusColor", typeof(Brush), typeof(StatusDisplay), new PropertyMetadata(Brushes.Gray));

        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public string DetailText
        {
            get { return (string)GetValue(DetailTextProperty); }
            set { SetValue(DetailTextProperty, value); }
        }

        public Brush StatusColor
        {
            get { return (Brush)GetValue(StatusColorProperty); }
            set { SetValue(StatusColorProperty, value); }
        }

        public StatusDisplay()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void SetStatus(string status, string detail = null, StatusState state = StatusState.Neutral)
        {
            StatusText = status;
            DetailText = detail ?? string.Empty;
            StatusColor = GetBrushForState(state);
        }

        private Brush GetBrushForState(StatusState state)
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
                case StatusState.Neutral:
                default:
                    return Brushes.Gray;
            }
        }
    }

    public enum StatusState
    {
        Neutral,
        Success,
        Warning,
        Error,
        Active,
        Inactive
    }
}