using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Quicker.CommonFunctions;
using System.Windows.Input;
using System.Windows.Media;
using Quicker.Database;
using System.Windows;

namespace Quicker.Windows
{
    public partial class ActionPageManageWindow : Window
    {
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T tChild) yield return tChild;
                foreach (var grandChild in FindVisualChildren<T>(child)) yield return grandChild;
            }
        } // 查找子元素

        private int TotalGlobalAntionPageIndex, TotalCommonActionPageIndex; // 全局和公共动作页索引
        private Dictionary<string, ButtonData> buttonDataDict; // 按钮数据字典
        private bool shouldHideTooltip, isDragging = false; // 是否正在拖拽
        private readonly IButtonManager buttonManager; // 按钮管理器
        private readonly SettingDatabase db1; // 设置数据库
        private readonly ButtonDatabase db2; // 按钮数据库
        private Point initialMousePosition; // 初始鼠标位置
        private Button SourceButton; // 源按钮

        public ActionPageManageWindow()
        {
            InitializeComponent();
            GlobalStackPanel.Children.Clear();

            db1 = new SettingDatabase();
            db1.InitializeDatabase();

            db2 = new ButtonDatabase();
            db2.InitializeDatabase();

            buttonManager = new ButtonManager();
        }

        private async void ActionPageManageWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var buttonDataList =  db2.GetAllButtonData();
            buttonDataDict = buttonDataList.ToDictionary(data => data.ButtonID);

            GetTotalAntionPageIndex();
            LoadGlobalCanvas();

            var Convention = db1.GetAllConventions().FirstOrDefault();
            shouldHideTooltip = Convention.HideTooltip;
        }

        private void GetTotalAntionPageIndex()
        {
            TotalGlobalAntionPageIndex = 0;
            TotalCommonActionPageIndex = 0;

            if (buttonDataDict == null || buttonDataDict.Count == 0) return;

            foreach (var data in buttonDataDict.Values)
            {
                string buttonID = data.ButtonID;
                Match match = Regex.Match(data.ButtonID, @"^([a-zA-Z0-9_]+)(\d{3})$");
                if (match.Success)
                {
                    string style = match.Groups[1].Value;
                    string numbersStr = match.Groups[2].Value;
                    int[] numbers = numbersStr.Select(c => int.Parse(c.ToString())).ToArray();

                    if (style == "Global")
                    {
                        if (numbers[0] > TotalGlobalAntionPageIndex) TotalGlobalAntionPageIndex = numbers[0];
                    }
                    else if (style == "Common")
                    {
                        if (numbers[0] > TotalCommonActionPageIndex) TotalCommonActionPageIndex = numbers[0];
                    }
                }
            }
        }

        private void LoadGlobalCanvas()
        {
            for (int i = 0; i <= TotalGlobalAntionPageIndex; i++)
            {
                GenerateCanvas(i, "Global");
            }
        }

        private void GenerateCanvas(int canvasIndex, string style)
        {
            string canvasName = $"{style}{canvasIndex}";

            Canvas dynamicCanvas = new Canvas
            {
                Width = 260,
                Height = style == "Global" ? 215 : 280,
                Name = canvasName,
                Margin = new Thickness(3, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Grid grid = new Grid
            {
                Height = 20,
                Width = 260,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"))
            };

            dynamicCanvas.Children.Add(grid);

            double buttonSpacing = 65;
            int rows = style == "Global" ? 3 : 4;
            int cols = 4;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int buttonIndex = row * 4 + col + 1;
                    string buttonName = $"{style}{canvasIndex}{row + 1}{col + 1}";

                    Button button = new Button
                    {
                        Name = buttonName,
                        Style = FindResource("ActionButton") as Style,
                        Margin = new Thickness(col * buttonSpacing, row * buttonSpacing + grid.Height, 0, 0),
                    };

                    BindButtonEvents(button);

                    dynamicCanvas.Children.Add(button);

                    if (buttonDataDict != null && buttonDataDict.TryGetValue(buttonName, out ButtonData data))
                    {
                        RefreshButtonDisplay(button, data);
                        button.Tag = data;
                    }
                }
            }

            GlobalStackPanel.Children.Add(dynamicCanvas);
        }

        private void RefreshButtonDisplay(Button button, ButtonData buttonInformation)
        {
            if (buttonInformation != null)
            {
                Grid grid = new();
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));

                if (buttonInformation.ImagePath != "none")
                {
                    try
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        Image image = new()
                        {
                            Source = new BitmapImage(new Uri(buttonInformation.ImagePath)),
                            Width = 30,
                            Height = 30,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        grid.Children.Add(image);
                        Grid.SetRow(image, 0);
                    }
                    catch
                    {
                        new ToastContentBuilder().AddText($"图标加载失败：按钮{buttonInformation.ButtonName}的图标被移动或删除").Show();
                    }
                }

                if (!string.IsNullOrEmpty(buttonInformation.ButtonName))
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    TextBlock textBlock = new()
                    {
                        Text = buttonInformation.ButtonName,
                        TextWrapping = TextWrapping.NoWrap,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                    };
                    buttonManager.AutoEllipsisTextBlock(textBlock, 60);
                    grid.Children.Add(textBlock);
                    Grid.SetRow(textBlock, 1);
                }

                button.Content = grid;

                if (!shouldHideTooltip)
                {
                    string toolTipText = null;
                    if (!string.IsNullOrWhiteSpace(buttonInformation.ButtonName) || !string.IsNullOrWhiteSpace(buttonInformation.Usage))
                    {
                        string name = !string.IsNullOrWhiteSpace(buttonInformation.ButtonName) ? buttonInformation.ButtonName : null;
                        string usage = !string.IsNullOrWhiteSpace(buttonInformation.Usage) ? buttonInformation.Usage : null;
                        toolTipText = (name + "\n" + usage).Trim('\n');
                    }
                    button.ToolTip = string.IsNullOrEmpty(toolTipText) ? null : toolTipText;
                }
            }
            else
            {
                button.Content = null;
                button.ToolTip = null;
                button.Tag = null;
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
            }
        }

        private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollViewer.ScrollToHorizontalOffset(ScrollBar.Value);
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ButtonData)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BEE6FD"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAEAEA"));
                }
            }
        }

        private void Button_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is ButtonData)
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                }
                else
                {
                    button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
                }
            }
        }

        private void BindButtonEvents(Button button)
        {
            button.MouseEnter += Button_MouseEnter;
            button.MouseLeave += Button_MouseLeave;
            button.MouseRightButtonDown += OpenCreatActionMenu;
            button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
            button.PreviewMouseMove += Button_PreviewMouseMove;
            button.PreviewMouseLeftButtonUp += Button_PreviewMouseLeftButtonUp;
            button.AllowDrop = true;
            button.Drop += Button_Drop;
            button.DragEnter += Button_DragEnter;
            button.Click += ShowCreatActionMenu;
            button.MouseDoubleClick += ShowEditWindow;
        }

        private void Button_Drop(object sender, DragEventArgs e)
        {
            if (sender is Button TargetButton)
            {
                if (e.Data.GetDataPresent(typeof(ButtonData)))
                {
                    db2.ExchangeButtonID(SourceButton.Name, TargetButton.Name);

                    var TargetData = db2.GetButtonDataByID(SourceButton.Name);
                    RefreshButtonDisplay(SourceButton, TargetData);
                    SourceButton.Tag = TargetData;

                    var SourceData = db2.GetButtonDataByID(TargetButton.Name);
                    RefreshButtonDisplay(TargetButton, SourceData);
                    TargetButton.Tag = SourceData;

                    buttonDataDict[SourceButton.Name] = TargetData;
                    buttonDataDict[TargetButton.Name] = SourceData;
                }
            }
        }

        public void Button_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move; // 设置拖拽效果为移动
            e.Handled = true; // 标记事件已处理
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                initialMousePosition = e.GetPosition(this);
                SourceButton = button;
                isDragging = false;
            }
        }

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Button button && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - initialMousePosition.X;
                double deltaY = currentPosition.Y - initialMousePosition.Y;
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                if (distance > 10 && !isDragging)
                {
                    isDragging = true;
                    if (button.Tag is ButtonData data)
                    {
                        DragDrop.DoDragDrop(button, data, DragDropEffects.Move);
                    }
                }
            }
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void ShowCreatActionMenu(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag == null)
            {
                Point mousePosition = Mouse.GetPosition(this);
                double left = mousePosition.X + 310.4, top = mousePosition.Y + 596 / 3;
                CreatActionMenu creatActionMenu = Application.Current.Windows.OfType<CreatActionMenu>().FirstOrDefault();
                creatActionMenu?.Close();
                creatActionMenu = new(button.Name)
                {
                    Left = left,
                    Top = top
                };
                creatActionMenu.Show();
            }
        }

        private void ShowEditWindow(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                AddWindow addWindow = new AddWindow(button.Name, 0);
                addWindow.Show();
                addWindow.Activate();
            }
        }

        private void AddActionPage(object sender, RoutedEventArgs e)
        {
            int canvasIndex = GlobalStackPanel.Children.Count;
            if (canvasIndex > 9) return;
            GenerateCanvas(canvasIndex, "Global");
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollBar.Maximum = ScrollViewer.ExtentWidth - ScrollViewer.ViewportWidth;
            ScrollBar.ViewportSize = ScrollViewer.ViewportWidth;
            ScrollBar.Value = ScrollViewer.HorizontalOffset;
        }

        private void GlobalButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalStackPanel.Children.Clear();
            LoadGlobalCanvas();
            MainBorder.Margin = new Thickness(239, 31, 11, 564);
            ScrollBar.Margin = new Thickness(240, 241.8, 10, 0);
        }

        private void CommonButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalStackPanel.Children.Clear();
            LoadCommonCanvas();
            MainBorder.Margin = new Thickness(239, 31, 11, 499);
            ScrollBar.Margin = new Thickness(240, 307.15, 10, 0);
        }

        private void LoadCommonCanvas()
        {
            for (int i = 0; i <= TotalCommonActionPageIndex; i++)
            {
                GenerateCanvas(i, "Common");
            }
        }

        private void OpenCreatActionMenu(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                Point mousePosition = Mouse.GetPosition(this);
                double left = mousePosition.X + 310.4, top = mousePosition.Y + 596 / 3;
                if (button.Tag is ButtonData)
                {
                    OperationMenu operationMenu = Application.Current.Windows.OfType<OperationMenu>().FirstOrDefault();
                    operationMenu?.Close();
                    operationMenu = new(button.Name)
                    {
                        Left = left,
                        Top = top
                    };
                    operationMenu.Show();
                }
                else
                {
                    CreatActionMenu creatActionMenu = Application.Current.Windows.OfType<CreatActionMenu>().FirstOrDefault();
                    creatActionMenu?.Close();
                    creatActionMenu = new(button.Name)
                    {
                        Left = left,
                        Top = top
                    };
                    creatActionMenu.Show();
                }
            }
        }
    }
}