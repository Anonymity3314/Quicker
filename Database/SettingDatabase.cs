using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

// SQLite数据库操作类
public class SettingDatabase
{
    private readonly string dbPath1 = "Data Source=Setting.db;Pooling=true;Max Pool Size=100;";

    // 初始化数据库
    public void Initialize()
    {
        if (File.Exists("Setting.db")) return; // 如果数据库文件存在，则直接返回
        SQLiteConnection.CreateFile("Setting.db"); // 创建数据库文件

        using var connection = OpenConnection(); // 打开数据库连接

        // 创建 Convention 表
        string createConventionTableQuery = @"
        CREATE TABLE IF NOT EXISTS Convention
        (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
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

        // 插入初始数据
        string insertConventionQuery = @"
            INSERT INTO Convention 
            (AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping) 
            VALUES 
            (@AutoStart, @ShowNotification, @ShowAddImage, @TotalUsageTime, @HideTooltip, @LongPressThreshold, @MouseMovePixels, @LoopPageFlipping);";
        using var insertConventionCommand = new SQLiteCommand(insertConventionQuery, connection); // 创建 SQLiteCommand 对象
        insertConventionCommand.Parameters.AddWithValue("@AutoStart", false); // 是否开机自启
        insertConventionCommand.Parameters.AddWithValue("@ShowNotification", true); // 是否显示通知
        insertConventionCommand.Parameters.AddWithValue("@ShowAddImage", true); // 是否显示添加图片
        insertConventionCommand.Parameters.AddWithValue("@TotalUsageTime", 0.0); // 总使用时长
        insertConventionCommand.Parameters.AddWithValue("@HideTooltip", false); // 是否隐藏提示
        insertConventionCommand.Parameters.AddWithValue("@LongPressThreshold", 300); // 长按阈值
        insertConventionCommand.Parameters.AddWithValue("@MouseMovePixels", 50); // 鼠标移动像素
        insertConventionCommand.Parameters.AddWithValue("@LoopPageFlipping", true); // 是否循环翻页
        insertConventionCommand.ExecuteNonQuery(); // 执行插入命令

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

        // 插入初始数据
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

    // 更新设置信息
    public void ApplySettings(bool autostart, bool shownotification, bool showaddimage, bool hideTooltip,int longPressThreshold, int mouseMovePixels, bool loopPageFlipping, bool OpenMainWindowByMiddleMouseClick, bool OpenMainWindowByX1MouseClick, bool OpenMainWindowByX2MouseClick, bool OpenMainWindowByCtrl_MiddleMouseClick, bool OpenMainWindowByCtrl_RightMouseClick, bool OpenMainWindowByMiddleMouseClickLonger, bool OpenMainWindowByRightMouseClickLonger, bool OpenMainWindowByRightMouseClick_Move, bool OpenMainWindowByCtrl, int windowStartupLocation)
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
        WHERE ID = 1;
        
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
        command.Parameters.AddWithValue("@AutoStart", autostart); // 是否开机自启
        command.Parameters.AddWithValue("@ShowNotification", shownotification); // 是否显示通知
        command.Parameters.AddWithValue("@ShowAddImage", showaddimage); // 是否显示添加图片
        command.Parameters.AddWithValue("@HideTooltip", hideTooltip); // 是否隐藏提示
        command.Parameters.AddWithValue("@LongPressThreshold", longPressThreshold); // 长按阈值
        command.Parameters.AddWithValue("@MouseMovePixels", mouseMovePixels); // 鼠标移动像素
        command.Parameters.AddWithValue("@LoopPageFlipping", loopPageFlipping); // 是否循环翻页
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

    // 保存总使用时长
    public void SaveTotalUsageTime(double totalUsageTime)
    {
        using var connection = OpenConnection(); // 打开数据库连接
        string updateQuery = "UPDATE Convention SET TotalUsageTime = @TotalUsageTime WHERE ID = 1;"; // 更新总使用时长
        using var command = new SQLiteCommand(updateQuery, connection); // 创建 SQLiteCommand 对象
        command.Parameters.AddWithValue("@TotalUsageTime", totalUsageTime); // 设置参数
        command.ExecuteNonQuery(); // 执行更新命令
    }

    // 获取设置信息
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
                AutoStart = reader.GetBoolean(1), // 是否开机自启
                ShowNotification = reader.GetBoolean(2), // 是否显示通知
                ShowAddImage = reader.GetBoolean(3), // 是否显示添加图片
                TotalUsageTime = reader.GetDouble(4), // 总使用时长
                HideTooltip = reader.GetBoolean(5), // 是否隐藏提示
                LongPressThreshold = reader.GetInt32(6), // 长按阈值
                MouseMovePixels = reader.GetInt32(7), // 鼠标移动像素
                LoopPageFlipping = reader.GetBoolean(8) // 是否循环翻页
            }); // 将读取到的数据添加到列表中
        }
        return conventions; // 返回所有 Convention 数据
    }

    // 获取OpenMainWindow设置信息
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

    // 打开数据库连接
    private SQLiteConnection OpenConnection()
    {
        var connection = new SQLiteConnection(dbPath1); // 创建 SQLiteConnection 对象
        connection.Open(); // 打开数据库连接
        return connection; // 返回打开的连接
    }
}

// 基础设置
public class Convention
{
    public int ID { get; set; } // 主键
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

// 外观设置
public class Appearance
{

}