# Quicker

[![Merge Project](https://github.com/LJZ-Anonymity/Quicker/actions/workflows/CodeMerging.yml/badge.svg?branch=Quicker)](https://github.com/LJZ-Anonymity/Quicker/actions/workflows/CodeMerging.yml)
[![Version](https://img.shields.io/github/v/release/LJZ-Anonymity/Quicker)](https://github.com/LJZ-Anonymity/Quicker/releases)
[![最后更新](https://img.shields.io/github/last-commit/LJZ-Anonymity/Quicker)](https://github.com/LJZ-Anonymity/Quicker/commits)
[![开源协议](https://img.shields.io/badge/License-GNU%20AGPL%20v3.0-blue.svg)](https://github.com/LJZ-Anonymity/Quicker/blob/main/LICENSE)
[![代码行数](https://aschey.tech/tokei/github/LJZ-Anonymity/Quicker)](https://github.com/LJZ-Anonymity/Quicker)
[![主要语言](https://img.shields.io/github/languages/top/LJZ-Anonymity/Quicker)](https://github.com/LJZ-Anonymity/Quicker)

---

## 项目简介

这是一个基于WPF开发的学习项目，参考了Quicker软件的界面和功能，但代码完全独立实现。

该项目旨在提供一个高效、便捷的操作界面，帮助用户快速执行各种任务。

主要功能包括动作管理、应用状态管理、按钮管理等。

**本项目仅供学习和研究使用，不用于任何商业目的。**

- [扩展/插件仓库](https://github.com/LJZ-Anonymity/Extensions "查看Quicker扩展项目")
- [使用说明/文档](https://github.com/LJZ-Anonymity/Instructions "查看Quicker说明项目")

---

## 功能特点

- **高效便捷的操作界面**：直观的用户界面，帮助用户快速执行各种任务。
- **强大的动作管理**：支持创建、编辑和组织各种动作，提高工作效率。
- **灵活的扩展机制**：通过扩展模块增强应用功能，满足不同用户的需求。
- **按钮管理**：自定义和管理界面按钮。
- **外观导入/导出**：基于 ImageSharp 实现，支持将外观参数嵌入 PNG 图片并从图片中恢复。

---

## 声明

本项目为开源学习项目，作者非计算机专业，项目仍有很多地方需要优化。

项目仍在持续更新中，如有侵权，请[联系作者](#contact)删除。

**重要说明**：本项目是作者出于学习目的独立开发的，所有代码均为自行编写，与原版Quicker软件无任何代码关联。界面和功能设计参考了原版Quicker，但实现方式完全不同。

如有能力或想体验更多功能，请支持正版：https://getquicker.net/ 。

---

## 开源协议

本项目采用 [GNU AGPL v3.0](LICENSE) 协议开源。

**声明**：
- 本项目是作者出于学习目的，参照原版Quicker的界面和功能进行独立开发的
- 所有代码均为自行编写，与原版Quicker软件无任何代码关联
- 本项目与原版Quicker无直接关联，仅作为学习交流使用
- 不得将本项目用于任何商业用途
- 任何修改必须开源
- 必须保留原始版权声明和许可证声明

---

## 快速开始指南

克隆项目仓库：https://github.com/LJZ-Anonymity/Quicker.git

安装必要的依赖项，包括以下 NuGet 程序包：

| 程序包                      | 说明                              |
| :------------------------: | :------------------------------: |
| Hardcodet.NotifyIcon.Wpf   | 实现 WPF 应用的系统托盘图标功能      |
| SharpHook                  | 提供键盘和鼠标钩子功能               |
| Svg                        | 提供加载、解析和渲染 SVG 图像的功能   |
| System.Data.SQLite         | 提供 SQLite 数据库支持              |
| SixLabors.ImageSharp       | 实现 PNG 图片的元数据读写与外观导入导出 |
| WpfAnimatedGif             | GIF 动画播放支持，支持 Image 控件直接显示动图 |

打开项目解决方案文件（Quicker.sln），使用支持 WPF 的开发环境（如 Visual Studio）进行编译和运行。

---

## 贡献指南

如果你发现项目中有问题或有改进建议，可以通过以下方式参与贡献：

1. 提交 Issue：如果你发现问题或有功能建议，可以提交 Issue 详细描述问题或建议。

2. Fork 项目：Fork 项目仓库到你的 GitHub 账号。

3. 本地开发：在本地克隆你的 Fork 仓库，并进行开发。

4. 提交 Pull Request：完成开发后，提交 Pull Request 到原始仓库，等待审核。

---

## 贡献者版权说明

所有贡献者提交的代码将自动采用 GNU AGPL v3.0 协议开源。提交代码即视为同意本项目的开源协议和相关声明。

---

## 扩展开发

Quicker 支持通过创建扩展模块来增强应用功能。你可以通过以下步骤开发扩展模块：

1. 创建一个 WPF 类库项目。

2. 引用Quicker的扩展接口（Extend/IExtensionModule.cs）。

3. 实现扩展接口，开发你的扩展功能。

4. 将编译后的扩展 DLL 文件放置在 Quicker 的扩展目录中，应用将会在需要时加载。

更多详情请见[扩展项目](https://github.com/LJZ-Anonymity/Extensions "查看Quicker 扩展项目")。

---

## 致谢

感谢为该项目提供界面和功能参考的 [Quicker 软件](https://getquicker.net/ "访问 Quicker 官方网站")。

---

## 版权声明

本项目部分界面设计、图标及图片等资源参考或来源于正版 Quicker 软件，仅用于学习和非商业用途。

相关版权归原作者及 Quicker 官方所有。如有侵权请及时联系删除。

---

## 免责声明

本项目为非商业学习项目，未获得 Quicker 官方或其他第三方的商业授权。

**因使用本项目造成的任何后果，作者不承担任何责任。**

---

## <a id="contact"></a>联系作者

如需进一步了解项目或有任何问题，请通过以下方式联系作者：

- **GitHub Issues**：https://github.com/LJZ-Anonymity/Quicker/issues
- **邮箱**：331433038@qq.com

---

## 项目文件结构

<details>
<summary>详细结构</summary>

```
Quicker/
├─.gitattributes                                      # Git属性配置
├─.gitignore                                          # Git忽略文件配置
├─App.xaml                                            # 应用入口点
├─App.xaml.cs                                         # 应用入口点的代码后台
├─AssemblyInfo.cs                                     # 项目的属性和版本信息
├─Converters/                                         # XAML 绑定用的值转换器
│   ├─HighlightTextConverter.cs                       # 高亮文本转换器
│   ├─GridWidthConverter.cs                           # 网格宽度计算转换器
│   ├─GridHeightConverter.cs                          # 网格高度计算转换器
│   ├─BorderHeightConverter.cs                        # 边框高度转换器
│   ├─LighterColorConverter.cs                        # 亮色系转换器
│   ├─SmartHoverColorConverter.cs                     # 智能悬停色转换器
│   ├─FontWeightConverter.cs                          # 字体粗细与枚举转换器
│   ├─ButtonBackgroundConverter.cs                    # 按钮背景色转换器
│   ├─IntToCornerRadiusConverter.cs                   # int转圆角半径转换器
│   ├─BlurEffectConverter.cs                          # 模糊效果转换器
│   ├─PathToImageSourceConverter.cs                   # 路径转图片源转换器
│   ├─ThicknessConverter.cs                           # 边距厚度转换器
│   └─SearchHintVisibilityConverter.cs                # 搜索框提示可见性转换器，根据TextBox文本内容控制提示Label的显示/隐藏
├─Database/                                           # 数据库相关
│   ├─Core/                                            # 数据库核心操作
│   │   ├─SettingDatabase.cs                            # 设置数据库，管理应用配置和设置信息
│   │   ├─ButtonDatabase.cs                             # 按钮数据库，管理动作按钮的数据存储
│   │   └─ActionPageDatabase.cs                         # 动作页数据库，管理动作页和场景数据
│   └─Upgrade/                                         # 数据库升级相关
│       ├─IDatabaseUpgradeStep.cs                       # 升级步骤接口，定义数据库升级的标准接口
│       ├─DatabaseUpdateManager.cs                      # 升级调度主类，负责数据库版本检查和升级流程管理
│       └─Versions/                                     # 各版本升级实现类（每个版本一个类，便于维护和扩展）
│           └─Upgrade_2_3_0.cs                          # 2.2.0到2.3.0版本升级实现
├─Extend/                                             # 扩展文件夹
│   ├─IExtensionModule.cs                              # 实现扩展的接口
│   └─ModuleLoader.cs                                  # 扩展模块加载器
├─Helpers/                                            # 通用辅助类/附加属性
│   ├─TextBlockHelper.cs                               # 文本块辅助类
│   ├─VersionHelper.cs                                 # 版本号比较工具类，用于版本号比较和检查更新
│   ├─DataSizeHelper.cs                                # 数据大小换算工具类，用于字节、KB、MB、GB等单位转换
│   ├─ClipHelper.cs                                    # UI裁剪附加属性，用于为按钮和边框设置自定义裁剪（圆角）
│   └─ShortcutHelper.cs                                # 快捷键辅助类，提供快捷键字符串生成、友好显示、比对等功能
├─VersionInfo.json                                    # 版本信息
├─LICENSE                                             # 开源协议文件
├─Managers/                                           # 管理器文件夹
│   ├─ActionManager.cs                                 # 动作管理器
│   ├─AppManager.cs                                    # 应用管理器
│   ├─AppStateManager.cs                               # 应用状态管理器
│   ├─AppUpdateManager.cs                              # 应用更新管理器
│   ├─ButtonManager.cs                                 # 按钮管理器
│   ├─DatabaseUpdateManager.cs                         # 数据库更新管理器
│   ├─IconManager.cs                                   # 图标管理器
│   ├─SettingManager.cs                                # 设置管理器
│   ├─SingleInstanceManager .cs                        # 互斥锁管理器
│   ├─ToastManager.cs                                  # 消息管理器
│   └─WindowManager.cs                                 # 窗口管理器
├─Models/                                             # 数据模型
│   ├─ActionPageData.cs                                # 动作页数据模型
│   ├─ButtonData.cs                                    # 按钮数据模型
│   ├─SceneData.cs                                     # 场景数据模型
│   └─Settings/                                        # 设置相关数据模型
│       ├─Appearance.cs                                 # 外观设置数据模型
│       ├─Blacklist.cs                                  # 黑名单设置数据模型
│       ├─Convention.cs                                 # 常规设置数据模型
│       └─OpenMainWindow.cs                             # 打开主窗口条件数据模型
├─Properties/                                         # 项目属性文件夹
│   ├─Settings.Designer.cs                             # 自动生成的设置代码
│   ├─Settings.settings                                # 应用程序设置
│   └─PublishProfiles/                                 # 发布配置文件夹
│       ├─FolderProfile.pubxml                          # 文件夹发布配置文件
│       └─FolderProfile.pubxml.user                     # 文件夹发布配置用户文件
├─Quicker.csproj                                      # 项目的核心配置文件
├─Quicker.csproj.user                                 # 项目用户配置文件
├─Quicker.ico                                         # 应用图标
├─Quicker.sln                                         # 项目解决方案文件
├─README.md                                           # 项目说明文档
├─Resources/                                          # 资源文件夹
│   ├─Images/                                          # 图像资源文件夹
│   │   ├─AboutQuicker.png                              # 关于Quicker的图片
│   │   ├─ActionInformation.png                         # 动作信息的图片
│   │   ├─ActionPagesManager.jpg                        # 动作场景的图片
│   │   ├─Add.png                                       # 主面板添加动作的图片
│   │   ├─BasicSettingsButton.png                       # 基础设置按钮的图片
│   │   ├─BlackList.png                                 # 黑名单设置按钮的图片
│   │   ├─ClearButton.png                               # 下载窗口中清理按钮的图片
│   │   ├─CloseDownloadWindow.png                       # 关闭下载窗口的按钮图片
│   │   ├─CloseQuicker.png                              # 退出Quicker的图片
│   │   ├─CloseWindow.png                               # 关闭主面板的图片
│   │   ├─CommonSceneImage.png                          # 通用场景图片
│   │   ├─CopyAction.png                                # 复制动作按钮的图片
│   │   ├─DeleteImage.png                               # 删除动作按钮图像的图片
│   │   ├─DesktopSceneImage.png                         # 桌面场景的图片
│   │   ├─EditButton.png                                # 编辑动作的图片
│   │   ├─GlobalSceneImage.png                          # 全局场景图片
│   │   ├─Locked.png                                    # 锁住通用动作页的图片
│   │   ├─MessageImage.jpg                              # 消息弹窗的图片
│   │   ├─MoreSelection.png                             # 主面板更多选择的图片
│   │   ├─OpenFile.png                                  # 打开文件所在文件夹的图片
│   │   ├─OpenFileImage.png                             # 添加窗口中打开文件动作的图片
│   │   ├─OpenFolder.png                                # 打开文件夹的图片
│   │   ├─OpenMainWindow1.png                           # 菜单中打开主面板的图片
│   │   ├─OpenMainWindow2.png                           # 设置窗口中弹出面板按钮的图片
│   │   ├─OpenWebsiteImage.png                          # 添加窗口中打开网站动作的图片
│   │   ├─PasteAction.png                               # 粘贴动作按钮图片
│   │   ├─Pause.png                                     # 暂停Quicker的图片
│   │   ├─PinToDesktop.png                              # 订住主面板的图片
│   │   ├─Quicker_Enabled.png                           # Quicker运行中的图片
│   │   ├─Quicker_Disabled.png                          # Quicker暂停时的图片
│   │   ├─RestartQuicker.png                            # 重启Quicker的图片
│   │   ├─SelectLocalImage.png                          # 从本地文件选择动作按钮图像的图片
│   │   ├─SettingImage1.png                             # 菜单中打开设置窗口的按钮图片
│   │   ├─SettingImage2.png                             # 主面板打开设置面板的图片
│   │   ├─SettingWindow.png                             # 设置面板的图片
│   │   ├─StartApp.png                                  # 启动应用图标
│   │   ├─TaskbarSceneImage.png                         # 任务栏场景图片
│   │   ├─Target.png                                    # 目标图标
│   │   ├─UnLocked.png                                  # 不锁住通用动作页的图片
│   │   └─UnpinFromDesktop.png                          # 不订住Quicker的图片
│   └─Styles/                                         # 样式资源文件夹
│       ├─BorderStyles.xaml                            # 边框样式
│       ├─ButtonStyles.xaml                            # 按钮样式
│       |   └─ButtonStyles.xaml.cs
│       ├─CanvasStyles.xaml                            # 画布样式
│       ├─CheckBoxStyle.xaml                           # 勾选框样式
│       ├─ComboBoxStyle.xaml                           # 下拉框样式
│       |   └─ComboBoxStyle.xaml.cs
│       ├─GridStyles.xaml                              # 表格样式
│       ├─ImageStyles.xaml                             # 图片样式
│       ├─PanelStyles.xaml                             # 面板样式
│       ├─ScrollBarStyle.xaml                          # 滚动条样式
│       ├─SliderStyle.xaml                             # 滑动条样式
│       |   └─SliderStyle.xaml.cs
│       ├─TextBlockStyles.xaml                         # 文本块样式
│       ├─TextBoxStyles.xaml                           # 文本框样式
│       └─TooltipStyle.xaml                            # 提示框样式
├─UserControls/                                       # 自定义控件文件夹
│   ├─AddWindow/                                       # 添加窗口的自定义控件
│   │   ├─LoadExtension.xaml                            # 加载扩展界面
│   │   │   └─LoadExtension.xaml.cs
│   │   ├─OpenFile.xaml                                 # 打开文件界面
│   │   │   └─OpenFile.xaml.cs
│   │   └─OpenWebsite.xaml                              # 打开网站界面
│   │       └─OpenWebsite.xaml.cs
│   └─SettingWindow/                                   # 设置窗口的自定义控件
│       ├─Auxiliary_Functions/                          # 辅助功能控件文件夹
│       ├─BasicSettings/                                # 基础设置控件文件夹
│       │   ├─AboutQuickerGrid.xaml                      # 关于界面
│       │   │   └─AboutQuickerGrid.xaml.cs
│       │   ├─AppearanceGrid.xaml                        # 外观设置界面
│       │   │   └─AppearanceGrid.xaml.cs
│       │   ├─BlacklistGrid.xaml                         # 黑名单设置界面
│       │   │   └─BlacklistGrid.xaml.cs
│       │   ├─ConventionGrid.xaml                        # 常规设置界面
│       │   │   └─ConventionGrid.xaml.cs
│       │   ├─FunctionShortcutKeysGrid.xaml              # 快捷键设置界面
│       │   │   └─FunctionShortcutKeysGrid.xaml.cs
│       │   └─OpenMainWindowGrid.xaml                    # 打开主窗口设置界面
│       │       └─OpenMainWindowGrid.xaml.cs
│       ├─ColorPicker.xaml                              # 颜色选择器
│       │   └─ColorPicker.xaml.cs
│       └─Tools/                                        # 工具控件文件夹
|           └─ExtensionManagementGrid.xaml               # 扩展管理界面
|               └─ExtensionManagementGrid.xaml.cs
├─Windows/                                            # 界面文件夹
│   ├─AddWindows/                                     # 添加相关窗口（如添加场景、添加动作等）
│   │   ├─AddSceneWindow.xaml                         # 添加场景窗口
│   │   │   └─AddSceneWindow.xaml.cs
│   │   └─AddWindow.xaml                              # 添加动作窗口
│   │       └─AddWindow.xaml.cs
│   ├─FloatingWindows/                                # 悬浮窗口相关
│   │   ├─Windows/                                    # 悬浮窗口实现文件夹
│   │   │   ├─FloatingActionPageWindow.xaml           # 悬浮动作页窗口
│   │   │   │   └─FloatingActionPageWindow.xaml.cs
│   │   │   └─FloatingActionWindow.xaml               # 悬浮动作窗口
│   │   │       └─FloatingActionWindow.xaml.cs
│   │   └─ViewModels/                                 # 悬浮窗口ViewModel文件夹
│   │       ├─FloatingActionPageWindow.cs             # 悬浮动作页窗口ViewModel
│   │       └─FloatingActionWindow.cs                 # 悬浮动作窗口ViewModel
│   ├─EditWindows/                                     # 编辑窗口文件夹
│   │   ├─EditActionPageInfoWindow.xaml                 # 编辑动作页信息窗口
│   │   │   └─EditActionPageInfoWindow.xaml.cs
│   │   └─EditSceneWindow.xaml                          # 编辑场景窗口
│   │       └─EditSceneWindow.xaml.cs
│   ├─MainWindows/                                      # 主窗口文件夹
│   │   ├─ActionPageManageWindow.xaml                    # 动作页管理窗口
│   │   │   └─ActionPageManageWindow.xaml.cs
│   │   ├─FindAppsWindow.xaml                            # 查找应用窗口
│   │   │   └─FindAppsWindow.xaml.cs
│   │   ├─MainWindow/                                    # 主功能面板MVVM文件夹
│   │   │   ├─MainWindow.xaml                             # 主面板
│   │   │   │   └─MainWindow.xaml.cs
│   │   │   └─MainWindowViewModel.cs                      # 主面板ViewModel
│   │   ├─SearchWindow.xaml                              # 搜索窗口
│   │   │   └─SearchWindow.xaml.cs
│   │   ├─SelectImageWindow.xaml                         # 选择图片窗口
│   │   │   └─SelectImageWindow.xaml.cs
│   │   ├─SettingWindow.xaml                             #  设置窗口
│   │   │   └─SettingWindow.xaml.cs
│   │   └─UpdateWindow.xaml                              #  更新窗口
│   │       └─UpdateWindow.xaml.cs
│   ├─Menus/                                           # 菜单文件夹
│   │   ├─CreatActionMenu.xaml                          # 创建动作菜单界面
│   │   │   └─CreatActionMenu.xaml.cs
│   │   ├─CustomMenu.xaml                               # 自定义菜单界面
│   │   │   └─CustomMenu.xaml.cs
│   │   ├─EditSceneMenu.xaml                            # 编辑场景菜单界面
│   │   │   └─EditSceneMenu.xaml.cs
│   │   ├─OperationMenu.xaml                            # 操作菜单界面
│   │   │   └─OperationMenu.xaml.cs
│   │   ├─SelectActionPageMenu.xaml                     # 选择动作页菜单界面
│   │   │   └─SelectActionPageMenu.xaml.cs
│   │   └─SelectActionPageMenu.xaml.cs                  # 选择动作页菜单逻辑
│   └─ToolWindows/                                     # 工具窗口文件夹
│       ├─ActionInfoWindow.xaml                         # 动作信息窗口
│       │   └─ActionInfoWindow.xaml.cs
│       ├─DownloadWindow.xaml                           # 下载窗口
│       │   └─DownloadWindow.xaml.cs
│       ├─ImageCropWindow.xaml                          # 图片裁剪窗口
│       │   └─ImageCropWindow.xaml.cs
│       ├─LoadingWindow.xaml                            # 加载窗口
│       │   └─LoadingWindow.xaml.cs
│       ├─MessageWindow.xaml                            # 消息窗口
│       │   └─MessageWindow.xaml.cs
│       ├─SelectWindowWindow.xaml                       # 选择窗口
│       │   └─SelectWindowWindow.xaml.cs
│       └─ToastWindow.xaml                              # Toast提示窗口
│           └─ToastWindow.xaml.cs
```

</details>

---

<p align="center">
  ⭐️ 如果你觉得本项目有帮助，欢迎 Star 支持！也欢迎 Issue 和 PR 共同完善！
</p>