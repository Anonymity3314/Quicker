using System.Data.SQLite;
using System.IO;

namespace Quicker.Database
{
    public class TemporaryDatabase
    {
        // 获取应用程序根目录，并设置数据库文件路径为根目录下的"Database"文件夹
        private readonly string db4 = "Data Source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Temporary.db") + ";Pooling=true;Max Pool Size=100;Journal Mode=Wal;";

        public TemporaryDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
            string dbFilePath = Path.Combine(dbFolder, "Temporary.db"); // 设置数据库文件路径
            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder); // 如果"Database"文件夹不存在，则创建它
            if (!File.Exists(dbFilePath))
            {
                SQLiteConnection.CreateFile(dbFilePath); // 创建数据库文件
                InitializeConvention(); // 初始化 Convention 表
                InitializeOpenMainWindow(); // 初始化 OpenMainWindow 表
                InitializeBlacklist(); // 初始化 Blacklist 表
                InitializeBlacklistApplication(); // 初始化 BlacklistApplication 表
            }
        }

        // 初始化 Convention 表
        private void InitializeConvention()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string createConventionTableQuery = @"
            CREATE TABLE IF NOT EXISTS Convention
            (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                LongPressThreshold INTEGER,
                MouseMovePixels INTEGER
            );"; // 创建 Convention 表
            using var createConventionCommand = new SQLiteCommand(createConventionTableQuery, connection); // 创建 SQLiteCommand 对象
            createConventionCommand.ExecuteNonQuery(); // 执行创建表的命令
        }

        // 初始化OpenMainWindow 表
        private void InitializeOpenMainWindow()
        {
            using var connection = OpenConnection(); // 打开数据库连接
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
        }

        // 初始化 Blacklist 表
        private void InitializeBlacklist()
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
        }

        // 初始化 BlacklistApplication 表
        private void InitializeBlacklistApplication()
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
        /// 插入 Convention 数据
        /// </summary>
        /// <param name="convention"></param>
        public void InsertConvention(Convention convention)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string insertQuery = @"
            INSERT INTO Convention 
            (LongPressThreshold, MouseMovePixels)
            VALUES 
            (@LongPressThreshold, @MouseMovePixels);";
            using var command = new SQLiteCommand(insertQuery, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@LongPressThreshold", convention.LongPressThreshold); // 添加参数
            command.Parameters.AddWithValue("@MouseMovePixels", convention.MouseMovePixels); // 添加参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        /// <summary>
        /// 插入 OpenMainWindow 数据
        /// </summary>
        /// <param name="conditions"></param>
        public void InsertOpenMainWindowConditions(OpenMainWindow conditions)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string insertQuery = @"
            INSERT INTO OpenMainWindow 
            (OpenMainWindowByMiddleMouseClick, OpenMainWindowByX1MouseClick, OpenMainWindowByX2MouseClick, OpenMainWindowByCtrl_MiddleMouseClick, OpenMainWindowByCtrl_RightMouseClick, OpenMainWindowByMiddleMouseClickLonger, OpenMainWindowByRightMouseClickLonger, OpenMainWindowByRightMouseClick_Move, OpenMainWindowByCtrl, WindowStartupLocation)
            VALUES 
            (@OpenMainWindowByMiddleMouseClick, @OpenMainWindowByX1MouseClick, @OpenMainWindowByX2MouseClick, @OpenMainWindowByCtrl_MiddleMouseClick, @OpenMainWindowByCtrl_RightMouseClick, @OpenMainWindowByMiddleMouseClickLonger, @OpenMainWindowByRightMouseClickLonger, @OpenMainWindowByRightMouseClick_Move, @OpenMainWindowByCtrl, @WindowStartupLocation);";
            using var command = new SQLiteCommand(insertQuery, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@OpenMainWindowByMiddleMouseClick", conditions.OpenMainWindowByMiddleMouseClick); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByX1MouseClick", conditions.OpenMainWindowByX1MouseClick); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByX2MouseClick", conditions.OpenMainWindowByX2MouseClick); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByCtrl_MiddleMouseClick", conditions.OpenMainWindowByCtrl_MiddleMouseClick); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByCtrl_RightMouseClick", conditions.OpenMainWindowByCtrl_RightMouseClick); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByMiddleMouseClickLonger", conditions.OpenMainWindowByMiddleMouseClickLonger); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByRightMouseClickLonger", conditions.OpenMainWindowByRightMouseClickLonger); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByRightMouseClick_Move", conditions.OpenMainWindowByRightMouseClick_Move); // 添加参数
            command.Parameters.AddWithValue("@OpenMainWindowByCtrl", conditions.OpenMainWindowByCtrl); // 添加参数
            command.Parameters.AddWithValue("@WindowStartupLocation", conditions.WindowStartupLocation); // 添加参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        /// <summary>
        /// 插入 Blacklist 数据
        /// </summary>
        /// <param name="blacklist"></param>
        public void InsertBlacklistSettings(Blacklist blacklist)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string insertQuery = @"
            INSERT INTO Blacklist 
            (IsFullScreenDisabled, IsBlacklistEnabledForExtendedHotkey)
            VALUES 
            (@IsFullScreenDisabled, @IsBlacklistEnabledForExtendedHotkey);"; // 创建 Blacklist 表
            using var command = new SQLiteCommand(insertQuery, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@IsFullScreenDisabled", blacklist.IsFullScreenDisabled); // 添加参数
            command.Parameters.AddWithValue("@IsBlacklistEnabledForExtendedHotkey", blacklist.IsBlacklistEnabledForExtendedHotkey); // 添加参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        /// <summary>
        /// 插入 BlacklistApplication 数据
        /// </summary>
        /// <param name="application"> BlacklistApplication 对象 </param>
        public void InsertBlacklistApplication(BlacklistApplication application)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string insertQuery = @"
            INSERT INTO BlacklistApplication 
            (ApplicationName, ProcessName, IsInBlacklist, IsFolder)
            VALUES 
            (@ApplicationName, @ProcessName, @IsInBlacklist, @IsFolder);"; // 创建 BlacklistApplication 表
            using var command = new SQLiteCommand(insertQuery, connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@ApplicationName", application.ApplicationName); // 添加参数
            command.Parameters.AddWithValue("@ProcessName", application.ProcessName); // 添加参数
            command.Parameters.AddWithValue("@IsInBlacklist", application.IsInBlacklist); // 添加参数
            command.Parameters.AddWithValue("@IsFolder", application.IsFolder); // 添加参数
            command.ExecuteNonQuery(); // 执行插入命令
        }

        /// <summary>
        /// 查询 Convention 数据
        /// </summary>
        /// <returns> Convention 对象 </returns>
        public Convention GetConvention()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string selectQuery = "SELECT * FROM Convention;"; // 创建查询语句
            using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询语句
            if (reader.Read())
            {
                return new Convention
                {
                    LongPressThreshold = reader.GetInt32(1),
                    MouseMovePixels = reader.GetInt32(2),
                };
            }
            return null; // 如果查询结果为空，则返回 null
        }

        /// <summary>
        /// 查询 OpenMainWindow 数据
        /// </summary>
        /// <returns> OpenMainWindow 对象 </returns>
        public OpenMainWindow GetOpenMainWindowConditions()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string selectQuery = "SELECT * FROM OpenMainWindow;"; // 创建查询语句
            using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询语句
            if (reader.Read())
            {
                return new OpenMainWindow
                {
                    ID = reader.GetInt32(0),
                    OpenMainWindowByMiddleMouseClick = reader.GetBoolean(1),
                    OpenMainWindowByX1MouseClick = reader.GetBoolean(2),
                    OpenMainWindowByX2MouseClick = reader.GetBoolean(3),
                    OpenMainWindowByCtrl_MiddleMouseClick = reader.GetBoolean(4),
                    OpenMainWindowByCtrl_RightMouseClick = reader.GetBoolean(5),
                    OpenMainWindowByMiddleMouseClickLonger = reader.GetBoolean(6),
                    OpenMainWindowByRightMouseClickLonger = reader.GetBoolean(7),
                    OpenMainWindowByRightMouseClick_Move = reader.GetBoolean(8),
                    OpenMainWindowByCtrl = reader.GetBoolean(9),
                    WindowStartupLocation = reader.GetInt32(10)
                };
            }
            return null; // 如果查询结果为空，则返回 null
        }

        /// <summary>
        /// 查询 Blacklist 数据
        /// </summary>
        /// <returns> Blacklist 对象 </returns>
        public Blacklist GetBlacklistSettings()
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string selectQuery = "SELECT * FROM Blacklist;"; // 创建查询语句
            using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询语句
            if (reader.Read())
            {
                return new Blacklist
                {
                    ID = reader.GetInt32(0),
                    IsFullScreenDisabled = reader.GetBoolean(1),
                    IsBlacklistEnabledForExtendedHotkey = reader.GetBoolean(2)
                };
            }
            return null; // 如果查询结果为空，则返回 null
        }

        /// <summary>
        /// 查询 BlacklistApplication 数据
        /// </summary>
        /// <returns> List<BlacklistApplication> 对象 </returns>
        public List<BlacklistApplication> GetBlacklistApplications()
        {
            var applications = new List<BlacklistApplication>(); // 创建 List<BlacklistApplication> 对象
            using var connection = OpenConnection(); // 打开数据库连接
            string selectQuery = "SELECT * FROM BlacklistApplication;"; // 创建查询语句
            using var command = new SQLiteCommand(selectQuery, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询语句
            while (reader.Read())
            {
                applications.Add(new BlacklistApplication
                {
                    ID = reader.GetInt32(0),
                    ApplicationName = reader.GetString(1),
                    ProcessName = reader.GetString(2),
                    IsInBlacklist = reader.GetBoolean(3),
                    IsFolder = reader.GetBoolean(4)
                }); // 添加 BlacklistApplication 对象到列表
            }
            return applications; // 返回查询结果
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <returns> SQLiteConnection 对象 </returns>
        public SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(db4); // 创建 SQLiteConnection 对象
            connection.Open(); // 打开数据库连接
            return connection; // 返回打开的连接
        }
    }
}