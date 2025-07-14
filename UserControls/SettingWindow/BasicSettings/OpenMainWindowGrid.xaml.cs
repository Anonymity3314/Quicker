using Quicker.Windows.MainWindows;
using System.Windows.Controls;
using Quicker.UserControls;
using System.Windows.Input;
using Quicker.Managers;
using System.Windows;

namespace Quicker.UserControls.SettingWindow.BasicSettings
{
    public partial class OpenMainWindowGrid : UserControl
    {
        private WeakReference<Quicker.Windows.MainWindows.SettingWindow> weakSettingWindow; // 弱引用设置窗口
        private List<string> ShortcutKeys = new(); // 保存快捷键
        SettingManager settingManager; // 设置管理器

        public OpenMainWindowGrid(Quicker.Windows.MainWindows.SettingWindow settingWindow)
        {
            InitializeComponent();
            settingManager = settingWindow._settingManager; // 创建设置管理器
            weakSettingWindow = new(settingWindow); // 保存设置窗口
            InitializeAsync(); // 异步初始化
        }

        // 异步初始化方法
        private async void InitializeAsync()
        {
            await LoadSettingsAsync(); // 异步加载设置
        }

        // 异步加载设置
        private async Task LoadSettingsAsync()
        {
            settingManager.LoadOpenMainWindowConditionsAsync(); // 初始化数据库缓存
            Application.Current.Dispatcher.Invoke(() =>
            {
                OpenMainWindowByMiddleMouseClickCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByMiddleMouseClick; // 按下中键
                OpenMainWindowByX1MouseClickCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByX1MouseClick; // 按下X1键
                OpenMainWindowByX2MouseClickCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByX2MouseClick; // 按下X2键
                OpenMainWindowByCtrl_MiddleMouseClickCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick; // Ctrl+中键单击
                OpenMainWindowByCtrl_RightMouseClickCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick; // Ctrl+右键单击
                OpenMainWindowByMiddleMouseClickLongerCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger; // 长按中键
                OpenMainWindowByRightMouseClickLongerCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByRightMouseClickLonger; // 长按右键
                OpenMainWindowByRightMouseClick_MoveCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByRightMouseClick_Move; // 按右键移动
                OpenMainWindowByCtrlCheckBox.IsChecked = settingManager.openMainWindowConditions.OpenMainWindowByCtrl; // 单击Ctrl键
                WindowStartupLocationComboBox.SelectedIndex = settingManager.openMainWindowConditions.WindowStartupLocation; // 功能面板打开位置
            });
        }

        // 勾选框点击事件
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox == null) return;
            bool value = checkBox.IsChecked == true;
            switch (checkBox.Name)
            {
                case "OpenMainWindowByMiddleMouseClickCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByMiddleMouseClick = value;
                    break;
                case "OpenMainWindowByX1MouseClickCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByX1MouseClick = value;
                    break;
                case "OpenMainWindowByX2MouseClickCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByX2MouseClick = value;
                    break;
                case "OpenMainWindowByCtrl_MiddleMouseClickCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByCtrl_MiddleMouseClick = value;
                    break;
                case "OpenMainWindowByCtrl_RightMouseClickCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByCtrl_RightMouseClick = value;
                    break;
                case "OpenMainWindowByMiddleMouseClickLongerCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByMiddleMouseClickLonger = value;
                    break;
                case "OpenMainWindowByRightMouseClickLongerCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByRightMouseClickLonger = value;
                    break;
                case "OpenMainWindowByRightMouseClick_MoveCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByRightMouseClick_Move = value;
                    break;
                case "OpenMainWindowByCtrlCheckBox":
                    settingManager.openMainWindowConditions.OpenMainWindowByCtrl = value;
                    break;
                default:
                    return;
            }
        }

        // 下拉框选择改变事件
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            switch (comboBox.Name)
            {
                case "WindowStartupLocationComboBox":
                    settingManager.openMainWindowConditions.WindowStartupLocation = comboBox.SelectedIndex;
                    break;
                default:
                    return;
            }
        }

        // 基础设置-弹出面板-弹出面板
        private void OpenMainWindowButton_Click(object sender, RoutedEventArgs e)
        {
            settingManager.SetGridVisible(OpenMainWindowButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(OpenMainWindowButton, MainGrid); // 设置Button样式
        }

        // 测试鼠标按键名称
        private void TestMouseKey(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    TestButton.Content = "左键";
                    break; // 左键
                case MouseButton.Right:
                    TestButton.Content = "右键";
                    break; // 右键
                case MouseButton.Middle:
                    TestButton.Content = "中键";
                    break; // 中键
                case MouseButton.XButton1:
                    TestButton.Content = "X1键";
                    break; // X1键
                case MouseButton.XButton2:
                    TestButton.Content = "X2键";
                    break; // X2键
                default:
                    TestButton.Content = "未知按键";
                    break; // 未知按键
            }
        }

        /*
        // 阻止文本输入
        private void DIY_ShortcutKeys_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true; // 阻止文本输入
        }

        // 自定义快捷键
        private void DIY_ShortcutKeys_KeyDown(object sender, KeyEventArgs e)
        {
            if (DIY_ShortcutKeys.IsFocused)
            {
                //e.Handled = true; // 阻止输入
                InputMethod.SetIsInputMethodEnabled(DIY_ShortcutKeys, false); // 禁用输入法
                ModifierKeys modifiers = Keyboard.Modifiers; // 获取修饰键
                Key key = e.Key; // 获取按键

                List<string> keys = new List<string>(); // 保存所有键

                // 清除之前的普通键（非 Ctrl、Shift、Alt）
                List<string> keysToRemove = new List<string>();
                foreach (string keyStr in ShortcutKeys)
                {
                    if (!keyStr.Equals("LeftCtrl") && !keyStr.Equals("RightCtrl") &&
                        !keyStr.Equals("LeftShift") && !keyStr.Equals("RightShift") &&
                        !keyStr.Equals("LeftAlt") && !keyStr.Equals("RightAlt"))
                    {
                        keysToRemove.Add(keyStr);
                    }
                } // 获取所有普通键
                foreach (string keyStr in keysToRemove)
                {
                    ShortcutKeys.Remove(keyStr);
                } // 移除所有普通键

                if (ShortcutKeys == null || ShortcutKeys.Count == 0)
                {
                    switch (key)
                    {
                        case Key.LeftCtrl:
                            if (!ShortcutKeys.Contains("LeftCtrl"))
                            {
                                ShortcutKeys.Add("LeftCtrl");
                            }
                            break; // 添加左Ctrl键
                        case Key.RightCtrl:
                            if (!ShortcutKeys.Contains("RightCtrl"))
                            {
                                ShortcutKeys.Add("RightCtrl");
                            }
                            break; // 添加右Ctrl键
                        case Key.LeftShift:
                            if (!ShortcutKeys.Contains("LeftShift"))
                            {
                                ShortcutKeys.Add("LeftShift");
                            }
                            break; // 添加左Shift键
                        case Key.RightShift:
                            if (!ShortcutKeys.Contains("RightShift"))
                            {
                                ShortcutKeys.Add("RightShift");
                            }
                            break; // 添加右Shift键
                        case Key.LeftAlt:
                            if (!ShortcutKeys.Contains("LeftAlt"))
                            {
                                ShortcutKeys.Add("LeftAlt");
                            }
                            break; // 添加左Alt键
                        case Key.RightAlt:
                            if (!ShortcutKeys.Contains("RightAlt"))
                            {
                                ShortcutKeys.Add("RightAlt");
                            }
                            break; // 添加右Alt键
                        case Key.Back:
                            if (!ShortcutKeys.Contains("Back"))
                            {
                                ShortcutKeys.Add("Back");
                            }
                            break; // 添加Back键
                        default:
                            string keyName = key.ToString();
                            if (ShortcutKeys.Contains(keyName))
                            {
                                ShortcutKeys.Remove(keyName);
                            }
                            ShortcutKeys.Add(keyName);
                            break; // 添加普通键
                    }
                } // 处理单独按键
                else
                {
                    if (modifiers.HasFlag(ModifierKeys.Control))
                    {                       
                        if (ShortcutKeys.Contains("LeftCtrl") || ShortcutKeys.Contains("RightCtrl"))
                        {
                           
                            ShortcutKeys.RemoveAll(key => key == "LeftCtrl" || key == "RightCtrl"); // 移除所有的Ctrl相关键
                           
                            if (!ShortcutKeys.Contains("Ctrl")) // 添加Ctrl
                            {
                                ShortcutKeys.Add("Ctrl");
                            }
                        } // 检查是否存在LeftCtrl或RightCtrl，并替换为Ctrl
                        else
                        {                           
                            if (!ShortcutKeys.Contains("Ctrl"))
                            {
                                ShortcutKeys.Add("Ctrl");
                            }
                        } // 如果没有LeftCtrl或RightCtrl，但有Ctrl modifier，直接添加Ctrl
                    } // 处理Ctrl键                     
                    if (modifiers.HasFlag(ModifierKeys.Shift))
                    {                       
                        if (ShortcutKeys.Contains("LeftShift") || ShortcutKeys.Contains("RightShift"))
                        {                           
                            ShortcutKeys.RemoveAll(key => key == "LeftShift" || key == "RightShift"); // 移除所有的Shift相关键                           
                            if (!ShortcutKeys.Contains("Shift")) // 添加Shift
                            {
                                ShortcutKeys.Add("Shift");
                            }
                        } // 检查是否存在LeftShift或RightShift，并替换为Shift
                        else
                        {
                            // 如果没有LeftShift或RightShift，但有Shift modifier，直接添加Shift
                            if (!ShortcutKeys.Contains("Shift"))
                            {
                                ShortcutKeys.Add("Shift");
                            }
                        }
                    } // 处理Shift键                   
                    if (modifiers.HasFlag(ModifierKeys.Alt))
                    {                       
                        if (ShortcutKeys.Contains("LeftAlt") || ShortcutKeys.Contains("RightAlt"))
                        {                           
                            ShortcutKeys.RemoveAll(key => key == "LeftAlt" || key == "RightAlt"); // 移除所有的Alt相关键                           
                            if (!ShortcutKeys.Contains("Alt")) // 添加Alt
                            {
                                ShortcutKeys.Add("Alt");
                            }
                        } // 检查是否存在LeftAlt或RightAlt，并替换为Alt
                        else
                        {                           
                            if (!ShortcutKeys.Contains("Alt"))
                            {
                                ShortcutKeys.Add("Alt");
                            }
                        } // 如果没有LeftAlt或RightAlt，但有Alt modifier，直接添加Alt
                    } // 处理Alt键
                    if(modifiers.HasFlag(ModifierKeys.Windows))
                    {
                        if (ShortcutKeys.Contains("LeftWindows") || ShortcutKeys.Contains("RightWindows"))
                        {
                            ShortcutKeys.RemoveAll(key => key == "LeftWindows" || key == "RightWindows"); // 移除所有的Windows相关键
                            if (!ShortcutKeys.Contains("Windows")) // 添加Windows
                            {
                                ShortcutKeys.Add("Windows");
                            }
                        } // 检查是否存在LeftWindows或RightWindows，并替换为Windows
                        else
                        {
                            // 如果没有LeftWindows或RightWindows，但有Windows modifier，直接添加Windows
                            if (!ShortcutKeys.Contains("Windows"))
                            {
                                ShortcutKeys.Add("Windows");
                            }
                        }
                    } // 处理Windows键

                    string keyName = key.ToString();
                    if (!ShortcutKeys.Contains(keyName))
                    {
                        ShortcutKeys.Add(keyName);
                    } // 添加普通键
                } // 处理组合键

                // 移除重复的键
                ShortcutKeys = ShortcutKeys.Distinct().ToList();

                // 将键组合成字符串
                string shortcut = string.Join("+", ShortcutKeys);
                DIY_ShortcutKeys.Text = shortcut;
            }
        }

        // 清除之前的输入
        private void DIY_ShortcutKeys_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ShortcutKeys.Clear();
        }

        // 清除文本框内容
        private void ClearDIY_ShortcutKeys(object sender, RoutedEventArgs e)
        {
            DIY_ShortcutKeys.Text = null;
            ShortcutKeys.Clear();
        }
        */
        // 基础设置-弹出面板-动作触发按键
        private void DoActionKeyboard_Click(object sender, RoutedEventArgs e)
        {
            settingManager.SetGridVisible(DoActionKeyboardButtonGrid, MainGrid); // 设置Grid可见性
            settingManager.ButtonStyle3_Click(DoActionKeyboardButton, MainGrid); // 设置Button样式
        }

        // 清理资源
        private void OpenMainWindowGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            OpenMainWindowByMiddleMouseClickCheckBox = null;
            OpenMainWindowByX1MouseClickCheckBox = null;
            OpenMainWindowByX2MouseClickCheckBox = null;
            OpenMainWindowByCtrl_MiddleMouseClickCheckBox = null;
            OpenMainWindowByCtrl_RightMouseClickCheckBox = null;
            OpenMainWindowByMiddleMouseClickLongerCheckBox = null;
            OpenMainWindowByRightMouseClickLongerCheckBox = null;
            OpenMainWindowByRightMouseClick_MoveCheckBox = null;
            OpenMainWindowByCtrlCheckBox = null;
            WindowStartupLocationComboBox = null;
            OpenMainWindowButton = null;
            OpenMainWindowButtonGrid = null;
            DoActionKeyboardButton = null;
            DoActionKeyboardButtonGrid = null;
            TestButton = null; // 清理测试按钮

            settingManager = null; // 释放设置管理器资源
        }
    }
}