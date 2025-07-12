using Quicker.UserControls.AddWindow;
using System.Windows.Media.Imaging;
using Quicker.Windows.ToolWindows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using Quicker.Managers;
using Quicker.Database;
using Quicker.Windows;
using System.Windows;
using Quicker.Models;
using System.Media;
using System.IO;

namespace Quicker.Windows.MainWindows
{
    public partial class AddWindow : Window
    {
        private const string OPEN_FILE_IMAGE_PATH = "pack://application:,,,/Resources/Images/OpenFileImage.png";
        private const string OPEN_WEBSITE_IMAGE_PATH = "pack://application:,,,/Resources/Images/OpenWebSiteImage.png";
        private const string DEFAULT_IMAGE_PATH = "pack://application:,,,/Resources/Images/Quicker1.png";

        private readonly ButtonManager buttonManager = new(); // 按钮管理器
        private readonly IconManager iconManager = new(); // 图标管理器
        private SelectWindowWindow selectWindowWindow; // SelectWindowWindow 的实例引用
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private FindAppsWindow findAppsWindow; // FindAppsWindow 的实例引用
        private bool isLoading = true; // 是否正在加载
        public string iconPath; // 图标路径

        public int ButtonID { get; private set; } // 当前按钮
        public string TableName { get; private set; } // 表名
        public int Choice { get; private set; } // 选择添加动作类型

        /*
         * 构造函数参数说明：
         * choice：
         * 0：编辑动作
         * 1：启动软件
         * 2：打开文件
         * 3：打开文件夹
         * 4：打开网址
         * 5：加载扩展
         */

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="currentbutton"> 当前按钮 </param>
        /// <param name="tableName"> 表名 </param>
        /// <param name="choice"> 选择添加动作类型 </param>
        public AddWindow(int currentbutton, string tableName, int choice)
        {
            ButtonID = currentbutton; // 当前按钮
            TableName = tableName; // 表名
            Choice = choice; // 选择添加动作类型
            InitializeComponent(); // 初始化窗口组件
            ExecuteChoiceAction(); // 执行对应命令
        }

        // 初始化标题和Button视图，并根据上个窗口数据执行对应命令
        private void AddWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeTitle(); // 初始化标题
            SetDefaultImage(); // 设置默认图标
            SetWindowHeight(ChoiceComboBox.SelectedIndex); // 设置窗口高度
            isLoading = false; // 加载完成
        }

        // 初始化标题
        private void InitializeTitle()
        {
            string pageName = TableName; // 页面名称
            if (Choice != 0) // 如果不是编辑动作
            {
                switch (TableName)
                {
                    case "Global":
                        pageName = "默认全局动作"; // 页面名称
                        break; // 默认全局动作
                    case "TaskBar":
                        pageName = "默认任务栏动作"; // 页面名称
                        break; // 默认任务栏动作
                    case "Desktop":
                        pageName = "默认桌面动作"; // 页面名称
                        break; // 默认桌面动作
                    case "Common":
                        pageName = "默认"; // 页面名称
                        break; // 默认动作
                }
            }
            else // 如果是编辑动作
                pageName = ""; // 页面名称
            int page = (ButtonID / 100) + 1; // 计算页码
            int row = (ButtonID / 10) % 10; // 计算行号
            int column = ButtonID % 10; // 计算列号
            Title = $"新动作--{pageName}第{page}页{row}行{column}列--编辑动作"; // 设置标题
        }

        // 根据上个窗口数据执行对应命令
        private void ExecuteChoiceAction()
        {
            switch (Choice) // 根据选择执行对应命令
            {
                case 0: // 编辑动作
                    LoadActionInfo(); // 加载动作信息
                    break; // 编辑动作, 加载动作信息
                case 1: // 启动软件
                case 2: // 打开文件
                case 3: // 打开文件夹
                    ChoiceComboBox.SelectedIndex = 0;
                    ActionInfoGrid.Children.Add(new OpenFile(this)); // 添加 OpenFile 控件到布局中
                    break; // 选择文件
                case 4:
                    ChoiceComboBox.SelectedIndex = 1;
                    ActionInfoGrid.Children.Add(new OpenWebsite(this)); // 添加 OpenWebsite 控件到布局中
                    break; // 选择网址
                case 5:
                    ChoiceComboBox.SelectedIndex = 2;
                    ActionInfoGrid.Children.Add(new LoadExtension(this)); // 添加 LoadExtension 控件到布局中
                    break; // 加载扩展
                default:
                    break; // 其他情况
            }
        }

        // 编辑动作，加载动作信息
        private void LoadActionInfo()
        {
            ButtonData buttonData = db2.GetButtonDataByID(ButtonID, TableName); // 获取按钮数据
            switch (buttonData.ActionType)
            {
                case "OpenFile":
                case "OpenFiles":
                case "OpenUwpApp":
                    ChoiceComboBox.SelectedIndex = 0;
                    ActionInfoGrid.Children.Add(new OpenFile(this)); // 添加 OpenFile 控件到布局中
                    break;
                case "OpenWebsite":
                    ChoiceComboBox.SelectedIndex = 1;
                    ActionInfoGrid.Children.Add(new OpenWebsite(this)); // 添加 OpenWebsite 控件到布局中
                    break;
                case "LoadExtension":
                    ChoiceComboBox.SelectedIndex = 2;
                    ActionInfoGrid.Children.Add(new LoadExtension(this)); // 添加 LoadExtension 控件到布局中
                    break; // 加载扩展
                default:
                    break; // 其他情况
            }
        }

        // 关闭添加动作窗口
        private void CloseAddWindow(object sender, RoutedEventArgs e)
        {
            this.Close(); // 关闭窗口
        }

        // 管理本地图标
        private void ManageLocalIcons(object sender, RoutedEventArgs e)
        {
            string localIconsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalIcons"); // 动态生成路径
            if (!Directory.Exists(localIconsPath)) Directory.CreateDirectory(localIconsPath); // 如果文件夹不存在，创建它
            Process.Start(new ProcessStartInfo
            {
                FileName = localIconsPath, // 打开文件夹
                UseShellExecute = true // 使用系统外壳程序打开
            });
        }

        // 删除图标
        private void DeleteImage(object sender, RoutedEventArgs e)
        {
            ButtonImage.Source = null;
            ButtonImage.Visibility = Visibility.Collapsed;
        }

        // 处理选中的应用
        private void OnApplicationSelected(object sender, FindAppsWindow.ApplicationSelectedEventArgs e)
        {
            AppInfo selectedApp = e.SelectedApp; // 获取选中的应用信息
            if (selectedApp != null)
            {
                // 更新控件数据
                TitleTextBox.Text = selectedApp.Name; // 设置标题

                // 设置图标
                ButtonImage.Source = selectedApp.Icon; // 设置图标
                ButtonImage.Visibility = Visibility.Visible; // 显示图标
                findAppsWindow.ApplicationSelected -= OnApplicationSelected; // 取消事件订阅
            }
        }

        // 保存动作
        private void Save()
        {
            switch(ChoiceComboBox.SelectedIndex)
            {
                case 0:
                    OpenFile openFile = (OpenFile)ActionInfoGrid.Children[0]; // 获取 OpenFile 控件
                    openFile.Save(); // 保存打开文件动作
                    break; // 选择文件
                case 1:
                    OpenWebsite openWebsite = (OpenWebsite)ActionInfoGrid.Children[0]; // 获取 OpenWebsite 控件
                    openWebsite.Save(); // 保存打开网址动作
                    break; // 选择网址
                case 2:
                    LoadExtension loadExtension = (LoadExtension)ActionInfoGrid.Children[0]; // 获取 LoadExtension 控件
                    loadExtension.Save(); // 保存加载扩展动作
                    break; // 加载扩展
                default:
                    break; // 其他情况
            }
            ActionPageManageWindow actionPageManageWindow = Application.Current.Windows.OfType<ActionPageManageWindow>().FirstOrDefault(); // 尝试查找现有的菜单栏
            if (actionPageManageWindow != null)
                actionPageManageWindow.UpdateButton(ButtonID); // 更新菜单栏按钮
            this.Close(); // 关闭窗口
        }

        // 点击保存按钮保存动作
        private void SaveAction(object sender, RoutedEventArgs e)
        {
            Save(); // 保存动作
        }

        // 按下 S 键保存动作
        private void SaveAction(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.S)
                Save(); // 保存动作
        }

        // 选择已有图标
        private void AddImage(object sender, RoutedEventArgs e)
        {
            SelectImage(sender, e); // 选择本地图片
        }

        // 更改ButtonName
        private void UpdateTitle(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                ButtonTitle.Visibility = Visibility.Visible; // 显示标题
                ButtonTitle.Text = TitleTextBox.Text; // 更新标题
                buttonManager.AutoEllipsisTextBlock(ButtonTitle, 70); // 更新按钮名称
            }
            else
                ButtonTitle.Visibility = Visibility.Collapsed; // 隐藏标题
            UpdateTooltip(); // 更新提示文本
        }

        // 更新提示文本
        public void UpdateTooltip()
        {
            string toolTipText = null; // 提示文本
            if (!string.IsNullOrWhiteSpace(TitleTextBox.Text) || !string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                string name = !string.IsNullOrWhiteSpace(TitleTextBox.Text) ? TitleTextBox.Text : null; // 获取按钮名称
                string usage = !string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? DescriptionTextBox.Text : null; // 获取按钮用途
                toolTipText = (name + "\n" + usage).Trim('\n'); // 设置按钮提示文本
            } // 如果按钮名称或用途不为空
            ButtonView.ToolTip = string.IsNullOrEmpty(toolTipText) ? null : toolTipText; // 设置按钮提示文本
        }

        // 如果FindAppsWindow或SelectWindowWindow存在，则闪烁窗口并响起提示音
        private void AddWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (findAppsWindow != null)
            {
                SystemSounds.Beep.Play(); // 播放提示音
                findAppsWindow.Focus(); // 设置焦点
            }
            else if (selectWindowWindow != null)
            {
                SystemSounds.Beep.Play(); // 播放提示音
                selectWindowWindow.Focus(); // 设置焦点
            }
        }

        // 选择本地图片
        private void SelectImage(object sender, RoutedEventArgs e)
        {
            SelectImageWindow selectImageWindow = new(); // 创建 SelectImageWindow 实例
            selectImageWindow.ImageConfirmed += OnImageConfirmed; // 订阅 ImageConfirmed 事件
            selectImageWindow.Owner = this; // 设置所有者为当前窗口
            selectImageWindow.ShowDialog(); // 显示为模式对话框
        }

        // 选择窗口
        private void SelectWindow(object sender, RoutedEventArgs e)
        {
            // 检查当前是否为OpenFile控件
            if (ActionInfoGrid.Children.Count == 0 || !(ActionInfoGrid.Children[0] is OpenFile))
            {
                ChoiceComboBox.SelectedIndex = 0; // 切换到OpenFile控件
            }
            
            selectWindowWindow = new(); // 创建 SelectWindowWindow 实例
            selectWindowWindow.WindowSelected += OnWindowSelected; // 订阅 WindowSelected 事件
            selectWindowWindow.StartSelecting(this); // 开始选择窗口
        }
        
        // 处理选中的窗口
        private void OnWindowSelected(object sender, SelectWindowWindow.WindowSelectedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ProcessPath))
            {
                try
                {
                    // 更新控件数据
                    TitleTextBox.Text = Path.GetFileNameWithoutExtension(e.ProcessPath); // 设置标题为进程名
                    // 更新OpenFile控件中的路径
                    if (ActionInfoGrid.Children.Count > 0 && ActionInfoGrid.Children[0] is OpenFile openFile)
                    {
                        openFile.LocationTextBox.Text = e.ProcessPath; // 设置路径
                        SaveButton.IsEnabled = true; // 启用保存按钮
                    }
                    
                    // 设置图标
                    if (e.ProcessIcon != null)
                    {
                        ButtonImage.Source = e.ProcessIcon; // 设置图标
                        ButtonImage.Visibility = Visibility.Visible; // 显示图标
                    }
                }
                catch
                {

                }
                finally
                {
                    // 取消事件订阅并关闭选择窗口
                    if (selectWindowWindow != null)
                    {
                        selectWindowWindow.WindowSelected -= OnWindowSelected;
                        selectWindowWindow.Close();
                        selectWindowWindow = null;
                    }
                }
            }
            this.Activate();
        }

        // 处理选择的图片
        private void OnImageConfirmed(object sender, string selectedImagePath)
        {           
            if (!string.IsNullOrEmpty(selectedImagePath))
            {
                SetImageSource(selectedImagePath);
            }
        }

        // 更新提示文本
        private void UsageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTooltip(); // 更新提示文本
        }

        // 下拉框选项改变时，执行对应命令
        private void ChoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoading) return; // 如果正在加载，则不执行命令
            ActionInfoGrid.Children.Clear(); // 清除子控件
            SetAddWindowChild(); // 设置子控件
            SetDefaultImage(); // 设置默认图标
        }

        // 设置子控件
        public void SetAddWindowChild()
        {
            switch (ChoiceComboBox.SelectedIndex)
            {
                case 0:
                    ActionInfoGrid.Children.Add(new OpenFile(this)); // 添加控件
                    SetWindowHeight(0); // 设置窗口高度
                    break; // 编辑动作
                case 1:
                    ActionInfoGrid.Children.Add(new OpenWebsite(this)); // 添加控件
                    SetWindowHeight(1); // 设置窗口高度
                    break; // 选择应用程序
                case 2:
                    ActionInfoGrid.Children.Add(new LoadExtension(this)); // 添加 LoadExtension 控件到布局中
                    SetWindowHeight(0); // 设置窗口高度
                    break; // 加载扩展
                default:
                    {
                        using var toast = new ToastManager(); // 创建 ToastManager 实例
                        toast.Show("功能开发中，敬请期待！", "Common"); // 显示提示消息
                    }
                    break; // 其他情况
            }
        }

        /// <summary>
        /// 根据选择的选项设置窗口高度
        /// </summary>
        /// <param name="choice"> 选择的选项 </param>
        private void SetWindowHeight(int choice)
        {
            switch (choice)
            {
                case 0:
                    this.Height = 450; // 打开文件的窗口高度
                    break; // 打开文件
                case 1:
                    this.Height = 370; // 打开网址的窗口高度
                    break; // 打开网站
                default:
                    break; // 其他情况
            }
        }

        /// <summary>
        /// 设置图片源
        /// </summary>
        /// <param name="imagePath"> 图片路径 </param>
        private void SetImageSource(string imagePath)
        {
            try
            {
                ButtonImage.Source = new BitmapImage(new Uri(imagePath)); // 设置图片源
                ButtonImage.Visibility = Visibility.Visible; // 显示图标
            }
            catch
            {
                using var toast = new ToastManager(); // 创建 ToastManager 实例
                toast.Show("加载图片失败!", "Error"); // 显示提示消息
                ButtonImage.Visibility = Visibility.Collapsed; // 隐藏图标
            }
        }

        // 根据选择的选项设置默认图标
        private void SetDefaultImage()
        {
            ButtonImage.Source = null; // 清空图标
            string imagePath = ChoiceComboBox.SelectedIndex switch
            {
                0 => OPEN_FILE_IMAGE_PATH,
                1 => OPEN_WEBSITE_IMAGE_PATH,
                _ => DEFAULT_IMAGE_PATH
            }; // 根据选择的选项设置默认图标路径
            SetImageSource(imagePath); // 设置图标
        }

        // 关闭窗口前，释放资源
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e); // 调用基类的 OnClosed 方法

            if (findAppsWindow != null)
            {
                findAppsWindow.ApplicationSelected -= OnApplicationSelected; // 取消事件订阅
                findAppsWindow = null; // 清理静态引用
            }
            
            if (selectWindowWindow != null)
            {
                selectWindowWindow.WindowSelected -= OnWindowSelected; // 取消事件订阅
                selectWindowWindow.Close(); // 关闭选择窗口
                selectWindowWindow = null; // 清理静态引用
            }
            iconPath = null;

            GC.Collect(); // 强制垃圾回收
            GC.WaitForPendingFinalizers(); // 等待垃圾回收完成
            GC.Collect(); // 再次强制垃圾回收
        }
    }
}