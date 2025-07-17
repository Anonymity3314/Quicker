using System.ComponentModel;
using Quicker.Database;

namespace Quicker.Windows.MainWindows.MainWindow
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        // 尺寸
        private double _buttonSize; // 按钮大小
        private double _buttonGap; // 按钮间隙
        private double _borderWidth; // 边框宽度
        private double _buttonCornerRadius; // 按钮圆角

        // 颜色
        private string _backgroundColor; // 背景颜色
        private string _borderColor; // 边框颜色
        private string _toolbarColor; // 工具栏颜色
        private string _toolbarIconColor; // 工具栏图标颜色
        private string _actionButtonColor; // 动作按钮颜色
        private string _actionButtonMouseOverColor; // 动作按钮鼠标悬停颜色
        private string _blankButtonColor; // 空白按钮颜色
        private string _blankButtonMouseOverColor; // 空白按钮鼠标悬停颜色
        private string _buttonTextColor; // 按钮文字颜色

        // 字体
        private double _fontSize; // 字体大小
        private int _fontWeight; // 字体粗细

        // 背景图片
        private string _backgroundImagePath; // 背景图片路径
        private double _backgroundImageOpacity; // 背景图片不透明度

        // 模糊与圆角
        private int _blur; // 模糊模式
        private int _win11CornerRadius; // Win11圆角模式

        // 选项
        private bool _showActionButtonMouseOver; // 鼠标悬浮在动作按钮上时，放大显示按钮
        private bool _showAddImage;

        private bool _isPinned;
        private bool _isLocked;

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

        public string ButtonTextColor
        {
            get => _buttonTextColor;
            set { _buttonTextColor = value; OnPropertyChanged(nameof(ButtonTextColor)); }
        }

        public string ToolbarIconColor
        {
            get => _toolbarIconColor;
            set { _toolbarIconColor = value; OnPropertyChanged(nameof(ToolbarIconColor)); }
        }

        public bool ShowAddImage
        {
            get => _showAddImage;
            set { _showAddImage = value; OnPropertyChanged(nameof(ShowAddImage)); }
        }

        public bool IsPinned
        {
            get => _isPinned;
            set { _isPinned = value; OnPropertyChanged(nameof(IsPinned)); }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set { _isLocked = value; OnPropertyChanged(nameof(IsLocked)); }
        }

        public int Blur
        {
            get => _blur;
            set { _blur = value; OnPropertyChanged(nameof(Blur)); }
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
                ButtonTextColor = appearance.ButtonTextColor;
                ToolbarIconColor = appearance.ToolbarIconColor;
                Blur = appearance.Blur;
            }
            var convention = SettingDatabase.GetAllConventions()?.FirstOrDefault();
            ShowAddImage = convention?.ShowAddImage ?? false;
            IsPinned = AppStateManager.Pinned;
            IsLocked = AppStateManager.Locked;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}