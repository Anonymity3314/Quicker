# 项目简介
这是由作者重构的 WPF 应用Quicker，主要功能包括动作管理、应用状态管理、按钮管理等。

该项目旨在提供一个高效、便捷的操作界面，帮助用户快速执行各种任务。

# 声明
本项目为开源项目，作者非计算机专业，项目仍有很多地方需要优化。

项目仍在持续更新中，如有侵权，请联系作者删除。

如有能力或想体验更多功能，请支持正版：https://getquicker.net/ 。

# NuGet 程序包

| 程序包                                 | 说明                              |
| :-----------------------------------: | :------------------------------: |
| Autoupdater.NET.Official              | 提供自动更新功能                    |
| DK.WshRuntime                         | 提供 Windows 脚本主机运行时功能      |
| Hardcodet.NotifyIcon.Wpf              | 实现 WPF 应用的系统托盘图标功能      |
| SharpHook                             | 提供键盘和鼠标钩子功能               |
| Svg                                   | 提供加载、解析和渲染 SVG 图像的功能   |
| System.Data.SQLite                    | 提供 SQLite 数据库支持              |

# 文件结构
```
Quicker/
│   App.xaml                                      # 应用入口点
│   App.xaml.cs                                   # 应用入口点的代码后台
│   AssemblyInfo.cs                               # 项目的属性和版本信息
│   installer.idproj                              # 安装程序项目文件
│   Quicker.csproj                                # 项目的核心配置文件
│   Quicker.png                                   # 应用图标
│   Quicker.sln                                   # 项目解决方案文件
│
├─Database/                                       # 数据库文件夹
│   ├─ActionPageDatabase.cs                       # 动作页数据库
│   ├─ButtonDatabase.cs                           # 按钮数据库
│   ├─SettingDatabase.cs                          # 设置数据库
│   └─TemporaryDatabase.cs                        # 临时数据库
│
├─Managers/                                       # 管理器文件夹
│   ├─ActionManager.cs                            # 动作管理器
│   ├─AppStateManager.cs                          # 应用状态管理器
│   ├─AppUpdateManager.cs                         # 应用更新管理器
│   ├─ButtonManager.cs                            # 按钮管理器
│   ├─DatabaseUpdateManager.cs                    # 数据库更新管理器
│   ├─IconManager.cs                              # 图标管理器
│   ├─SettingManager.cs                           # 设置管理器
│   ├─SingleInstanceManager.cs                    # 互斥锁管理器
│   ├─ToastManager.cs                             # 消息管理器
│   └─WindowManager.cs                            # 窗口管理器
│
├─Resources/                                      # 资源文件夹
│   ├─Images/                                     # 图像资源文件夹
│   │   ├─Icons/                                  # 图标资源文件夹
│   │   │   ├─AboutQuicker.png                    # 关于Quicker的图标
│   │   │   ├─ActionInformation.png               # 动作信息的图标
│   │   │   ├─ActionPagesManager.png              # 动作场景的图标
│   │   │   ├─Add.png                             # 主面板添加动作的图标
│   │   │   ├─BasicSettingButton.png              # 基础设置按钮的图标
│   │   │   ├─Book.png                            # 订住主面板的图标
│   │   │   ├─CloseQuicker.png                    # 退出Quicker的图标
│   │   │   ├─CloseWindow.png                     # 关闭主面板的图标
│   │   │   ├─DeleteImage.png                     # 删除动作按钮图像的图标
│   │   │   ├─Disbook.png                         # 不订住Quicker的图标
│   │   │   ├─EditButton.png                      # 编辑动作的图标
│   │   │   ├─Locked.png                          # 锁住通用动作页的图标
│   │   │   ├─MoreSelection.png                   # 主面板更多选择的图标
│   │   │   ├─OpenFile.png                        # 打开文件所在文件夹的图标
│   │   │   ├─OpenMainWindow1.png                 # 菜单中打开主面板的图标
│   │   │   ├─OpenMainWindow2.png                 # 设置窗口中弹出面板按钮的图标
│   │   │   ├─Pause.png                           # 暂停Quicker的图标
│   │   │   ├─Quicker1.png                        # Quicker运行中的图标
│   │   │   ├─Quicker2.png                        # Quicker暂停时的图标
│   │   │   ├─RestartQuicker.png                  # 重启Quicker的图标
│   │   │   ├─SelectLocalImage.png                # 从本地文件选择动作按钮图像的图标
│   │   │   ├─SettingImage1.png                   # 菜单中打开设置窗口的按钮图标
│   │   │   ├─SettingImage2.png                   # 主面板打开设置面板的图标
│   │   │   ├─SettingWindow.png                   # 设置面板的图标
│   │   │   └─UnLocked.png                        # 不锁住通用动作页的图标
│   │   │
│   │   └─SourseImages/                           # 图标的原图文件夹
│   │       ├─AboutQuicker.png                    # 关于Quicker的图标原图
│   │       ├─ActionInformation.png               # 动作信息的图标原图
│   │       ├─ActionPagesManager.jpg              # 动作场景的图标原图
│   │       ├─Add.png                             # 主面板添加动作的图标原图
│   │       ├─BasicSettingButton.png              # 基础设置按钮的图标原图
│   │       ├─Book.png                            # 订住主面板的图标原图
│   │       ├─CloseQuicker.png                    # 退出Quicker的图标原图
│   │       ├─CloseWindow.png                     # 关闭主面板的图标原图
│   │       ├─DeleteImage.png                     # 删除动作按钮图像的图标原图
│   │       ├─Disbook.png                         # 不订住Quicker的图标原图
│   │       ├─EditButton.png                      # 编辑动作的图标原图
│   │       ├─Locked.png                          # 锁住通用动作页的图标原图
│   │       ├─MoreSelection.png                   # 主面板更多选择的图标原图
│   │       ├─OpenFile.png                        # 打开文件所在文件夹的图标原图
│   │       ├─OpenMainWindow1.png                 # 菜单中打开主面板的图标原图
│   │       ├─OpenMainWindow2.png                 # 设置窗口中弹出面板按钮的图标原图
│   │       ├─Pause.png                           # 暂停Quicker的图标原图
│   │       ├─Quicker1.png                        # Quicker运行中的图标原图
│   │       ├─Quicker2.png                        # Quicker暂停时的图标原图
│   │       ├─RestartQuicker.png                  # 重启Quicker的图标原图
│   │       ├─SelectLocalImage.png                # 从本地文件选择动作按钮图像的图标原图
│   │       ├─SettingImage1.png                   # 菜单中打开设置窗口的按钮图标原图
│   │       ├─SettingImage2.png                   # 主面板打开设置面板的图标原图
│   │       ├─SettingWindow.png                   # 设置面板的图标原图
│   │       └─UnLocked.png                        # 不锁住通用动作页的图标原图
│   │
│   └─Styles/                                     # 样式资源文件夹
│       ├─ButtonStyles.xaml                       # 按钮样式
│       ├─CanvasStyles.xaml                       # 画布样式
│       ├─CheckBoxStyle.xaml                      # 勾选框样式
│       ├─ComboBoxStyle.xaml                      # 下拉框样式
│       ├─GridStyles.xaml                         # 表格样式
│       ├─ImageStyles.xaml                        # 图片样式
│       ├─ScrollBarStyle.xaml                     # 滚动条样式
│       ├─SliderStyle.xaml                        # 滑动条样式
│       ├─TextBlockStyles.xaml                    # 文本块样式
│       ├─TextBoxStyles.xaml                      # 文本框样式
│       └─TooltipStyle.xaml                       # 提示框样式
│
├─UserControls/                                   # 自定义控件文件夹
│   ├─AddWindow/                                  # 添加窗口的自定义控件文件夹
│   │   ├─OpenFile.xaml                           # 添加窗口中添加打开文件动作的界面
│   │   │   └─OpenFile.xaml.cs
│   │   └─OpenWebsite.xaml                        # 添加窗口中添加打开网站动作的界面
│   │       └─OpenWebsite.xaml.cs
│   └─SettingWindow/                              # 设置窗口的自定义控件文件夹
│       ├─AboutQuickerGrid.xaml                   # 设置窗口中关于Quicker界面
│       │   └─AboutQuickerGrid.xaml.cs
│       ├─AppearanceGrid.xaml                     # 设置窗口中外观设置界面
│       │   └─AppearanceGrid.xaml.cs
│       ├─BlacklistGrid.xaml                      # 设置窗口中黑名单设置界面
│       │   └─BlacklistGrid.xaml.cs
│       ├─ColorPicker.xaml                        # 颜色选择器控件
│       │   └─ColorPicker.xaml.cs
│       ├─ConventionGrid.xaml                     # 设置窗口中常规设置界面
│       │   └─ConventionGrid.xaml.cs
│       └─OpenMainWindowGrid.xaml                 # 设置窗口中弹出面板设置界面
│           └─OpenMainWindowGrid.xaml.cs
│
└─Windows/                                        # 界面文件夹
    ├─Forms/                                      # 窗口文件夹
    │   ├─ActionPageManageWindow.xaml             # 编辑动作页窗口
    │   │   └─ActionPageManageWindow.xaml.cs
    │   ├─AddWindow.xaml                          # 添加动作窗口
    │   │   └─AddWindow.xaml.cs
    │   ├─FindAppsWindow.xaml                     # 查找应用窗口
    │   │   └─FindAppsWindow.xaml.cs
    │   ├─MainWindow.xaml                         # 功能面板
    │   │   └─MainWindow.xaml.cs
    │   ├─SelectImageWindow.xaml                  # 选择图片窗口
    │   │   └─SelectImageWindow.xaml.cs
    │   ├─SettingWindow.xaml                      # 设置窗口
    │   │    └─SettingWindow.xaml.cs
    │   └─UpdateWindow.xaml                       # 更新窗口
    │        └─UpdateWindow.xaml.cs
    │
    └─Menus/                                      # 菜单文件夹
        ├─ActionInformationWindow.xaml            # 动作信息菜单
        │   └─ActionInformationWindow.xaml.cs
        ├─CreatActionMenu.xaml                    # 创建动作菜单
        │   └─CreatActionMenu.xaml.cs
        ├─CustomMenu.xaml                         # 用户菜单
        │   └─CustomMenu.xaml.cs
        ├─DownloadWindow.xaml                     # 下载窗口
        │   └─DownloadWindow.xaml.cs
        ├─EditActionPageInfoWindow.xaml           # 编译动作页信息窗口
        │   └─EditActionPageInfoWindow.xaml.cs
        ├─EditSceneWindow.xaml                    # 编译场景窗口
        │   └─EditSceneWindow.xaml.cs
        ├─LoadingWindow.xaml                      # 加载弹窗
        │   └─LoadingWindow.xaml.cs
        ├─OperationMenu.xaml                      # 动作按钮操作菜单
        │   └─OperationMenu.xaml.cs
        ├─SelectActionPageMenu.xaml               # 选择动作页菜单
        │    └─SelectActionPageMenu.xaml.cs
        └─ToastWindow.xaml                        # 消息弹窗
             └─ToastWindow.xaml.cs
```

# 快速开始指南
克隆项目仓库：https://github.com/Anonymity3314/Quicker.git

安装必要的依赖项，包括上述列出的 NuGet 程序包。

打开项目解决方案文件（Quicker.sln），使用支持 WPF 的开发环境（如 Visual Studio）进行编译和运行。

# 贡献指南
如果你发现项目中有问题或有改进建议，可以通过提交 Issue 的方式告知作者。

你可以 Fork 项目仓库，在本地进行开发后，通过 Pull Request 的方式提交你的代码贡献。

# 致谢
感谢为该项目提供参考的 [Quicker 软件](https://getquicker.net/ "访问 Quicker 官方网站")。