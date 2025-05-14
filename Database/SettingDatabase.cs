using System.Collections.Generic;
using System.Data.SQLite;
using Quicker.Database;
using System.IO;

// SQLite数据库操作类
public class SettingDatabase
{
    // 获取应用程序根目录，并设置数据库文件路径为根目录下的"Database"文件夹
    private readonly string db1 = "Data Source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Setting.db") + ";Pooling=true;Max Pool Size=100;Journal Mode=Wal;";
    private readonly static ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库
    private readonly string currentVersion = "2.1.3"; // 当前版本号

    public SettingDatabase()
    {
        Initialize(); // 初始化数据库
    }

    // 初始化数据库
    public void Initialize()
    {
        string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
        if (!Directory.Exists(dbFolder))
            Directory.CreateDirectory(dbFolder); // 如果"Database"文件夹不存在，则创建它
        string dbFilePath = Path.Combine(dbFolder, "Setting.db"); // 设置数据库文件路径
        if (File.Exists(dbFilePath))
            CheckAndUpgradeDatabase(); // 如果数据库存在，则检查并升级数据库
        else
        {
            SQLiteConnection.CreateFile(dbFilePath); // 创建数据库文件
            InitializeConvention(); // 初始化 Convention 表
            InitializeOpenMainWindow(); // 初始化 OpenMainWindow 表
            InitializeBlacklist(); // 初始化 Blacklist 表
            InitializeBlacklistApplication(); // 初始化 BlacklistApplication 表
        }
    }

    // 检查数据库版本并进行升级
    public void CheckAndUpgradeDatabase()
    {
        if (IsNewVersion()) return; // 如果数据库是最新版本，则直接返回
        UpdateDatabase(); // 升级数据库
    }

    // 判断数据库是否是最新版本
    public bool IsNewVersion()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string selectVersionQuery = "SELECT Version FROM Convention ORDER BY ID DESC LIMIT 1;"; // 查询版本号
        using var command = new SQLiteCommand(selectVersionQuery, connection); // 创建 SQLiteCommand 对象
        using var reader = command.ExecuteReader(); // 执行查询命令
        if (reader.Read()) // 检查是否有数据
        {
            var thisVersion = reader.GetString(0); // 返回版本号
            if (thisVersion == currentVersion) return true; // 数据库版本已是最新，返回true
        }
        return false; // 数据库版本不是最新，或者没有数据，返回false
    }

    // 升级数据库
    private void UpdateDatabase()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string updateVersionQuery = @$"UPDATE Convention SET Version = '{currentVersion}';"; // 设置默认值
        using var updateVersionCommand = new SQLiteCommand(updateVersionQuery, connection); // 创建 SQLiteCommand 对象
        updateVersionCommand.ExecuteNonQuery(); // 执行更新命令

        // 其它的升级操作...
        db2.UpdateDatabase(); // 升级按钮数据库
    }

    // 初始化 Convention 表
    private void InitializeConvention()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string createConventionTableQuery = @"
        CREATE TABLE IF NOT EXISTS Convention
        (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            Version TEXT,
            AutoStart BOOL,
            ShowNotification BOOL,
            ShowAddImage BOOL,
            TotalUsageTime REAL,
            HideTooltip BOOL,
            LongPressThreshold INTEGER,
            MouseMovePixels INTEGER,
            LoopPageFlipping BOOL
        );"; // 创建 Convention 表
        using var createConventionCommand = new SQLiteCommand(createConventionTableQuery, connection); // 创建 SQLiteCommand 对象
        createConventionCommand.ExecuteNonQuery(); // 执行创建表的命令
        InsertDefaultConventionData(); // 插入默认数据
    }

    // 插入默认数据
    private void InsertDefaultConventionData()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string insertConventionQuery = @"
            INSERT INTO Convention 
            (Version, AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping)
            VALUES 
            (@Version, @AutoStart, @ShowNotification, @ShowAddImage, @TotalUsageTime, @HideTooltip, @LongPressThreshold, @MouseMovePixels, @LoopPageFlipping);";
        using var insertConventionCommand = new SQLiteCommand(insertConventionQuery, connection); // 创建 SQLiteCommand 对象
        insertConventionCommand.Parameters.AddWithValue("@Version", currentVersion); // 版本号
        insertConventionCommand.Parameters.AddWithValue("@AutoStart", false); // 是否开机自启
        insertConventionCommand.Parameters.AddWithValue("@ShowNotification", true); // 是否显示通知
        insertConventionCommand.Parameters.AddWithValue("@ShowAddImage", true); // 是否显示添加图片
        insertConventionCommand.Parameters.AddWithValue("@TotalUsageTime", 0.0); // 总使用时长
        insertConventionCommand.Parameters.AddWithValue("@HideTooltip", false); // 是否隐藏提示
        insertConventionCommand.Parameters.AddWithValue("@LongPressThreshold", 300); // 长按阈值
        insertConventionCommand.Parameters.AddWithValue("@MouseMovePixels", 50); // 鼠标移动像素
        insertConventionCommand.Parameters.AddWithValue("@LoopPageFlipping", true); // 是否循环翻页
        insertConventionCommand.ExecuteNonQuery(); // 执行插入命令
    }

    // 初始化OpenMainWindow 表
    private void InitializeOpenMainWindow()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        // 创建 OpenMainWindow 表
        string createOpenMainWindowTableQuery = @"
        CREATE TABLE IF NOT EXISTS OpenMainWindow
        (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            OpenMainWindowByMiddleMouseClick BOOL,
            OpenMainWindowByX1MouseClick BOOL,
            OpenMainWindowByX2MouseClick BOOL,
            OpenMainWindowByCtrl_MiddleMouseClick BOOL,
            OpenMainWindowByCtrl_RightMouseClick BOOL,
            OpenMainWindowByMiddleMouseClickLonger BOOL,
            OpenMainWindowByRightMouseClickLonger BOOL,
            OpenMainWindowByRightMouseClick_Move BOOL,
            OpenMainWindowByCtrl BOOL,
            WindowStartupLocation INT
        );"; // 创建 OpenMainWindow 表
        using var createOpenMainWindowCommand = new SQLiteCommand(createOpenMainWindowTableQuery, connection); // 创建 SQLiteCommand 对象
        createOpenMainWindowCommand.ExecuteNonQuery(); // 执行创建表的命令
        InsertDefaultOpenMainWindowData(); // 插入默认数据
    }

    // 插入默认数据
    private void InsertDefaultOpenMainWindowData()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string insertOpenMainWindowQuery = @"
            INSERT INTO OpenMainWindow 
            (OpenMainWindowByMiddleMouseClick, OpenMainWindowByX1MouseClick, OpenMainWindowByX2MouseClick, OpenMainWindowByCtrl_MiddleMouseClick, OpenMainWindowByCtrl_RightMouseClick, OpenMainWindowByMiddleMouseClickLonger, OpenMainWindowByRightMouseClickLonger, OpenMainWindowByRightMouseClick_Move, OpenMainWindowByCtrl, WindowStartupLocation)
            VALUES 
            (@OpenMainWindowByMiddleMouseClick, @OpenMainWindowByX1MouseClick, @OpenMainWindowByX2MouseClick, @OpenMainWindowByCtrl_MiddleMouseClick, @OpenMainWindowByCtrl_RightMouseClick, @OpenMainWindowByMiddleMouseClickLonger, @OpenMainWindowByRightMouseClickLonger, @OpenMainWindowByRightMouseClick_Move, @OpenMainWindowByCtrl, @WindowStartupLocation);";
        using var insertOpenMainWindowCommand = new SQLiteCommand(insertOpenMainWindowQuery, connection); // 创建 SQLiteCommand 对象
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByMiddleMouseClick", false); // 按下中键
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByX1MouseClick", false); // 按下X1键
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByX2MouseClick", false); // 按下X2键
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByCtrl_MiddleMouseClick", false); // Ctrl+中键单击
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByCtrl_RightMouseClick", false); // Ctrl+右键单击
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByMiddleMouseClickLonger", false); // 长按中键
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByRightMouseClickLonger", false); // 长按右键
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByRightMouseClick_Move", false); // 按右键移动
        insertOpenMainWindowCommand.Parameters.AddWithValue("@OpenMainWindowByCtrl", true); // 单击Ctrl键
        insertOpenMainWindowCommand.Parameters.AddWithValue("@WindowStartupLocation", 2); // 功能面板打开位置
        insertOpenMainWindowCommand.ExecuteNonQuery(); // 执行插入命令
    }

    // 初始化 Blacklist 表
    public void InitializeBlacklist()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string createBlacklistTableQuery = @"
        CREATE TABLE IF NOT EXISTS Blacklist
        (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            IsFullScreenDisabled BOOL,
            IsBlacklistEnabledForExtendedHotkey BOOL
        );"; // 创建 Blacklist 表
        using var createBlacklistCommand = new SQLiteCommand(createBlacklistTableQuery, connection); // 创建 SQLiteCommand 对象
        createBlacklistCommand.ExecuteNonQuery(); // 执行创建表的命令
        InsertDefaultBlacklistData(); // 插入默认数据
    }

    // 插入默认数据
    private void InsertDefaultBlacklistData()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string insertBlacklistQuery = @"
            INSERT INTO Blacklist 
            (IsFullScreenDisabled, IsBlacklistEnabledForExtendedHotkey) 
            VALUES 
            (@IsFullScreenDisabled, @IsBlacklistEnabledForExtendedHotkey);";
        using var insertBlacklistCommand = new SQLiteCommand(insertBlacklistQuery, connection); // 创建 SQLiteCommand 对象
        insertBlacklistCommand.Parameters.AddWithValue("@IsFullScreenDisabled", false); // 是否开启全屏或最大化禁用功能
        insertBlacklistCommand.Parameters.AddWithValue("@IsBlacklistEnabledForExtendedHotkey", false); // 是否将黑名单与全屏禁用设置应用于扩展热键功能
        insertBlacklistCommand.ExecuteNonQuery(); // 执行插入命令
    }

    // 初始化 BlacklistApplication 表
    public void InitializeBlacklistApplication()
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string createBlacklistApplicationTableQuery = @"
        CREATE TABLE IF NOT EXISTS BlacklistApplication
        (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            ApplicationName TEXT,
            ProcessName TEXT,
            IsInBlacklist BOOL,
            IsFolder BOOL
        );"; // 创建 BlacklistApplication 表
        using var createBlacklistApplicationCommand = new SQLiteCommand(createBlacklistApplicationTableQuery, connection); // 创建 SQLiteCommand 对象
        createBlacklistApplicationCommand.ExecuteNonQuery(); // 执行创建表的命令
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
    public void ApplyConventionSettings(bool autostart, bool shownotification, bool showaddimage, bool hideTooltip, int longPressThreshold, int mouseMovePixels, bool loopPageFlipping)
    {
        using var connection = OpenConnection(); // 打开数据库连接
        using var command = new SQLiteCommand(@"
        UPDATE Convention SET 
            AutoStart = @AutoStart, 
            ShowNotification = @ShowNotification, 
            ShowAddImage = @ShowAddImage, 
            HideTooltip = @HideTooltip,
            LongPressThreshold = @LongPressThreshold,
            MouseMovePixels = @MouseMovePixels,
            LoopPageFlipping = @LoopPageFlipping
        WHERE ID = 1;", connection); // 创建 SQLiteCommand 对象
        command.Parameters.AddWithValue("@AutoStart", autostart); // 是否开机自启
        command.Parameters.AddWithValue("@ShowNotification", shownotification); // 是否显示通知
        command.Parameters.AddWithValue("@ShowAddImage", showaddimage); // 是否显示添加图片
        command.Parameters.AddWithValue("@HideTooltip", hideTooltip); // 是否隐藏提示
        command.Parameters.AddWithValue("@LongPressThreshold", longPressThreshold); // 长按阈值
        command.Parameters.AddWithValue("@MouseMovePixels", mouseMovePixels); // 鼠标移动像素
        command.Parameters.AddWithValue("@LoopPageFlipping", loopPageFlipping); // 是否循环翻页
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
    public void ApplyOpenMainWindowSettings(bool OpenMainWindowByMiddleMouseClick, bool OpenMainWindowByX1MouseClick, bool OpenMainWindowByX2MouseClick, bool OpenMainWindowByCtrl_MiddleMouseClick, bool OpenMainWindowByCtrl_RightMouseClick, bool OpenMainWindowByMiddleMouseClickLonger, bool OpenMainWindowByRightMouseClickLonger, bool OpenMainWindowByRightMouseClick_Move, bool OpenMainWindowByCtrl, int windowStartupLocation)
    {
        using var connection = OpenConnection(); // 打开数据库连接
        using var command = new SQLiteCommand(@"
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
        WHERE ID = 1;", connection); // 创建 SQLiteCommand 对象
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
    }

    /// <summary>
    /// 更新Blacklist设置信息
    /// </summary>
    /// <param name="isFullScreenDisabled"> 是否开启全屏或最大化禁用功能 </param>
    /// <param name="isBlacklistEnabledForExtendedHotkey"> 是否将黑名单与全屏禁用设置应用于扩展热键功能 </param>
    public void ApplyBlacklistSettings(bool isFullScreenDisabled, bool isBlacklistEnabledForExtendedHotkey)
    {
        using var connection = OpenConnection(); // 打开数据库连接
        using var command = new SQLiteCommand(@"
        UPDATE Blacklist SET 
            IsFullScreenDisabled = @IsFullScreenDisabled,
            IsBlacklistEnabledForExtendedHotkey = @IsBlacklistEnabledForExtendedHotkey
        WHERE ID = 1;", connection); // 创建 SQLiteCommand 对象
        command.Parameters.AddWithValue("@IsFullScreenDisabled", isFullScreenDisabled); // 是否开启全屏或最大化禁用功能
        command.Parameters.AddWithValue("@IsBlacklistEnabledForExtendedHotkey", isBlacklistEnabledForExtendedHotkey); // 是否将黑名单与全屏禁用设置应用于扩展热键功能
        command.ExecuteNonQuery(); // 执行更新命令
    }

    /// <summary>
    /// 添加黑名单应用
    /// </summary>
    /// <param name="applicationName"> 应用名称 </param>
    /// <param name="processName"> 进程名称 </param>
    /// <param name="isInBlacklist"> 是否在黑名单中 </param>
    /// <param name="isFolder"> 是否是文件夹 </param>
    public void ApplyBlacklistApplication(string applicationName, string processName, bool isInBlacklist, bool isFolder)
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string insertQuery = @"INSERT INTO BlacklistApplication 
            (ApplicationName, ProcessName, IsInBlacklist, IsFolder)
            VALUES 
            (@ApplicationName, @ProcessName, @IsInBlacklist, @IsFolder);";
        using var command = new SQLiteCommand(insertQuery, connection); // 创建 SQLiteCommand 对象
        command.Parameters.AddWithValue("@ApplicationName", applicationName); // 应用名称
        command.Parameters.AddWithValue("@ProcessName", processName); // 进程名称
        command.Parameters.AddWithValue("@IsInBlacklist", isInBlacklist); // 是否在黑名单中
        command.Parameters.AddWithValue("@IsFolder", isFolder); // 是否是文件夹
        command.ExecuteNonQuery(); // 执行插入命令
    }

    /// <summary>
    /// 通过应用名称删除黑名单应用
    /// </summary>
    /// <param name="applicationName"> 应用名称 </param>
    public void DeleteBlacklistApplication(string applicationName)
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string deleteQuery = "DELETE FROM BlacklistApplication WHERE ApplicationName = @ApplicationName;"; // 删除黑名单应用
        using var command = new SQLiteCommand(deleteQuery, connection); // 创建 SQLiteCommand 对象
        command.Parameters.AddWithValue("@ApplicationName", applicationName); // 设置参数
        command.ExecuteNonQuery(); // 执行删除命令
    }

    /// <summary>
    /// 保存总使用时长
    /// </summary>
    /// <param name="totalUsageTime"> 总使用时长 </param>
    public void SaveTotalUsageTime(double totalUsageTime)
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string updateQuery = "UPDATE Convention SET TotalUsageTime = @TotalUsageTime WHERE ID = 1;"; // 更新总使用时长
        using var command = new SQLiteCommand(updateQuery, connection); // 创建 SQLiteCommand 对象
        command.Parameters.AddWithValue("@TotalUsageTime", totalUsageTime); // 设置参数
        command.ExecuteNonQuery(); // 执行更新命令
    }

    /// <summary>
    /// 获取常规设置信息
    /// </summary>
    /// <returns> Convention 类 </returns>
    public List<Convention> GetAllConventions()
    {
        var conventions = new List<Convention>(); // 创建一个空的 Convention 列表
        using var connection = OpenConnection(); // 打开数据库连接
        string selectQuery = "SELECT * FROM Convention;"; // 查询所有 Convention 数据
        using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
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
                LoopPageFlipping = reader.GetBoolean(9) // 是否循环翻页
            }); // 将读取到的数据添加到列表中
        }
        return conventions; // 返回所有 Convention 数据
    }

    /// <summary>
    /// 获取OpenMainWindow设置信息
    /// </summary>
    /// <returns> OpenMainWindow 类 </returns>
    public List<OpenMainWindow> GetAllOpenMainWindowConditions()
    {
        var conditions = new List<OpenMainWindow>(); // 创建一个空的 OpenMainWindow 列表
        using var connection = OpenConnection(); // 打开数据库连接
        string selectQuery = "SELECT * FROM OpenMainWindow;"; // 查询所有 OpenMainWindow 数据
        using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
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
        return conditions; // 返回所有 OpenMainWindow 数据
    }

    /// <summary>
    /// 获取黑名单设置
    /// </summary>
    /// <returns> Blacklist 类 </returns>
    public List<Blacklist> GetAllBlacklistSettings()
    {
        var blacklists = new List<Blacklist>(); // 创建一个空的 Blacklist 列表
        using var connection = OpenConnection(); // 打开数据库连接
        string selectQuery = "SELECT * FROM Blacklist;"; // 查询所有 Blacklist 数据
        using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
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
        return blacklists; // 返回所有 Blacklist 数据
    }

    /// <summary>
    /// 获取黑名单应用
    /// </summary>
    /// <returns> BlacklistApplication 类 </returns>
    public List<BlacklistApplication> GetAllBlacklistApplications()
    {
        var applications = new List<BlacklistApplication>(); // 创建一个空的 BlacklistApplication 列表
        using var connection = OpenConnection(); // 打开数据库连接
        string selectQuery = "SELECT * FROM BlacklistApplication;"; // 查询所有 BlacklistApplication 数据
        using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
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
        return applications; // 返回所有 BlacklistApplication 数据
    }

    /// <summary>
    /// 打开数据库连接
    /// </summary>
    /// <returns> SQLiteConnection 对象 </returns>
    public SQLiteConnection OpenConnection()
    {
        var connection = new SQLiteConnection(db1); // 创建 SQLiteConnection 对象
        connection.Open(); // 打开数据库连接
        return connection; // 返回打开的连接
    }
}

// 基础设置
public class Convention
{
    public int ID { get; set; } // 主键
    public string Version { get; set; } // 版本号
    public bool AutoStart { get; set; } // 是否开机自启
    public bool ShowNotification { get; set; } // 是否显示通知
    public bool ShowAddImage { get; set; } // 是否显示添加图片
    public double TotalUsageTime { get; set; } // 总使用时长
    public bool HideTooltip { get; set; } // 是否隐藏提示
    public int LongPressThreshold { get; set; } // 长按阈值
    public int MouseMovePixels { get; set; } // 鼠标移动像素
    public bool LoopPageFlipping { get; set; } // 是否循环翻页
}

// 打开主窗口的条件
public class OpenMainWindow
{
    public int ID { get; set; } // 主键
    public bool OpenMainWindowByMiddleMouseClick { get; set; } // 按下中键
    public bool OpenMainWindowByX1MouseClick { get; set; } // 按下X1键
    public bool OpenMainWindowByX2MouseClick { get; set; } // 按下X2键
    public bool OpenMainWindowByCtrl_MiddleMouseClick { get; set; } // Ctrl+中键单击
    public bool OpenMainWindowByCtrl_RightMouseClick { get; set; } // Ctrl+右键单击
    public bool OpenMainWindowByMiddleMouseClickLonger { get; set; } // 长按中键
    public bool OpenMainWindowByRightMouseClickLonger { get; set; } // 长按右键
    public bool OpenMainWindowByRightMouseClick_Move { get; set; } // 按右键移动
    public bool OpenMainWindowByCtrl { get; set; } // 单击Ctrl键
    public int WindowStartupLocation { get; set; } // 功能面板打开位置
}

// 黑名单设置
public class Blacklist
{
    public int ID { get; set; } // 主键
    public bool IsFullScreenDisabled { get; set; } // 是否开启全屏或最大化禁用功能
    public bool IsBlacklistEnabledForExtendedHotkey { get; set; } // 是否将黑名单与全屏禁用设置应用于扩展热键功能
}

// 黑名单应用
public class BlacklistApplication
{
    /* ApplicationName 与 ProcessName 字段的含义如下：
     * ApplicationName: 黑名单列表显示的文字，可以是文件夹路径，也可以是应用程序名称。
     * ProcessName: 确切的应用程序进程名称，应用程序的可执行文件名称。
     * 
     * 一个 ApplicationName 可以对应多个 ProcessName，例如，一个文件夹路径可以对应多个应用程序的进程名称。
     * 但是一个 ProcessName 只能对应一个 ApplicationName。
     */
    public int ID { get; set; } // 主键
    public string ApplicationName { get; set; } // 应用程序名称
    public string ProcessName { get; set; } // 进程名称
    public bool IsInBlacklist { get; set; } // 是否在黑名单中
    public bool IsFolder { get; set; } // 是否是文件夹
}

// 外观设置
public class Appearance
{
    public double ButtonSize { get; set; } // 按钮大小
    public double ButtonGap { get; set; } // 按钮间隙
    public double BorderWidth { get; set; } // 边框宽度
    public double ButtonCornerRadius { get; set; } // 按钮圆角
}