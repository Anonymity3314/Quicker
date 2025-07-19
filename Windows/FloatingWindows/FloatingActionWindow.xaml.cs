using Quicker.Database.Core;
using Quicker.Windows.Menus;
using System.Windows.Input;
using System.Windows.Media;
using Quicker.Managers;
using System.Windows;
using Quicker.Models;

namespace Quicker.Windows.FloatingWindows
{
    public partial class FloatingActionWindow : Window
    {
        public int ButtonID { get; private set; } // 当前按钮
        public string TableName { get; private set; } // 表名

        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private SolidColorBrush _normalBrush; // 按钮初始背景色
        private SolidColorBrush _hoverBrush; // 按钮悬停背景色
        private bool _isDragging = false; // 是否正在拖动
        private Point _mouseDownPoint; // 鼠标按下时的位置

        /// <summary>
        /// 悬浮动作窗体
        /// </summary>
        /// <param name="buttonID">按钮ID</param>
        /// <param name="tableName">表名</param>
        public FloatingActionWindow(int buttonID, string tableName)
        {
            InitializeComponent();
            ButtonID = buttonID; // 设置当前按钮
            TableName = tableName; // 设置表名
        }

        // 加载窗体
        private void FloatingActionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowPositionAndTopmost(); // 设置窗口位置和置顶
            SetWindowSizeAndBackground(); // 设置窗口大小和背景色
            InitButtonAppearance(); // 初始化按钮外观
            LoadButtonData(); // 加载按钮数据并刷新显示
        }

        /// <summary>
        /// 设置窗口位置和置顶
        /// </summary>
        private void SetWindowPositionAndTopmost()
        {
            using var windowManager = new WindowManager(); // 创建窗口管理器
            windowManager.SetWindowPositionNearMouse(this); // 设置窗口位置
            windowManager.SetWindowTopmost(this); // 设置窗口置顶
        }

        /// <summary>
        /// 设置窗口大小和背景色
        /// </summary>
        private void SetWindowSizeAndBackground()
        {
            var appearance = SettingDatabase.GetAllAppearanceSettings().FirstOrDefault(); // 获取外观设置
            Border.Height = appearance.ButtonSize; // 设置高度
            Border.Width = appearance.ButtonSize; // 设置宽度
            _normalBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(appearance.ActionButtonColor)); // 设置按钮初始背景色
            _hoverBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(appearance.ActionButtonMouseOverColor)); // 设置按钮悬停背景色
        }

        /// <summary>
        /// 初始化按钮外观
        /// </summary>
        private void InitButtonAppearance()
        {
            Button.Background = _normalBrush; // 设置按钮初始背景色
        }

        /// <summary>
        /// 加载按钮数据并刷新显示
        /// </summary>
        private void LoadButtonData()
        {
            var buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            Button.Tag = buttonData; // 绑定数据到按钮
            buttonManager.RefreshButtonDisplay(Button, buttonData, 60, true); // 刷新按钮显示
        }

        /// <summary>
        /// 鼠标左键按下时记录初始位置，准备判断是否为拖动
        /// </summary>
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownPoint = e.GetPosition(this); // 记录初始位置
            _isDragging = false; // 重置拖动状态
        }

        /// <summary>
        /// 鼠标移动时判断是否达到拖动阈值，若达到则拖动窗口
        /// </summary>
        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) // 判断是否左键按下
            {
                Point currentPoint = e.GetPosition(this); // 记录当前位置
                if (!_isDragging && (Math.Abs(currentPoint.X - _mouseDownPoint.X) > 5 ||
                    Math.Abs(currentPoint.Y - _mouseDownPoint.Y) > 5)) // 判断是否达到拖动阈值
                {
                    _isDragging = true; // 开始拖动
                    DragMove(); // 开始拖动窗口
                }
            }
        }

        /// <summary>
        /// 鼠标左键释放时重置拖动状态
        /// </summary>
        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false; // 重置拖动状态
        }

        /// <summary>
        /// 按钮点击事件，未拖动时执行按钮动作
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!_isDragging)
            {
                var buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
                using (var actionManager = new ActionManager())
                {
                    actionManager.DoAction(buttonData); // 执行动作
                } // 执行动作
                db2.IncreaseActionUsedTimes(ButtonID, TableName); // 增加动作使用次数
            }
        }

        /// <summary>
        /// 鼠标移入按钮时切换为悬停背景色
        /// </summary>
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button.Background = _hoverBrush; // 设置悬停背景色
        }

        /// <summary>
        /// 鼠标移出按钮时恢复为普通背景色
        /// </summary>
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button.Background = _normalBrush; // 设置按钮初始背景色
        }

        /// <summary>
        /// 右键点击按钮时显示操作菜单
        /// </summary>
        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            OperationMenu menu = new OperationMenu(ButtonID, TableName, this, false); // 创建操作菜单
            menu.Show(); // 显示菜单
        }

        /// <summary>
        /// 关闭窗口释放资源
        /// </summary>
        private void FloatingActionWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            // 释放引用
            Button.Tag = null;
            _normalBrush = null;
            _hoverBrush = null;
        }
    }
}