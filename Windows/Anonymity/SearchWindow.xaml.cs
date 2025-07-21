using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;

namespace Quicker.Windows.MainWindows
{
    public partial class SearchWindow : Window
    {
        private bool _isLoaded = false;

        public SearchWindow()
        {
            InitializeComponent();
        }

        private void SearchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 延迟设置按钮可见性，先显示Border后显示按钮
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateButtonVisibility();
                _isLoaded = true; // 标记已加载
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void UpdateButtonVisibility()
        {
            Btn1.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            Btn2.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            Btn3.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            Btn4.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            Btn5.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// 当窗口失去激活（失去焦点）时关闭窗口。
        /// </summary>
        private void SearchWindow_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 当搜索框获得焦点时，如果内容为“开始搜索...”，则清空文本。
        /// </summary>
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && tb.Text == "开始搜索...")
            {
                tb.Text = ""; //清空文本
                tb.Foreground = Brushes.Black; //恢复默认样式
            }
        }

        /// <summary>
        /// 当搜索框失去焦点时，如果内容为空，则恢复为初始提示文本和样式
        /// </summary>
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = "开始搜索..."; //恢复初始提示文本
                tb.Foreground = Brushes.LightGray; //设置提示文本样式
            }
        }

        // 搜索框内容发生变化时
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoaded)
                UpdateButtonVisibility();
        }
    }
}