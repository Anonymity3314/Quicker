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

```
Quicker/
├─.gitattributes                                      # Git属性配置
├─.gitignore                                          # Git忽略文件配置
├─App.xaml                                            # 应用入口点
├─App.xaml.cs                                         # 应用入口点的代码后台
├─AssemblyInfo.cs                                     # 项目的属性和版本信息
├─Converters/                                         # WPF XAML 绑定用的值转换器
│   ├─BlurEffectConverter.cs
│   ├─DoubleSubtractConverter.cs
│   ├─FontWeightConverter.cs
│   ├─GridHeightConverter.cs
│   ├─GridWidthConverter.cs
│   ├─PathToImageSourceConverter.cs
│   ├─PreviewBorderHeightConverter.cs
│   └─ThicknessConverter.cs
├─Database/                                           # 数据库文件夹
│   ├─ActionPageDatabase.cs                           # 动作页数据库
│   ├─ButtonDatabase.cs                               # 按钮数据库
│   └─SettingDatabase.cs                              # 设置数据库
├─Extend/                                             # 扩展文件夹
│   ├─IExtensionModule.cs                             # 实现扩展的接口
│   └─ModuleLoader.cs                                 # 扩展模块加载器
├─Helpers/                                            # 通用辅助类/附加属性
│   ├─ClipHelper.cs                                   # UI裁剪附加属性
│   └─DataSizeHelper.cs                               # 数据大小换算工具类
├─InfoData/                                           # 信息数据文件夹
│   ├─Extensions.json                                 # 扩展信息
│   ├─UpdateHistory.txt                               # 更新历史
│   └─VersionInfo.json                                # 版本信息
├─LICENSE                                             # 开源协议文件
├─Managers/                                           # 管理器文件夹
│   ├─ActionManager.cs                                # 动作管理器
│   ├─AppManager.cs                                   # 应用管理器
│   ├─AppStateManager.cs                              # 应用状态管理器
│   ├─AppUpdateManager.cs                             # 应用更新管理器
│   ├─ButtonManager.cs                                # 按钮管理器
│   ├─DatabaseUpdateManager.cs                        # 数据库更新管理器
│   ├─IconManager.cs                                  # 图标管理器
│   ├─SettingManager.cs                               # 设置管理器
│   ├─SingleInstanceManager .cs                       # 互斥锁管理器
│   ├─ToastManager.cs                                 # 消息管理器
│   └─WindowManager.cs                                # 窗口管理器
├─Models/                                             # 数据模型
│   ├─ActionPageData.cs                               # 动作页数据模型
│   ├─ButtonData.cs                                   # 按钮数据模型
│   ├─SceneData.cs                                    # 场景数据模型
│   └─Settings/                                       # 设置相关数据模型
│       ├─Appearance.cs                               # 外观设置数据模型
│       ├─Blacklist.cs                                # 黑名单设置数据模型
│       ├─Convention.cs                               # 常规设置数据模型
│       └─OpenMainWindow.cs                           # 打开主窗口条件数据模型
├─obj/                                                # 编译中间文件夹
├─Properties/                                         # 项目属性文件夹
│   ├─Settings.Designer.cs                            # 自动生成的设置代码
│   ├─Settings.settings                               # 应用程序设置
│   └─PublishProfiles/                                # 发布配置文件夹
├─Quicker.csproj                                      # 项目的核心配置文件
├─Quicker.csproj.user                                 # 项目用户配置文件
├─Quicker.ico                                         # 应用图标
├─Quicker.sln                                         # 项目解决方案文件
├─README.md                                           # 项目说明文档
├─Resources/                                          # 资源文件夹
│   ├─Images/                                         # 图像资源文件夹
│   │   ├─AboutQuicker.png                            # 关于Quicker的图片
│   │   ├─ActionInformation.png                       # 动作信息的图片
│   │   ├─ActionPagesManager.jpg                      # 动作场景的图片
│   │   ├─Add.png                                     # 主面板添加动作的图片
│   │   ├─BasicSettingsButton.png                     # 基础设置按钮的图片
│   │   ├─BlackList.png                               # 黑名单设置按钮的图片
│   │   ├─ClearButton.png                             # 下载窗口中清理按钮的图片
│   │   ├─CloseDownloadWindow.png                     # 关闭下载窗口的按钮图片
│   │   ├─CloseQuicker.png                            # 退出Quicker的图片
│   │   ├─CloseWindow.png                             # 关闭主面板的图片
│   │   ├─CopyAction.png                              # 复制动作按钮的图片
│   │   ├─DeleteImage.png                             # 删除动作按钮图像的图片
│   │   ├─DesktopSceneImage.png                       # 桌面场景的图片
│   │   ├─EditButton.png                              # 编辑动作的图片
│   │   ├─Locked.png                                  # 锁住通用动作页的图片
│   │   ├─MessageImage.jpg                            # 消息弹窗的图片
│   │   ├─MoreSelection.png                           # 主面板更多选择的图片
│   │   ├─OpenFile.png                                # 打开文件所在文件夹的图片
│   │   ├─OpenFileImage.png                           # 添加窗口中打开文件动作的图片
│   │   ├─OpenMainWindow1.png                         # 菜单中打开主面板的图片
│   │   ├─OpenMainWindow2.png                         # 设置窗口中弹出面板按钮的图片
│   │   ├─OpenWebsiteImage.png                        # 添加窗口中打开网站动作的图片
│   │   ├─Pause.png                                   # 暂停Quicker的图片
│   │   ├─PinToDesktop.png                            # 订住主面板的图片
│   │   ├─Quicker1.png                                # Quicker运行中的图片
│   │   ├─Quicker2.png                                # Quicker暂停时的图片
│   │   ├─RestartQuicker.png                          # 重启Quicker的图片
│   │   ├─SelectLocalImage.png                        # 从本地文件选择动作按钮图像的图片
│   │   ├─SettingImage1.png                           # 菜单中打开设置窗口的按钮图片
│   │   ├─SettingImage2.png                           # 主面板打开设置面板的图片
│   │   ├─SettingWindow.png                           # 设置面板的图片
│   │   ├─UnLocked.png                                # 不锁住通用动作页的图片
│   │   └─UnpinFromDesktop.png                        # 不订住Quicker的图片
│   └─Styles/                                         # 样式资源文件夹
│       ├─BorderStyles.xaml                            # 边框样式
│       ├─ButtonStyles.xaml                            # 按钮样式
│       ├─ButtonStyles.xaml.cs                         # 按钮样式后台
│       ├─CanvasStyles.xaml                            # 画布样式
│       ├─CheckBoxStyle.xaml                           # 勾选框样式
│       ├─ComboBoxStyle.xaml                           # 下拉框样式
│       ├─ComboBoxStyle.xaml.cs                        # 下拉框样式后台
│       ├─GridStyles.xaml                              # 表格样式
│       ├─ImageStyles.xaml                             # 图片样式
│       ├─PanelStyles.xaml                             # 面板样式
│       ├─ScrollBarStyle.xaml                          # 滚动条样式
│       ├─SliderStyle.xaml                             # 滑动条样式
│       ├─TextBlockStyles.xaml                         # 文本块样式
│       ├─TextBoxStyles.xaml                           # 文本框样式
│       └─TooltipStyle.xaml                            # 提示框样式
├─UserControls/                                       # 自定义控件文件夹
│   ├─AddWindow/                                      # 添加窗口的自定义控件
│   │   ├─LoadExtension.xaml
│   │   ├─LoadExtension.xaml.cs
│   │   ├─OpenFile.xaml
│   │   ├─OpenFile.xaml.cs
│   │   ├─OpenWebsite.xaml
│   │   └─OpenWebsite.xaml.cs
│   └─SettingWindow/                                  # 设置窗口的自定义控件
│       ├─Auxiliary_Functions/                        # 辅助功能控件文件夹
│       ├─BasicSettings/                              # 基础设置控件文件夹
│       │   ├─AboutQuickerGrid.xaml
│       │   ├─AboutQuickerGrid.xaml.cs
│       │   ├─AppearanceGrid.xaml
│       │   ├─AppearanceGrid.xaml.cs
│       │   ├─BlacklistGrid.xaml
│       │   ├─BlacklistGrid.xaml.cs
│       │   ├─ConventionGrid.xaml
│       │   ├─ConventionGrid.xaml.cs
│       │   ├─FunctionShortcutKeysGrid.xaml
│       │   ├─FunctionShortcutKeysGrid.xaml.cs
│       │   ├─OpenMainWindowGrid.xaml
│       │   └─OpenMainWindowGrid.xaml.cs
│       ├─ColorPicker.xaml
│       ├─ColorPicker.xaml.cs
│       └─Tools/                                      # 工具控件文件夹
├─Windows/                                            # 界面文件夹
│   ├─EditWindows/                                    # 编辑窗口文件夹
│   │   ├─EditActionPageInfoWindow.xaml
│   │   ├─EditActionPageInfoWindow.xaml.cs
│   │   ├─EditSceneWindow.xaml
│   │   └─EditSceneWindow.xaml.cs
│   ├─MainWindows/                                    # 主窗口文件夹
│   │   ├─ActionPageManageWindow.xaml
│   │   ├─ActionPageManageWindow.xaml.cs
│   │   ├─AddWindow.xaml
│   │   ├─AddWindow.xaml.cs
│   │   ├─FindAppsWindow.xaml
│   │   ├─FindAppsWindow.xaml.cs
│   │   ├─MainWindow/                                 # 主功能面板MVVM文件夹
│   │   │   ├─MainWindow.xaml
│   │   │   ├─MainWindow.xaml.cs
│   │   │   └─MainWindowViewModel.cs
│   │   ├─SelectImageWindow.xaml
│   │   ├─SelectImageWindow.xaml.cs
│   │   ├─SettingWindow.xaml
│   │   ├─SettingWindow.xaml.cs
│   │   ├─UpdateWindow.xaml
│   │   └─UpdateWindow.xaml.cs
│   ├─Menus/                                          # 菜单文件夹
│   │   ├─CreatActionMenu.xaml
│   │   ├─CreatActionMenu.xaml.cs
│   │   ├─CustomMenu.xaml
│   │   ├─CustomMenu.xaml.cs
│   │   ├─OperationMenu.xaml
│   │   ├─OperationMenu.xaml.cs
│   │   ├─SelectActionPageMenu.xaml
│   │   └─SelectActionPageMenu.xaml.cs
│   └─ToolWindows/                                    # 工具窗口文件夹
│       ├─ActionInformationWindow.xaml
│       ├─ActionInformationWindow.xaml.cs
│       ├─DownloadWindow.xaml
│       ├─DownloadWindow.xaml.cs
│       ├─ImageCropWindow.xaml
│       ├─ImageCropWindow.xaml.cs
│       ├─LoadingWindow.xaml
│       ├─LoadingWindow.xaml.cs
│       ├─MessageWindow.xaml
│       ├─MessageWindow.xaml.cs
│       ├─SelectWindowWindow.xaml
│       ├─SelectWindowWindow.xaml.cs
│       ├─ToastWindow.xaml
│       └─ToastWindow.xaml.cs
```

---

<p align="center">
  ⭐️ 如果你觉得本项目有帮助，欢迎 Star 支持！也欢迎 Issue 和 PR 共同完善！
</p>