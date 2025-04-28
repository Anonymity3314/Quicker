# 声明

由于正版的免费版图标提取功能受限，作者重构了一个WPF应用Quicker

有能力或想体验更多功能请支持正版：https://getquicker.net/

作者非计算机专业，项目很多地方待优化

项目仍在更新中

侵权必删

## 文件结构
```
Quicker/
│   App.xaml
│   App.xaml.cs
│   AssemblyInfo.cs
│   installer.idproj
│   Quicker.csproj
│   Quicker.csproj.user
│   Quicker.ico
│   Quicker.sln
│
├─Database/                                       # 数据库
│   ├─ButtonDatabase.cs                          # 按钮数据库
│   └─SettingDatabase.cs                         # 设置数据库
│
├─Managers/                                       # 管理器
│   ├─ButtonManager.cs                           # 按钮管理器
│   ├─IconManager.cs                             # 图标管理器
│   ├─SettingManager.cs                          # 设置管理器
│   └─WindowManager.cs                           # 窗口管理器
│
├─Resources/                                      # 资源
│   ├─Images/                                    # 图像资源
│   │   ├─Icons/                                # 图标资源
│   │   │   ├─AboutQuicker.ico                 # 关于Quicker的图标
│   │   │   ├─ActionInformation.ico            # 动作信息的图标
│   │   │   ├─ActionPagesManager.ico           # 动作场景的图标
│   │   │   ├─Add.ico                          # 主面板添加动作的图标
│   │   │   ├─BasicSettingButton.ico           # 基础设置按钮的图标
│   │   │   ├─Book.ico                         # 订住主面板的图标
│   │   │   ├─CloseQuicker.ico                 # 退出Quicker的图标
│   │   │   ├─CloseWindow.ico                  # 关闭主面板的图标
│   │   │   ├─DeleteImage.ico                  # 删除动作按钮图像的图标
│   │   │   ├─Disbook.ico                      # 不订住Quicker的图标
│   │   │   ├─EditButton.ico                   # 编辑动作的图标
│   │   │   ├─Locked.ico                       # 锁住通用动作页的图标
│   │   │   ├─MoreSelection.ico                # 主面板更多选择的图标
│   │   │   ├─OpenFile.ico                     # 打开文件所在文件夹的图标
│   │   │   ├─OpenMainWindow1.ico              # 菜单中打开主面板的图标
│   │   │   ├─OpenMainWindow2.ico              # 设置窗口中弹出面板按钮的图标
│   │   │   ├─Pause.ico                        # 暂停Quicker的图标
│   │   │   ├─Quicker1.ico                     # Quicker运行中的图标
│   │   │   ├─Quicker2.ico                     # Quicker暂停时的图标
│   │   │   ├─RestartQuicker.ico               # 重启Quicker的图标
│   │   │   ├─SelectLocalImage.ico             # 从本地文件选择动作按钮图像的图标
│   │   │   ├─SettingImage1.ico                # 菜单中打开设置窗口的按钮图标
│   │   │   ├─SettingImage2.ico                # 主面板打开设置面板的图标
│   │   │   ├─SettingWindow.ico                # 设置面板的图标
│   │   │   └─UnLocked.ico                     # 不锁住通用动作页的图标
│   │   │
│   │   └─SourseImages/                         # 图标的原图
│   │       ├─AboutQuicker.png                  # 关于Quicker的图标原图
│   │       ├─ActionInformation.png             # 动作信息的图标原图
│   │       ├─ActionPagesManager.jpg            # 动作场景的图标原图
│   │       ├─Add.png                           # 主面板添加动作的图标原图
│   │       ├─BasicSettingButton.png            # 基础设置按钮的图标原图
│   │       ├─Book.png                          # 订住主面板的图标原图
│   │       ├─CloseQuicker.png                  # 退出Quicker的图标原图
│   │       ├─CloseWindow.png                   # 关闭主面板的图标原图
│   │       ├─DeleteImage.png                   # 删除动作按钮图像的图标原图
│   │       ├─Disbook.png                       # 不订住Quicker的图标原图
│   │       ├─EditButton.png                    # 编辑动作的图标原图
│   │       ├─Locked.png                        # 锁住通用动作页的图标原图
│   │       ├─MoreSelection.png                 # 主面板更多选择的图标原图
│   │       ├─OpenFile.png                      # 打开文件所在文件夹的图标原图
│   │       ├─OpenMainWindow1.png               # 菜单中打开主面板的图标原图
│   │       ├─OpenMainWindow2.png               # 设置窗口中弹出面板按钮的图标原图
│   │       ├─Pause.png                         # 暂停Quicker的图标原图
│   │       ├─Quicker1.png                      # Quicker运行中的图标原图
│   │       ├─Quicker2.png                      # Quicker暂停时的图标原图
│   │       ├─RestartQuicker.png                # 重启Quicker的图标原图
│   │       ├─SelectLocalImage.png              # 从本地文件选择动作按钮图像的图标原图
│   │       ├─SettingImage1.png                 # 菜单中打开设置窗口的按钮图标原图
│   │       ├─SettingImage2.png                 # 主面板打开设置面板的图标原图
│   │       ├─SettingWindow.png                 # 设置面板的图标原图
│   │       └─UnLocked.png                      # 不锁住通用动作页的图标原图
│   │
│   └─Styles/                                    # 样式资源
│       ├─ButtonStyles.xaml                      # 按钮样式
│       ├─CheckBoxStyle.xaml                     # 勾选框样式
│       ├─ComboBoxStyle.xaml                     # 下拉框样式
│       ├─ScrollBarStyle.xaml                    # 滚动条样式
│       ├─SliderStyle.xaml                       # 滑动条样式
│       ├─TextBoxStyle.xaml                      # 文本框样式
│       └─TooltipStyle.xaml                      # 提示框样式
│
├─Setup                                           # 安装Quicker
│
├─UserControls/                                   # 自定义控件
│   ├─AboutQuickerGrid.xaml                      # 设置窗口中关于Quicker界面
│   │   └─AboutQuickerGrid.xaml.cs
│   ├─AppearanceGrid.xaml                        # 设置窗口中外观设置界面
│   │   └─AppearanceGrid.xaml.cs
│   ├─BlacklistGrid.xaml                         # 设置窗口中黑名单设置界面
│   │   └─BlacklistGrid.xaml.cs
│   ├─ColorPicker.xaml                           # 颜色选择器控件
│   │   └─ColorPicker.xaml.cs
│   ├─ConventionGrid.xaml                        # 设置窗口中常规设置界面
│   │   └─ConventionGrid.xaml.cs
│   └─OpenMainWindowGrid.xaml                    # 设置窗口中弹出面板设置界面
│        └─OpenMainWindowGrid.xaml.cs
│
└─Windows/                                        # 界面
    ├─Forms/                                      # 窗口
    │   ├─ActionPageManageWindow.xaml            #编辑动作页窗口
    │   │   └─ActionPageManageWindow.xaml.cs
    │   ├─AddWindow.xaml                         # 添加动作窗口
    │   │   └─AddWindow.xaml.cs
    │   ├─FindAppsWindow.xaml                    # 查找应用窗口
    │   │   └─FindAppsWindow.xaml.cs
    │   ├─MainWindow.xaml                        # 主面板
    │   │   └─MainWindow.xaml.cs
    │   ├─SelectImageWindow.xaml                 # 选择图片窗口
    │   │   └─SelectImageWindow.xaml.cs
    │   └─SettingWindow.xaml                     # 设置窗口
    │        └─SettingWindow.xaml.cs
    │
    └─Menus/                                      # 菜单
        ├─ActionInformationShower.xaml            # 动作信息菜单
        │   └─ActionInformationShower.xaml.cs
        ├─CreatActionMenu.xaml                    # 创建动作菜单
        │   └─CreatActionMenu.xaml.cs
        ├─CustomMenu.xaml                         # 用户菜单
        │   └─CustomMenu.xaml.cs
        ├─LoadingWindow.xaml                      # 加载弹窗
        │   └─LoadingWindow.xaml.cs
        ├─OperationMenu.xaml                      # 动作按钮操作菜单
        │   └─OperationMenu.xaml.cs
        └─SelectActionPageMenu.xaml               # 选择动作页菜单
             └─SelectActionPageMenu.xaml.cs
```