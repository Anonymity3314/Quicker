# 声明

由于正版的免费版图标提取功能受限，作者重构了一个WPF应用Quicker

有能力或想体验更多功能请支持正版：https://getquicker.net/

作者非计算机专业，项目很多地方待优化

项目仍在更新中

侵权必删

## 文件结构
Quicker
│  App.xaml
│  App.xaml.cs
│  AssemblyInfo.cs
│  file_tree.txt
│  installer.idproj
│  Quicker.csproj
│  Quicker.csproj.user
│  Quicker.ico
│  Quicker.sln
│  README.md
│
├─Database
│  ├─ButtonDatabase.cs
│  └─SettingDatabase.cs
│
├─Managers
│  ├─ButtonManager.cs
│  ├─IconManager.cs
│  ├─SettingManager.cs
│  └─WindowManager.cs
│
├─Resources
│  ├─Images
│  │  ├─Icons
│  │  │  ├─AboutQuicker.ico
│  │  │  ├─ActionInformation.ico
│  │  │  ├─ActionPagesManager.ico
│  │  │  ├─Add.ico
│  │  │  ├─BasicSettingButton.ico
│  │  │  ├─Book.ico
│  │  │  ├─CloseQuicker.ico
│  │  │  ├─CloseWindow.ico
│  │  │  ├─DeleteImage.ico
│  │  │  ├─Disbook.ico
│  │  │  ├─EditButton.ico
│  │  │  ├─Locked.ico
│  │  │  ├─MoreSelection.ico
│  │  │  ├─OpenFile.ico
│  │  │  ├─OpenMainWindow1.ico
│  │  │  ├─OpenMainWindow2.ico
│  │  │  ├─Pause.ico
│  │  │  ├─Quicker1.ico
│  │  │  ├─Quicker2.ico
│  │  │  ├─RestartQuicker.ico
│  │  │  ├─SelectLocalImage.ico
│  │  │  ├─SettingImage1.ico
│  │  │  ├─SettingImage2.ico
│  │  │  ├─SettingWindow.ico
│  │  │  └─UnLocked.ico
│  │  │
│  │  └─SourseImages
│  │      ├─AboutQuicker.png
│  │      ├─ActionInformation.png
│  │      ├─ActionPagesManager.jpg
│  │      ├─Add.png
│  │      ├─BasicSettingButton.png
│  │      ├─Book.png
│  │      ├─CloseQuicker.png
│  │      ├─CloseWindow.png
│  │      ├─DeleteImage.png
│  │      ├─Disbook.png
│  │      ├─EditButton.png
│  │      ├─Locked.png
│  │      ├─MoreSelection.png
│  │      ├─OpenFile.png
│  │      ├─OpenMainWindow1.png
│  │      ├─OpenMainWindow2.png
│  │      ├─Pause.png
│  │      ├─Quicker1.png
│  │      ├─Quicker2.png
│  │      ├─RestartQuicker.png
│  │      ├─SelectLocalImage.png
│  │      ├─SettingImage1.png
│  │      ├─SettingImage2.png
│  │      ├─SettingWindow.png
│  │      └─UnLocked.png
│  │
│  └─Styles
│      ├─ButtonStyles.xaml
│      ├─CheckBoxStyle.xaml
│      ├─ComboBoxStyle.xaml
│      ├─ScrollBarStyle.xaml
│      ├─SliderStyle.xaml
│      ├─TextBoxStyle.xaml
│      └─TooltipStyle.xaml
│
├─Setup
│
├─UserControls
│  ├─AboutQuickerGrid.xaml
│  │  AboutQuickerGrid.xaml.cs
│  ├─AppearanceGrid.xaml
│  │  AppearanceGrid.xaml.cs
│  ├─BlacklistGrid.xaml
│  │  BlacklistGrid.xaml.cs
│  ├─ColorPicker.xaml
│  │  ColorPicker.xaml.cs
│  ├─ConventionGrid.xaml
│  │  ConventionGrid.xaml.cs
│  └─OpenMainWindowGrid.xaml
│      └─OpenMainWindowGrid.xaml.cs
│
└─Windows
    ├─Forms
    │  ├─ActionPageManageWindow.xaml
    │  │  └─ActionPageManageWindow.xaml.cs
    │  ├─AddWindow.xaml
    │  │  └─AddWindow.xaml.cs
    │  ├─FindAppsWindow.xaml
    │  │  └─FindAppsWindow.xaml.cs
    │  ├─MainWindow.xaml
    │  │  └─MainWindow.xaml.cs
    │  ├─SelectImageWindow.xaml
    │  │  └─SelectImageWindow.xaml.cs
    │  └─SettingWindow.xaml
    │      └─SettingWindow.xaml.cs
    │
    └─Menus
        ├─ActionInformationShower.xaml
            └─ActionInformationShower.xaml.cs
        ├─CreatActionMenu.xaml
            └─CreatActionMenu.xaml.cs
        ├─CustomMenu.xaml
            └─CustomMenu.xaml.cs
        ├─LoadingWindow.xaml
            └─LoadingWindow.xaml.cs
        ├─OperationMenu.xaml
            └─OperationMenu.xaml.cs
        └─SelectActionPageMenu.xaml
            └─SelectActionPageMenu.xaml.cs