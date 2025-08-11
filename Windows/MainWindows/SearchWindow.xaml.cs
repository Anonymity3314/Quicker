using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using System.Diagnostics;
using Quicker.Managers;
using System.Windows;
using System.Text;

namespace Quicker.Windows.MainWindows
{
    public partial class SearchWindow : Window
    {
        private ActionManager actionManager = new();
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
            Activate(); // 激活窗口
        }

        private void UpdateButtonVisibility()
        {
            RunasCommandButton.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            OpenUrlCommandButton.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            SearchBingCommandButton.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            SearchBaiduCommandButton.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
            SearchGoogleCommandButton.Visibility = (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "开始搜索...") ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// 当窗口失去激活（失去焦点）时关闭窗口。
        /// </summary>
        private void SearchWindow_Deactivated(object sender, EventArgs e)
        {
            Close();
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
            {
                UpdateButtonVisibility();
                UpdateSearchUrl();
            }
        }

        // 运行命令
        private void RunasCommandButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string command = SearchBox.Text.Trim('"'); // 去除引号
                var parts = SplitCommand(command); // 分割命令
                Process.Start(new ProcessStartInfo
                {
                    Arguments = parts.arguments,
                    FileName = parts.fileName,
                    UseShellExecute = true
                }); // 运行命令
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2)
            {
                actionManager.ShowToast("找不到指定文件！", ToastType.Error);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 193 || ex.NativeErrorCode == 1155)
            {
                actionManager.ShowToast("文件格式不支持直接执行！", ToastType.Error);
            }
            catch (ArgumentException)
            {
                actionManager.ShowToast("路径格式无效！", ToastType.Error);
            }
            catch
            {
                actionManager.ShowToast("运行失败！", ToastType.Error);
            }
        }

        /// <summary>
        /// 命令行分割辅助方法
        /// </summary>
        /// <param name="command"> 命令行 </param>
        /// <returns> 文件名和参数 </returns>
        private (string fileName, string arguments) SplitCommand(string command)
        {
            command = command.Trim(); // 去除空白字符
            var buffer = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < command.Length; i++)
            {
                char current = command[i];
                if (current == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (current == ' ' && !inQuotes)
                {
                    string fileName = buffer.ToString(); // 截取文件名
                    string arguments = command.Substring(i + 1).TrimStart(); // 截取参数
                    return (fileName, arguments); // 返回结果
                }
                buffer.Append(current); // 添加字符到缓冲区
            }
            return (buffer.ToString(), "");
        }

        // 打开指定网址
        private void OpenUrlCommandButton_Click(object sender, RoutedEventArgs e)
        {
            actionManager.LaunchDefaultBrowser(OpenUrlCommandButton_Text.Text);
        }

        // 用必应搜索
        private void SearchBingCommandButton_Click(object sender, RoutedEventArgs e)
        {
            actionManager.LaunchDefaultBrowser(SearchBingCommandButton_Text.Text); //必应搜索
        }

        // 用百度搜索
        private void SearchBaiduCommandButton_Click(object sender, RoutedEventArgs e)
        {
            actionManager.LaunchDefaultBrowser(SearchBaiduCommandButton_Text.Text); //百度搜索
        }

        // 用谷歌搜索
        private void SearchGoogleCommandButton_Click(object sender, RoutedEventArgs e)
        {
            actionManager.LaunchDefaultBrowser(SearchGoogleCommandButton_Text.Text); //谷歌搜索
        }

        private void UpdateSearchUrl()
        {
            OpenUrlCommandButton_Text.Text = $"http://{SearchBox.Text}.com";
            SearchBingCommandButton_Text.Text = $"https://cn.bing.com/search?q={Uri.EscapeDataString(SearchBox.Text ?? "")}";
            SearchBaiduCommandButton_Text.Text = $"https://www.baidu.com/s?wd={Uri.EscapeDataString(SearchBox.Text ?? "")}";
            SearchGoogleCommandButton_Text.Text = $"https://www.google.com/search?q={Uri.EscapeDataString(SearchBox.Text ?? "")}";
        }
    }
}