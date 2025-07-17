using System.ComponentModel;
using Quicker.Database;

namespace Quicker.Windows.MainWindows.MainWindow
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private double _backgroundImageOpacity;
        private string _backgroundImagePath;
        private int _win11CornerRadius;
        private string _backgroundColor;
        private double _buttonSize;
        private double _buttonGap;
        private string _toolbarColor;
        private double _borderWidth;
        private double _buttonCornerRadius;
        private string _borderColor;
        private string _blankButtonColor;
        private string _blankButtonMouseOverColor;
        private double _fontSize;
        private int _fontWeight;
        private string _actionButtonColor;
        private string _actionButtonMouseOverColor;
        private bool _showActionButtonMouseOver;

        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set { _backgroundImagePath = value; OnPropertyChanged(nameof(BackgroundImagePath)); }
        }

        public double BackgroundImageOpacity
        {
            get => _backgroundImageOpacity;
            set { _backgroundImageOpacity = value; OnPropertyChanged(nameof(BackgroundImageOpacity)); }
        }

        public int Win11CornerRadius
        {
            get => _win11CornerRadius;
            set { _win11CornerRadius = value; OnPropertyChanged(nameof(Win11CornerRadius)); }
        }

        /// <summary>
        /// 外观设置中的背景色（如 #FFF3F3F3）
        /// </summary>
        public string BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; OnPropertyChanged(nameof(BackgroundColor)); }
        }

        public double ButtonSize
        {
            get => _buttonSize;
            set { _buttonSize = value; OnPropertyChanged(nameof(ButtonSize)); }
        }

        public double ButtonGap
        {
            get => _buttonGap;
            set { _buttonGap = value; OnPropertyChanged(nameof(ButtonGap)); }
        }

        /// <summary>
        /// 外观设置中的工具栏颜色（如 #00F3F3F3）
        /// </summary>
        public string ToolbarColor
        {
            get => _toolbarColor;
            set { _toolbarColor = value; OnPropertyChanged(nameof(ToolbarColor)); }
        }

        public double BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; OnPropertyChanged(nameof(BorderWidth)); }
        }
        public double ButtonCornerRadius
        {
            get => _buttonCornerRadius;
            set { _buttonCornerRadius = value; OnPropertyChanged(nameof(ButtonCornerRadius)); }
        }
        public string BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; OnPropertyChanged(nameof(BorderColor)); }
        }
        public string BlankButtonColor
        {
            get => _blankButtonColor;
            set { _blankButtonColor = value; OnPropertyChanged(nameof(BlankButtonColor)); }
        }
        public string BlankButtonMouseOverColor
        {
            get => _blankButtonMouseOverColor;
            set { _blankButtonMouseOverColor = value; OnPropertyChanged(nameof(BlankButtonMouseOverColor)); }
        }
        public double FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }
        public int FontWeight
        {
            get => _fontWeight;
            set { _fontWeight = value; OnPropertyChanged(nameof(FontWeight)); }
        }

        public string ActionButtonColor
        {
            get => _actionButtonColor;
            set { _actionButtonColor = value; OnPropertyChanged(nameof(ActionButtonColor)); }
        }
        public string ActionButtonMouseOverColor
        {
            get => _actionButtonMouseOverColor;
            set { _actionButtonMouseOverColor = value; OnPropertyChanged(nameof(ActionButtonMouseOverColor)); }
        }

        public bool ShowActionButtonMouseOver
        {
            get => _showActionButtonMouseOver;
            set { _showActionButtonMouseOver = value; OnPropertyChanged(nameof(ShowActionButtonMouseOver)); }
        }

        public MainWindowViewModel()
        {
            // 加载数据库数据
            var appearance = SettingDatabase.GetAllAppearanceSettings()?.FirstOrDefault();
            if (appearance != null)
            {
                BackgroundImagePath = appearance.BackgroundImagePath;
                BackgroundImageOpacity = appearance.BackgroundImageOpacity;
                Win11CornerRadius = appearance.Win11CornerRadius;
                BackgroundColor = appearance.BackgroundColor;
                ButtonSize = appearance.ButtonSize;
                ButtonGap = appearance.ButtonGap;
                ToolbarColor = appearance.ToolbarColor;
                BorderWidth = appearance.BorderWidth;
                ButtonCornerRadius = appearance.ButtonCornerRadius;
                BorderColor = appearance.BorderColor;
                BlankButtonColor = appearance.BlankButtonColor;
                BlankButtonMouseOverColor = appearance.BlankButtonMouseOverColor;
                FontSize = appearance.FontSize;
                FontWeight = appearance.FontWeight;
                ActionButtonColor = appearance.ActionButtonColor;
                ActionButtonMouseOverColor = appearance.ActionButtonMouseOverColor;
                ShowActionButtonMouseOver = appearance.ShowActionButtonMouseOver;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}