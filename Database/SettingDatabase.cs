using System.Collections.Generic;
using Quicker.Models.Settings;
using System.Data.SQLite;
using System.IO;

// SQLite数据库操作类
namespace Quicker.Database
{
    public static class SettingDatabase
    {
        // 配置项默认说明：
        /* [Convention] 常规
         * Version: 软件的当前版本号，默认为程序版本。
         * AutoStart: 是否开机自启，默认为false（关闭）。
         * ShowNotification: 是否显示系统通知，默认为true（开启）。
         * ShowAddImage: 是否显示添加图片选项，默认为true（开启）。
         * HideTooltip: 是否隐藏工具提示，默认为false（不隐藏）。
         * LongPressThreshold: 长按判定阈值（毫秒），默认为300ms。
         * MouseMovePixels: 鼠标移动触发距离（像素），默认为50px。
         * LoopPageFlipping: 是否循环翻页，默认为true（开启）。
         * RememberLastPage: 是否记住设置窗口中最后打开的页面，默认为false（不记住）。
         * LastPage: 最后打开的页面ID，默认为11。
         * EnableMemoryOptimization: 是否启用内存优化，默认为true（开启）。
         */
        /* [OpenMainWindow] 弹出面板
         * OpenMainWindowByMiddleMouseClick: 按中键点击打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByX1MouseClick: 按X1键点击打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByX2MouseClick: 按X2键点击打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByCtrl_MiddleMouseClick: 按Ctrl+中键点击打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByCtrl_RightMouseClick: 按Ctrl+右键点击打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByMiddleMouseClickLonger: 中键长按打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByRightMouseClickLonger: 右键长按打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByRightMouseClick_Move: 右键移动打开设置窗口，默认为false（关闭）。
         * OpenMainWindowByCtrl: 按Ctrl打开设置窗口，默认为true（开启）。
         * WindowStartupLocation: 设置窗口启动位置，默认为2（可能代表屏幕中央或其他位置）。
         */
        /* [Blacklist] 黑名单
         * IsFullScreenDisabled: 是否禁用全屏功能，默认为false（不禁用）。
         * IsBlacklistEnabledForExtendedHotkey: 扩展快捷键是否启用黑名单，默认为false（不启用）。
         */
        /* [BlacklistApplication] 黑名单列表
         * ApplicationName: 黑名单列表显示的文字，可以是文件夹路径，也可以是应用程序名称。
         * ProcessName: 确切的应用程序进程名称，应用程序的可执行文件名称。
         * IsInBlacklist: 此项用于标识该应用程序是否在黑名单中。
         * IsFolder: 此项用于标识ApplicationName是否为文件夹路径。
         */
        /* [Appearance] 外观
         * ButtonSize: 按钮大小，默认为77.6。
         * ButtonGap: 按钮间隙，默认为0.2。
         * BorderWidth: 边框宽度，默认为0.0。
         * ButtonCornerRadius: 按钮圆角，默认为0.0。
         *
         * BackgroundColor: 背景颜色，默认为#FFF3F3F3。
         * BorderColor：边框颜色，默认为#FFD3D3D3。
         * ToolbarColor: 工具栏颜色，默认为#00F3F3F3。
         * ToolbarIconColor: 工具栏图标颜色，默认为#FFA1A1A1。
         * ActionButtonColor: 动作按钮颜色，默认为#FFFFFFFF。
         * ActionButtonMouseOverColor: 动作按钮鼠标悬停颜色，默认为#FFBEE6FD。
         * BlankButtonColor: 空白按钮颜色，默认为#FFF3F3F3。
         * BlankButtonMouseOverColor: 空白按钮鼠标悬停颜色，默认为#FFEAEAEA。
         * ButtonTextColor: 按钮文字颜色，默认为#FF000000。
         * ActionIconColor: 动作图标颜色，默认为#FF696969。
         * TriggerKeyTextColor: 触发键文字颜色，默认为#D0FF8C00。
         * OtherIconColor: 其他位置图标颜色，默认为#FF666666。
         *
         * Font1: 字体1，默认为-1。
         * Font2: 字体2，默认为-1。
         * FontSize: 字体大小，默认为12。
         * FontWeight: 字体粗细，默认为400。
         *
         * BackgroundImagePath: 背景图片路径，默认为空。
         * BackgroundImageOpacity: 背景图片不透明度，默认为1.0。
         *
         * Blur: 模糊模式，默认为0。
         * Win11CornerRadius: Win11圆角模式，默认为0。
         *
         * AutoHideTitleBar: 自动缩小动作名称文字，默认为false。
         * ShowActionButtonMouseOver: 鼠标悬浮在动作按钮上时，放大显示按钮，默认为false。
         * HideActionNameAfterIcon: 设置动作图标后隐藏动作名称，默认为false。
         * ShowActionIconShadow: 动作图标显示阴影，默认为false。
         * EnablePreview: 开启预览功能，默认为false。
         */

        // 数据库连接
        private const string db1 = "Data Source=C:\\Users\\LENOVO\\AppData\\Roaming\\Anonymity\\Quicker\\Database\\Setting.db;Pooling=true;Max Pool Size=100;Journal Mode=Wal;";
        private readonly static ButtonDatabase db2 = new(); // 按钮数据库
        public const string currentVersion = "2.2.0"; // 当前版本号

        static SettingDatabase()
        {
            string dbFolder = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\Database"; // 获取数据库文件夹路径
            string dbFilePath = Path.Combine(dbFolder, "Setting.db"); // 设置数据库文件路径
            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder); // 如果"Database"文件夹不存在，则创建它
            if (!File.Exists(dbFilePath))
            {
                SQLiteConnection.CreateFile(dbFilePath); // 创建数据库文件
                InitializeConvention(); // 初始化 Convention 表
                InitializeOpenMainWindow(); // 初始化 OpenMainWindow 表
                InitializeBlacklist(); // 初始化 Blacklist 表
                InitializeBlacklistApplication(); // 初始化 BlacklistApplication 表
                InitializeAppearance(); // 初始化 Appearance 表
            }
        }

        // 初始化 Convention 表
        private static void InitializeConvention()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var createConventionCommand = new SQLiteCommand(SQLStatements.CreateConventionTable, connection); // 创建 SQLiteCommand 对象
            createConventionCommand.ExecuteNonQuery(); // 执行创建表的命令
            InsertDefaultConventionData(); // 插入默认数据
        }
        // 插入默认数据
        private static void InsertDefaultConventionData()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            var defaults = (currentVersion, false, true, true, 0.0, false, 300, 50, true, false, 111, true); // 使用参数元组封装默认值
            var parameters = new Dictionary<string, object>
            {
                ["@Version"] = defaults.Item1,
                ["@AutoStart"] = defaults.Item2,
                ["@ShowNotification"] = defaults.Item3,
                ["@ShowAddImage"] = defaults.Item4,
                ["@TotalUsageTime"] = defaults.Item5,
                ["@HideTooltip"] = defaults.Item6,
                ["@LongPressThreshold"] = defaults.Item7,
                ["@MouseMovePixels"] = defaults.Item8,
                ["@LoopPageFlipping"] = defaults.Item9,
                ["@RememberLastPage"] = defaults.Item10,
                ["@LastPage"] = defaults.Item11,
                ["@EnableMemoryOptimization"] = defaults.Item12
            }; // 使用字典批量绑定参数
            using var command = new SQLiteCommand(SQLStatements.InsertConvention, connection); // 创建 SQLiteCommand 对象
            foreach (var param in parameters)
                command.Parameters.AddWithValue(param.Key, param.Value); // 绑定参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        // 初始化OpenMainWindow 表
        private static void InitializeOpenMainWindow()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var createOpenMainWindowCommand = new SQLiteCommand(SQLStatements.CreateOpenMainWindowTable, connection); // 创建 SQLiteCommand 对象
            createOpenMainWindowCommand.ExecuteNonQuery(); // 执行创建表的命令
            InsertDefaultOpenMainWindowData(); // 插入默认数据
        }
        // 插入默认数据
        private static void InsertDefaultOpenMainWindowData()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            var defaults = (false, true, 2); // 使用参数元组封装默认值
            var parameters = new Dictionary<string, object>
            {
                ["@OpenMainWindowByMiddleMouseClick"] = defaults.Item1,
                ["@OpenMainWindowByX1MouseClick"] = defaults.Item1,
                ["@OpenMainWindowByX2MouseClick"] = defaults.Item1,
                ["@OpenMainWindowByCtrl_MiddleMouseClick"] = defaults.Item1,
                ["@OpenMainWindowByCtrl_RightMouseClick"] = defaults.Item1,
                ["@OpenMainWindowByMiddleMouseClickLonger"] = defaults.Item1,
                ["@OpenMainWindowByRightMouseClickLonger"] = defaults.Item1,
                ["@OpenMainWindowByRightMouseClick_Move"] = defaults.Item1,
                ["@OpenMainWindowByCtrl"] = defaults.Item2,
                ["@WindowStartupLocation"] = defaults.Item3
            }; // 使用字典批量绑定参数
            using var command = new SQLiteCommand(SQLStatements.InsertOpenMainWindow, connection); // 创建 SQLiteCommand 对象
            foreach (var param in parameters)
                command.Parameters.AddWithValue(param.Key, param.Value); // 绑定参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        // 初始化 Blacklist 表
        public static void InitializeBlacklist()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var createBlacklistCommand = new SQLiteCommand(SQLStatements.CreateBlacklistTable, connection); // 创建 SQLiteCommand 对象
            createBlacklistCommand.ExecuteNonQuery(); // 执行创建表的命令
            InsertDefaultBlacklistData(); // 插入默认数据
        }
        // 插入默认数据
        private static void InsertDefaultBlacklistData()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            var defaults = (false, false); // 使用参数元组封装默认值
            var parameters = new Dictionary<string, object>
            {
                ["@IsFullScreenDisabled"] = defaults.Item1,
                ["@IsBlacklistEnabledForExtendedHotkey"] = defaults.Item2
            }; // 使用字典批量绑定参数
            using var command = new SQLiteCommand(SQLStatements.InsertBlacklist, connection); // 创建 SQLiteCommand 对象
            foreach (var param in parameters)
                command.Parameters.AddWithValue(param.Key, param.Value); // 绑定参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        // 初始化 BlacklistApplication 表
        public static void InitializeBlacklistApplication()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var createBlacklistApplicationCommand = new SQLiteCommand(SQLStatements.CreateBlacklistApplicationTable, connection); // 创建 SQLiteCommand 对象
            createBlacklistApplicationCommand.ExecuteNonQuery(); // 执行创建表的命令
        }

        // 初始化 Appearance 表
        private static void InitializeAppearance()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var createAppearanceCommand = new SQLiteCommand(SQLStatements.CreateAppearanceTable, connection); // 创建 SQLiteCommand 对象
            createAppearanceCommand.ExecuteNonQuery(); // 执行创建表的命令
            InsertDefaultAppearanceData(); // 插入默认数据
        }

        // 插入默认数据
        private static void InsertDefaultAppearanceData()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            var defaults = (77.6, 0.2, 0.0, 0.0, "#FFF3F3F3", "#FFD3D3D3", "#00F3F3F3", "#FFA1A1A1", "#FFFFFFFF", "#FFBEE6FD", "#FFF3F3F3", "#FFEAEAEA", "#FF000000", "#FF696969", "#D0FF8C00", "#FF666666", -1, -1, 12, 400, "", 1.0, 1, 0, false, false, false, false, false); // 使用参数元组封装默认值
            var parameters = new Dictionary<string, object>
            {
                ["@ButtonSize"] = defaults.Item1, // 按钮大小
                ["@ButtonGap"] = defaults.Item2, // 按钮间隙
                ["@BorderWidth"] = defaults.Item3, // 边框宽度
                ["@ButtonCornerRadius"] = defaults.Item4, // 按钮圆角
                ["@BackgroundColor"] = defaults.Item5, // 背景颜色
                ["@BorderColor"] = defaults.Item6, // 边框颜色
                ["@ToolbarColor"] = defaults.Item7, // 工具栏颜色
                ["@ToolbarIconColor"] = defaults.Item8, // 工具栏图标颜色
                ["@ActionButtonColor"] = defaults.Item9, // 动作按钮颜色
                ["@ActionButtonMouseOverColor"] = defaults.Item10, // 动作按钮鼠标悬停颜色
                ["@BlankButtonColor"] = defaults.Item11, // 空白按钮颜色
                ["@BlankButtonMouseOverColor"] = defaults.Item12, // 空白按钮鼠标悬停颜色
                ["@ButtonTextColor"] = defaults.Item13, // 按钮文字颜色
                ["@ActionIconColor"] = defaults.Item14, // 动作图标颜色
                ["@TriggerKeyTextColor"] = defaults.Item15, // 触发键文字颜色
                ["@OtherIconColor"] = defaults.Item16, // 其他位置图标颜色
                ["@Font1"] = defaults.Item17, // 字体1
                ["@Font2"] = defaults.Item18, // 字体2
                ["@FontSize"] = defaults.Item19, // 字体大小
                ["@FontWeight"] = defaults.Item20, // 字体粗细
                ["@BackgroundImagePath"] = defaults.Item21, // 背景图片路径
                ["@BackgroundImageOpacity"] = defaults.Item22, // 背景图片不透明度
                ["@Blur"] = defaults.Item23, // 模糊模式
                ["@Win11CornerRadius"] = defaults.Item24, // Win11圆角模式
                ["@AutoHideTitleBar"] = defaults.Item25, // 自动缩小动作名称文字
                ["@ShowActionButtonMouseOver"] = defaults.Item26, // 鼠标悬浮在动作按钮上时，放大显示按钮
                ["@HideActionNameAfterIcon"] = defaults.Item27, // 设置动作图标后隐藏动作名称
                ["@ShowActionIconShadow"] = defaults.Item28, // 动作图标显示阴影
                ["@EnablePreview"] = defaults.Item29 // 开启预览功能
            };
            using var command = new SQLiteCommand(SQLStatements.InsertAppearance, connection); // 创建 SQLiteCommand 对象
            foreach (var param in parameters)
                command.Parameters.AddWithValue(param.Key, param.Value); // 绑定参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        /// <summary>
        /// 更新Convention设置信息
        /// </summary>
        /// <param name="autostart"> 是否开机自启 </param>
        /// <param name="shownotification"> 是否显示通知 </param>
        /// <param name="showaddimage"> 是否显示添加图片 </param>
        /// <param name="hideTooltip"> 是否隐藏提示 </param>
        /// <param name="longPressThreshold"> 长按阈值 </param>
        /// <param name="mouseMovePixels"> 鼠标移动像素 </param>
        /// <param name="loopPageFlipping"> 是否循环翻页 </param>
        /// <param name="rememberLastPage"> 是否记住设置窗口中最后打开的页面 </param>
        /// <param name="enableMemoryOptimization"> 是否启用内存优化 </param>
        public static void ApplyConventionSettings(bool autostart, bool shownotification, bool showaddimage, bool hideTooltip, int longPressThreshold, int mouseMovePixels, bool loopPageFlipping, bool rememberLastPage, bool enableMemoryOptimization)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.UpdateConvention, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@AutoStart", autostart); // 是否开机自启
            command.Parameters.AddWithValue("@ShowNotification", shownotification); // 是否显示通知
            command.Parameters.AddWithValue("@ShowAddImage", showaddimage); // 是否显示添加图片
            command.Parameters.AddWithValue("@HideTooltip", hideTooltip); // 是否隐藏提示
            command.Parameters.AddWithValue("@LongPressThreshold", longPressThreshold); // 长按阈值
            command.Parameters.AddWithValue("@MouseMovePixels", mouseMovePixels); // 鼠标移动像素
            command.Parameters.AddWithValue("@LoopPageFlipping", loopPageFlipping); // 是否循环翻页
            command.Parameters.AddWithValue("@RememberLastPage", rememberLastPage); // 是否记住设置窗口中最后打开的页面
            command.Parameters.AddWithValue("@EnableMemoryOptimization", enableMemoryOptimization); // 是否启用内存优化
            command.ExecuteNonQuery(); // 执行更新命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 设置窗口中最后打开的页面
        /// </summary>
        /// <param name="lastPage"> 设置窗口中最后打开的页面 </param>
        public static void RecordLastPage(int lastPage)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand(SQLStatements.UpdateLastPage, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@LastPage", lastPage); // 绑定参数
            command.ExecuteNonQuery(); // 执行更新命令
        }

        /// <summary>
        /// 更新OpenMainWindow设置信息
        /// </summary>
        /// <param name="OpenMainWindowByMiddleMouseClick"> 按下中键 </param>
        /// <param name="OpenMainWindowByX1MouseClick"> 按下X1键 </param>
        /// <param name="OpenMainWindowByX2MouseClick"> 按下X2键 </param>
        /// <param name="OpenMainWindowByCtrl_MiddleMouseClick"> Ctrl+中键单击 </param>
        /// <param name="OpenMainWindowByCtrl_RightMouseClick"> Ctrl+右键单击 </param>
        /// <param name="OpenMainWindowByMiddleMouseClickLonger"> 长按中键 </param>
        /// <param name="OpenMainWindowByRightMouseClickLonger"> 长按右键 </param>
        /// <param name="OpenMainWindowByRightMouseClick_Move"> 按右键移动 </param>
        /// <param name="OpenMainWindowByCtrl"> 单击Ctrl键 </param>
        /// <param name="windowStartupLocation"> 功能面板打开位置 </param>
        public static void ApplyOpenMainWindowSettings(bool OpenMainWindowByMiddleMouseClick, bool OpenMainWindowByX1MouseClick, bool OpenMainWindowByX2MouseClick, bool OpenMainWindowByCtrl_MiddleMouseClick, bool OpenMainWindowByCtrl_RightMouseClick, bool OpenMainWindowByMiddleMouseClickLonger, bool OpenMainWindowByRightMouseClickLonger, bool OpenMainWindowByRightMouseClick_Move, bool OpenMainWindowByCtrl, int windowStartupLocation)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.UpdateOpenMainWindow, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@OpenMainWindowByMiddleMouseClick", OpenMainWindowByMiddleMouseClick); // 按下中键
            command.Parameters.AddWithValue("@OpenMainWindowByX1MouseClick", OpenMainWindowByX1MouseClick); // 按下X1键
            command.Parameters.AddWithValue("@OpenMainWindowByX2MouseClick", OpenMainWindowByX2MouseClick); // 按下X2键
            command.Parameters.AddWithValue("@OpenMainWindowByCtrl_MiddleMouseClick", OpenMainWindowByCtrl_MiddleMouseClick); // Ctrl+中键单击
            command.Parameters.AddWithValue("@OpenMainWindowByCtrl_RightMouseClick", OpenMainWindowByCtrl_RightMouseClick); // Ctrl+右键单击
            command.Parameters.AddWithValue("@OpenMainWindowByMiddleMouseClickLonger", OpenMainWindowByMiddleMouseClickLonger); // 长按中键
            command.Parameters.AddWithValue("@OpenMainWindowByRightMouseClickLonger", OpenMainWindowByRightMouseClickLonger); // 长按右键
            command.Parameters.AddWithValue("@OpenMainWindowByRightMouseClick_Move", OpenMainWindowByRightMouseClick_Move); // 按右键移动
            command.Parameters.AddWithValue("@OpenMainWindowByCtrl", OpenMainWindowByCtrl); // 单击Ctrl键
            command.Parameters.AddWithValue("@WindowStartupLocation", windowStartupLocation); // 功能面板打开位置
            command.ExecuteNonQuery(); // 执行更新命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 更新Blacklist设置信息
        /// </summary>
        /// <param name="isFullScreenDisabled"> 是否开启全屏或最大化禁用功能 </param>
        /// <param name="isBlacklistEnabledForExtendedHotkey"> 是否将黑名单与全屏禁用设置应用于扩展热键功能 </param>
        public static void ApplyBlacklistSettings(bool isFullScreenDisabled, bool isBlacklistEnabledForExtendedHotkey)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.UpdateBlacklist, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@IsFullScreenDisabled", isFullScreenDisabled); // 是否开启全屏或最大化禁用功能
            command.Parameters.AddWithValue("@IsBlacklistEnabledForExtendedHotkey", isBlacklistEnabledForExtendedHotkey); // 是否将黑名单与全屏禁用设置应用于扩展热键功能
            command.ExecuteNonQuery(); // 执行更新命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 添加黑名单应用
        /// </summary>
        /// <param name="applicationName"> 应用名称 </param>
        /// <param name="processName"> 进程名称 </param>
        /// <param name="isInBlacklist"> 是否在黑名单中 </param>
        /// <param name="isFolder"> 是否是文件夹 </param>
        public static void ApplyBlacklistApplication(string applicationName, string processName, bool isInBlacklist, bool isFolder)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.InsertBlacklistApplication, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@ApplicationName", applicationName); // 应用名称
            command.Parameters.AddWithValue("@ProcessName", processName); // 进程名称
            command.Parameters.AddWithValue("@IsInBlacklist", isInBlacklist); // 是否在黑名单中
            command.Parameters.AddWithValue("@IsFolder", isFolder); // 是否是文件夹
            command.ExecuteNonQuery(); // 执行插入命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 通过应用名称删除黑名单应用
        /// </summary>
        /// <param name="applicationName"> 应用名称 </param>
        public static void DeleteBlacklistApplication(string applicationName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.DeleteBlacklistApplication, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@ApplicationName", applicationName); // 应用名称
            command.ExecuteNonQuery(); // 执行删除命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 保存总使用时长
        /// </summary>
        /// <param name="totalUsageTime"> 总使用时长 </param>
        public static void SaveTotalUsageTime(double totalUsageTime)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.UpdateTotalUsageTime, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@TotalUsageTime", totalUsageTime); // 设置参数
            command.ExecuteNonQuery(); // 执行更新命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 获取常规设置信息
        /// </summary>
        /// <returns> Convention 类 </returns>
        public static List<Convention> GetAllConventions()
        {
            var conventions = new List<Convention>(); // 创建一个空的 Convention 列表
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.GetAllConventions, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询并获取结果
            while (reader.Read())
            {
                conventions.Add(new Convention
                {
                    ID = reader.GetInt32(0), // 主键
                    Version = reader.GetString(1), // 版本号
                    AutoStart = reader.GetBoolean(2), // 是否开机自启
                    ShowNotification = reader.GetBoolean(3), // 是否显示通知
                    ShowAddImage = reader.GetBoolean(4), // 是否显示添加图片
                    TotalUsageTime = reader.GetDouble(5), // 总使用时长
                    HideTooltip = reader.GetBoolean(6), // 是否隐藏提示
                    LongPressThreshold = reader.GetInt32(7), // 长按阈值
                    MouseMovePixels = reader.GetInt32(8), // 鼠标移动像素
                    LoopPageFlipping = reader.GetBoolean(9), // 是否循环翻页
                    RememberLastPage = reader.GetBoolean(10), // 是否记住设置窗口中最后打开的页面
                    LastPage = reader.GetInt32(11), // 设置窗口中最后打开的页面
                    EnableMemoryOptimization = reader.GetBoolean(12) // 是否启用内存优化
                }); // 将读取到的数据添加到列表中
            }
            transaction.Commit(); // 提交事务
            return conventions; // 返回所有 Convention 数据
        }

        /// <summary>
        /// 获取OpenMainWindow设置信息
        /// </summary>
        /// <returns> OpenMainWindow 类 </returns>
        public static List<OpenMainWindow> GetAllOpenMainWindowConditions()
        {
            var conditions = new List<OpenMainWindow>(); // 创建一个空的 OpenMainWindow 列表
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.GetAllOpenMainWindowConditions, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询并获取结果
            while (reader.Read())
            {
                conditions.Add(new OpenMainWindow
                {
                    ID = reader.GetInt32(0),
                    OpenMainWindowByMiddleMouseClick = reader.GetBoolean(1), // 按下中键
                    OpenMainWindowByX1MouseClick = reader.GetBoolean(2), // 按下X1键
                    OpenMainWindowByX2MouseClick = reader.GetBoolean(3), // 按下X2键
                    OpenMainWindowByCtrl_MiddleMouseClick = reader.GetBoolean(4), // Ctrl+中键单击
                    OpenMainWindowByCtrl_RightMouseClick = reader.GetBoolean(5), // Ctrl+右键单击
                    OpenMainWindowByMiddleMouseClickLonger = reader.GetBoolean(6), // 长按中键
                    OpenMainWindowByRightMouseClickLonger = reader.GetBoolean(7), // 长按右键
                    OpenMainWindowByRightMouseClick_Move = reader.GetBoolean(8), // 按右键移动
                    OpenMainWindowByCtrl = reader.GetBoolean(9), // 单击Ctrl键
                    WindowStartupLocation = reader.GetInt32(10) // 功能面板打开位置
                }); // 将读取到的数据添加到列表中
            }
            transaction.Commit(); // 提交事务
            return conditions; // 返回所有 OpenMainWindow 数据
        }

        /// <summary>
        /// 获取黑名单设置
        /// </summary>
        /// <returns> Blacklist 类 </returns>
        public static List<Blacklist> GetAllBlacklistSettings()
        {
            var blacklists = new List<Blacklist>(); // 创建一个空的 Blacklist 列表
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.GetAllBlacklistSettings, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询并获取结果
            while (reader.Read())
            {
                blacklists.Add(new Blacklist
                {
                    ID = reader.GetInt32(0), // 主键
                    IsFullScreenDisabled = reader.GetBoolean(1), // 是否开启全屏或最大化禁用功能
                    IsBlacklistEnabledForExtendedHotkey = reader.GetBoolean(2) // 是否将黑名单与全屏禁用设置应用于扩展热键功能
                }); // 将读取到的数据添加到列表中
            }
            transaction.Commit(); // 提交事务
            return blacklists; // 返回所有 Blacklist 数据
        }

        /// <summary>
        /// 获取黑名单应用
        /// </summary>
        /// <returns> BlacklistApplication 类 </returns>
        public static List<BlacklistApplication> GetAllBlacklistApplications()
        {
            var applications = new List<BlacklistApplication>(); // 创建一个空的 BlacklistApplication 列表
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.GetAllBlacklistApplications, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询并获取结果
            while (reader.Read())
            {
                applications.Add(new BlacklistApplication
                {
                    ID = reader.GetInt32(0), // 主键
                    ApplicationName = reader.GetString(1), // 应用名称
                    ProcessName = reader.GetString(2), // 进程名称
                    IsInBlacklist = reader.GetBoolean(3), // 是否在黑名单中
                    IsFolder = reader.GetBoolean(4) // 是否是文件夹
                }); // 将读取到的数据添加到列表中
            }
            transaction.Commit(); // 提交事务
            return applications; // 返回所有 BlacklistApplication 数据
        }

        /// <summary>
        /// 获取外观设置
        /// </summary>
        /// <returns> Appearance 类 </returns>
        public static List<Appearance> GetAllAppearanceSettings()
        {
            var appearances = new List<Appearance>();
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted); // 开启只读事务
            using var command = new SQLiteCommand(SQLStatements.GetAllAppearanceSettings, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询并获取数据读取器
            while (reader.Read())
            {
                appearances.Add(new Appearance
                {
                    ID = reader.GetInt32(0), // 外观ID
                    ButtonSize = reader.GetDouble(1), // 按钮大小
                    ButtonGap = reader.GetDouble(2), // 按钮间隙
                    BorderWidth = reader.GetDouble(3), // 边框宽度
                    ButtonCornerRadius = reader.GetDouble(4), // 按钮圆角
                    BackgroundColor = reader.GetString(5), // 背景颜色
                    BorderColor = reader.GetString(6), // 边框颜色
                    ToolbarColor = reader.GetString(7), // 工具栏颜色
                    ToolbarIconColor = reader.GetString(8), // 工具栏图标颜色
                    ActionButtonColor = reader.GetString(9), // 动作按钮颜色
                    ActionButtonMouseOverColor = reader.GetString(10), // 动作按钮悬浮颜色
                    BlankButtonColor = reader.GetString(11), // 空白按钮颜色
                    BlankButtonMouseOverColor = reader.GetString(12), // 空白按钮悬浮颜色
                    ButtonTextColor = reader.GetString(13), // 按钮文字颜色
                    ActionIconColor = reader.GetString(14), // 动作图标颜色
                    TriggerKeyTextColor = reader.GetString(15), // 触发键文字颜色
                    OtherIconColor = reader.GetString(16), // 其他图标颜色
                    Font1 = reader.GetInt32(17), // 字体1
                    Font2 = reader.GetInt32(18), // 字体2
                    FontSize = reader.GetDouble(19), // 字体大小
                    FontWeight = reader.GetDouble(20), // 字体粗细
                    BackgroundImagePath = reader.GetString(21), // 背景图片路径
                    BackgroundImageOpacity = reader.GetDouble(22), // 背景图片不透明度
                    Blur = reader.GetInt32(23), // 模糊模式
                    Win11CornerRadius = reader.GetInt32(24), // Win11圆角模式
                    AutoHideTitleBar = reader.GetBoolean(25), // 自动隐藏标题栏
                    ShowActionButtonMouseOver = reader.GetBoolean(26), // 动作按钮悬浮放大
                    HideActionNameAfterIcon = reader.GetBoolean(27), // 设置动作图标后隐藏名称
                    ShowActionIconShadow = reader.GetBoolean(28), // 动作图标显示阴影
                    EnablePreview = reader.GetBoolean(29) // 启用预览
                });
            }
            transaction.Commit(); // 提交事务
            return appearances; // 返回所有外观设置
        }

        /// <summary>
        /// 更新外观设置
        /// </summary>
        /// <param name="appearance"> Appearance 类 </param>
        public static void UpdateAppearance(Appearance appearance)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            using var command = new SQLiteCommand(SQLStatements.UpdateAppearance, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@ButtonSize", appearance.ButtonSize); // 按钮大小
            command.Parameters.AddWithValue("@ButtonGap", appearance.ButtonGap); // 按钮间隙
            command.Parameters.AddWithValue("@BorderWidth", appearance.BorderWidth); // 边框宽度
            command.Parameters.AddWithValue("@ButtonCornerRadius", appearance.ButtonCornerRadius); // 按钮圆角
            command.Parameters.AddWithValue("@BackgroundColor", appearance.BackgroundColor); // 背景颜色
            command.Parameters.AddWithValue("@BorderColor", appearance.BorderColor); // 边框颜色
            command.Parameters.AddWithValue("@ToolbarColor", appearance.ToolbarColor); // 工具栏颜色
            command.Parameters.AddWithValue("@ToolbarIconColor", appearance.ToolbarIconColor); // 工具栏图标颜色
            command.Parameters.AddWithValue("@ActionButtonColor", appearance.ActionButtonColor); // 动作按钮颜色
            command.Parameters.AddWithValue("@ActionButtonMouseOverColor", appearance.ActionButtonMouseOverColor); // 动作按钮悬浮颜色
            command.Parameters.AddWithValue("@BlankButtonColor", appearance.BlankButtonColor); // 空白按钮颜色
            command.Parameters.AddWithValue("@BlankButtonMouseOverColor", appearance.BlankButtonMouseOverColor); // 空白按钮悬浮颜色
            command.Parameters.AddWithValue("@ButtonTextColor", appearance.ButtonTextColor); // 按钮文字颜色
            command.Parameters.AddWithValue("@ActionIconColor", appearance.ActionIconColor); // 动作图标颜色
            command.Parameters.AddWithValue("@TriggerKeyTextColor", appearance.TriggerKeyTextColor); // 触发键文字颜色
            command.Parameters.AddWithValue("@OtherIconColor", appearance.OtherIconColor); // 其他图标颜色
            command.Parameters.AddWithValue("@Font1", appearance.Font1); // 字体1
            command.Parameters.AddWithValue("@Font2", appearance.Font2); // 字体2
            command.Parameters.AddWithValue("@FontSize", appearance.FontSize); // 字体大小
            command.Parameters.AddWithValue("@FontWeight", appearance.FontWeight); // 字体粗细
            command.Parameters.AddWithValue("@BackgroundImagePath", appearance.BackgroundImagePath); // 背景图片路径
            command.Parameters.AddWithValue("@BackgroundImageOpacity", appearance.BackgroundImageOpacity); // 背景图片不透明度
            command.Parameters.AddWithValue("@Blur", appearance.Blur); // 模糊模式
            command.Parameters.AddWithValue("@Win11CornerRadius", appearance.Win11CornerRadius); // Win11圆角模式
            command.Parameters.AddWithValue("@AutoHideTitleBar", appearance.AutoHideTitleBar); // 自动隐藏标题栏
            command.Parameters.AddWithValue("@ShowActionButtonMouseOver", appearance.ShowActionButtonMouseOver); // 动作按钮悬浮放大
            command.Parameters.AddWithValue("@HideActionNameAfterIcon", appearance.HideActionNameAfterIcon); // 设置动作图标后隐藏名称
            command.Parameters.AddWithValue("@ShowActionIconShadow", appearance.ShowActionIconShadow); // 动作图标显示阴影
            command.Parameters.AddWithValue("@EnablePreview", appearance.EnablePreview); // 启用预览
            command.ExecuteNonQuery(); // 执行命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <returns> SQLiteConnection 对象 </returns>
        public static SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(db1); // 创建 SQLiteConnection 对象
            connection.BusyTimeout = 30000; // 设置超时时间为 30 秒
            connection.Open(); // 打开数据库连接
            return connection; // 返回打开的连接
        }

        // 数据库文件路径语句
        private static class SQLStatements
        {
            // 常规设置表
            public const string CreateConventionTable = @"
            CREATE TABLE IF NOT EXISTS Convention
            (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Version TEXT,
                AutoStart BOOLEAN,
                ShowNotification BOOLEAN,
                ShowAddImage BOOLEAN,
                TotalUsageTime REAL,
                HideTooltip BOOLEAN,
                LongPressThreshold INTEGER,
                MouseMovePixels INTEGER,
                LoopPageFlipping BOOLEAN,
                RememberLastPage BOOLEAN,
                LastPage INTEGER,
                EnableMemoryOptimization BOOLEAN
            );";
            public const string InsertConvention = @"
            INSERT INTO Convention
            (
                Version,            AutoStart, 
                ShowNotification,   ShowAddImage,
                TotalUsageTime,     HideTooltip,
                LongPressThreshold, MouseMovePixels,
                LoopPageFlipping,   RememberLastPage,
                LastPage,           EnableMemoryOptimization
            )
            VALUES
            (
                @Version,           @AutoStart,
                @ShowNotification,  @ShowAddImage,
                @TotalUsageTime,    @HideTooltip,
                @LongPressThreshold,@MouseMovePixels,
                @LoopPageFlipping,  @RememberLastPage,
                @LastPage,          @EnableMemoryOptimization
            );";
            public const string UpdateConvention = @"
            UPDATE Convention SET
                AutoStart = @AutoStart,
                ShowNotification = @ShowNotification,
                ShowAddImage = @ShowAddImage,
                HideTooltip = @HideTooltip,
                LongPressThreshold = @LongPressThreshold,
                MouseMovePixels = @MouseMovePixels,
                LoopPageFlipping = @LoopPageFlipping,
                RememberLastPage = @RememberLastPage,
                EnableMemoryOptimization = @EnableMemoryOptimization
            WHERE ID = 1;";
            public const string GetAllConventions = "SELECT * FROM Convention;";
            // 打开主窗口设置表
            public const string CreateOpenMainWindowTable = @"
            CREATE TABLE IF NOT EXISTS OpenMainWindow
            (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                OpenMainWindowByMiddleMouseClick BOOLEAN,
                OpenMainWindowByX1MouseClick BOOLEAN,
                OpenMainWindowByX2MouseClick BOOLEAN,
                OpenMainWindowByCtrl_MiddleMouseClick BOOLEAN,
                OpenMainWindowByCtrl_RightMouseClick BOOLEAN,
                OpenMainWindowByMiddleMouseClickLonger BOOLEAN,
                OpenMainWindowByRightMouseClickLonger BOOLEAN,
                OpenMainWindowByRightMouseClick_Move BOOLEAN,
                OpenMainWindowByCtrl BOOLEAN,
                WindowStartupLocation INTEGER
            );";
            public const string InsertOpenMainWindow = @"
            INSERT INTO OpenMainWindow
            (
                OpenMainWindowByMiddleMouseClick,
                OpenMainWindowByX1MouseClick,
                OpenMainWindowByX2MouseClick,
                OpenMainWindowByCtrl_MiddleMouseClick,
                OpenMainWindowByCtrl_RightMouseClick,
                OpenMainWindowByMiddleMouseClickLonger,
                OpenMainWindowByRightMouseClickLonger,
                OpenMainWindowByRightMouseClick_Move,
                OpenMainWindowByCtrl,
                WindowStartupLocation
            )
            VALUES
            (
                @OpenMainWindowByMiddleMouseClick,
                @OpenMainWindowByX1MouseClick,
                @OpenMainWindowByX2MouseClick,
                @OpenMainWindowByCtrl_MiddleMouseClick,
                @OpenMainWindowByCtrl_RightMouseClick,
                @OpenMainWindowByMiddleMouseClickLonger,
                @OpenMainWindowByRightMouseClickLonger,
                @OpenMainWindowByRightMouseClick_Move,
                @OpenMainWindowByCtrl,
                @WindowStartupLocation
            );";
            public const string UpdateOpenMainWindow = @"
            UPDATE OpenMainWindow SET
                OpenMainWindowByMiddleMouseClick = @OpenMainWindowByMiddleMouseClick,
                OpenMainWindowByX1MouseClick = @OpenMainWindowByX1MouseClick,
                OpenMainWindowByX2MouseClick = @OpenMainWindowByX2MouseClick,
                OpenMainWindowByCtrl_MiddleMouseClick = @OpenMainWindowByCtrl_MiddleMouseClick,
                OpenMainWindowByCtrl_RightMouseClick = @OpenMainWindowByCtrl_RightMouseClick,
                OpenMainWindowByMiddleMouseClickLonger = @OpenMainWindowByMiddleMouseClickLonger,
                OpenMainWindowByRightMouseClickLonger = @OpenMainWindowByRightMouseClickLonger,
                OpenMainWindowByRightMouseClick_Move = @OpenMainWindowByRightMouseClick_Move,
                OpenMainWindowByCtrl = @OpenMainWindowByCtrl,
                WindowStartupLocation = @WindowStartupLocation
            WHERE ID = 1;";
            public const string GetAllOpenMainWindowConditions = "SELECT * FROM OpenMainWindow;";
            // 黑名单设置表
            public const string CreateBlacklistTable = @"
            CREATE TABLE IF NOT EXISTS Blacklist
            (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                IsFullScreenDisabled BOOLEAN,
                IsBlacklistEnabledForExtendedHotkey BOOLEAN
            );";
            public const string InsertBlacklist = @"
            INSERT INTO Blacklist
            (
                IsFullScreenDisabled,
                IsBlacklistEnabledForExtendedHotkey
            )
            VALUES
            (
                @IsFullScreenDisabled,
                @IsBlacklistEnabledForExtendedHotkey
            );";
            public const string UpdateBlacklist = @"
            UPDATE Blacklist SET
                IsFullScreenDisabled = @IsFullScreenDisabled,
                IsBlacklistEnabledForExtendedHotkey = @IsBlacklistEnabledForExtendedHotkey
            WHERE ID = 1;";
            public const string GetAllBlacklistSettings = "SELECT * FROM Blacklist;";
            // 黑名单应用表
            public const string CreateBlacklistApplicationTable = @"
            CREATE TABLE IF NOT EXISTS BlacklistApplication
            (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                ApplicationName TEXT,
                ProcessName TEXT,
                IsInBlacklist BOOLEAN,
                IsFolder BOOLEAN
            );";
            public const string InsertBlacklistApplication = @"
            INSERT INTO BlacklistApplication
            (
                ApplicationName,
                ProcessName,
                IsInBlacklist,
                IsFolder
            )
            VALUES
            (
                @ApplicationName,
                @ProcessName,
                @IsInBlacklist,
                @IsFolder
            );";
            public const string DeleteBlacklistApplication = @"
            DELETE FROM BlacklistApplication WHERE ApplicationName = @ApplicationName;";
            public const string GetAllBlacklistApplications = "SELECT * FROM BlacklistApplication;";
            // 其他语句
            public const string UpdateTotalUsageTime = @"
            UPDATE Convention SET 
                TotalUsageTime = @TotalUsageTime 
            WHERE ID = 1;"; // 更新使用总时长
            public const string UpdateLastPage = @"
            UPDATE Convention SET 
                LastPage = @LastPage
            WHERE ID = 1;";
            // 外观设置表
            public const string CreateAppearanceTable = @"
            CREATE TABLE IF NOT EXISTS Appearance
            (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                ButtonSize REAL,
                ButtonGap REAL,
                BorderWidth REAL,
                ButtonCornerRadius REAL,
                BackgroundColor TEXT,
                BorderColor TEXT,
                ToolbarColor TEXT,
                ToolbarIconColor TEXT,
                ActionButtonColor TEXT,
                ActionButtonMouseOverColor TEXT,
                BlankButtonColor TEXT,
                BlankButtonMouseOverColor TEXT,
                ButtonTextColor TEXT,
                ActionIconColor TEXT,
                TriggerKeyTextColor TEXT,
                OtherIconColor TEXT,
                Font1 INTEGER,
                Font2 INTEGER,
                FontSize REAL,
                FontWeight REAL,
                BackgroundImagePath TEXT,
                BackgroundImageOpacity REAL,
                Blur INTEGER,
                Win11CornerRadius INTEGER,
                AutoHideTitleBar BOOLEAN,
                ShowActionButtonMouseOver BOOLEAN,
                HideActionNameAfterIcon BOOLEAN,
                ShowActionIconShadow BOOLEAN,
                EnablePreview BOOLEAN
            );";
            public const string InsertAppearance = @"
            INSERT INTO Appearance
            (
                ButtonSize,
                ButtonGap,
                BorderWidth,
                ButtonCornerRadius,
                BackgroundColor,
                BorderColor,
                ToolbarColor,
                ToolbarIconColor,
                ActionButtonColor,
                ActionButtonMouseOverColor,
                BlankButtonColor,
                BlankButtonMouseOverColor,
                ButtonTextColor,
                ActionIconColor,
                TriggerKeyTextColor,
                OtherIconColor,
                Font1,
                Font2,
                FontSize,
                FontWeight,
                BackgroundImagePath,
                BackgroundImageOpacity,
                Blur,
                Win11CornerRadius,
                AutoHideTitleBar,
                ShowActionButtonMouseOver,
                HideActionNameAfterIcon,
                ShowActionIconShadow,
                EnablePreview
            )
            VALUES
            (
                @ButtonSize,
                @ButtonGap,
                @BorderWidth,
                @ButtonCornerRadius,
                @BackgroundColor,
                @BorderColor,
                @ToolbarColor,
                @ToolbarIconColor,
                @ActionButtonColor,
                @ActionButtonMouseOverColor,
                @BlankButtonColor,
                @BlankButtonMouseOverColor,
                @ButtonTextColor,
                @ActionIconColor,
                @TriggerKeyTextColor,
                @OtherIconColor,
                @Font1,
                @Font2,
                @FontSize,
                @FontWeight,
                @BackgroundImagePath,
                @BackgroundImageOpacity,
                @Blur,
                @Win11CornerRadius,
                @AutoHideTitleBar,
                @ShowActionButtonMouseOver,
                @HideActionNameAfterIcon,
                @ShowActionIconShadow,
                @EnablePreview
            );"; // 插入外观设置
            public const string GetAllAppearanceSettings = "SELECT * FROM Appearance;"; // 获取所有外观设置
            public const string UpdateAppearance = @"
            UPDATE Appearance SET
                ButtonSize = @ButtonSize,
                ButtonGap = @ButtonGap,
                BorderWidth = @BorderWidth,
                ButtonCornerRadius = @ButtonCornerRadius,
                BackgroundColor = @BackgroundColor,
                BorderColor = @BorderColor,
                ToolbarColor = @ToolbarColor,
                ToolbarIconColor = @ToolbarIconColor,
                ActionButtonColor = @ActionButtonColor,
                ActionButtonMouseOverColor = @ActionButtonMouseOverColor,
                BlankButtonColor = @BlankButtonColor,
                BlankButtonMouseOverColor = @BlankButtonMouseOverColor,
                ButtonTextColor = @ButtonTextColor,
                ActionIconColor = @ActionIconColor,
                TriggerKeyTextColor = @TriggerKeyTextColor,
                OtherIconColor = @OtherIconColor,
                Font1 = @Font1,
                Font2 = @Font2,
                FontSize = @FontSize,
                FontWeight = @FontWeight,
                BackgroundImagePath = @BackgroundImagePath,
                BackgroundImageOpacity = @BackgroundImageOpacity,
                Blur = @Blur,
                Win11CornerRadius = @Win11CornerRadius,
                AutoHideTitleBar = @AutoHideTitleBar,
                ShowActionButtonMouseOver = @ShowActionButtonMouseOver,
                HideActionNameAfterIcon = @HideActionNameAfterIcon,
                ShowActionIconShadow = @ShowActionIconShadow,
                EnablePreview = @EnablePreview
            WHERE ID = 1;"; // 更新外观设置
        }
    }
}