using Microsoft.Toolkit.Uwp.Notifications;
using System.Data.SQLite;
using Quicker.Database;
using System.IO;

namespace Quicker.Managers
{
    internal class UpdateManager
    {
        private readonly SettingDatabase db1 = new SettingDatabase(); // 设置数据库
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库
        private readonly string currentVersion = "2.1.3"; // 当前版本号

        public UpdateManager()
        {
            CheckAndUpgradeDatabase(); // 检查并更新数据库
        }

        // 检查并更新数据库
        private void CheckAndUpgradeDatabase()
        {
            if (CurrentVertion() != currentVersion) UpdateDatabase(); // 如果当前数据库版本不是最新，更新数据库
        }

        /// <summary>
        /// 获取当前应用版本号
        /// </summary>
        /// <returns> 当前版本号 </returns>
        private string CurrentVertion()
        {
            var connection = db1.OpenConnection(); // 打开数据库连接
            string selectVersionQuery = "SELECT Version FROM Convention ORDER BY ID DESC LIMIT 1;"; // 查询版本号
            using var command = new SQLiteCommand(selectVersionQuery, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询命令
            if (reader.Read()) // 检查是否有数据
                return reader.GetString(0); // 如果有数据，返回版本号
            return null; // 如果没有数据，则返回null
        }

        private void SetCurrentVersion(SQLiteConnection connection, string currentVersion)
        {
            string updateVersionQuery = @$"UPDATE Convention SET Version = '{currentVersion}';"; // 设置默认值
            using var updateVersionCommand = new SQLiteCommand(updateVersionQuery, connection); // 创建 SQLiteCommand 对象
            updateVersionCommand.ExecuteNonQuery(); // 执行更新命令
        }

        private bool ExistSettingDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
            string dbFilePath = Path.Combine(dbFolder, "Setting.db"); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        private bool ExistButtonDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
            string dbFilePath = Path.Combine(dbFolder, "Button.db"); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        private bool ExistActionPageDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
            string dbFilePath = Path.Combine(dbFolder, "ActionPage.db"); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        // 更新数据库
        private void UpdateDatabase()
        {
            switch (CurrentVertion())
            {
                case "2.1.4":
                    UpdateFrom2_1_3To2_1_4(); // 数据库版本从2.1.3升级到2.1.4
                    break; // 数据库版本从2.1.3升级到2.1.4
                case "2.1.3":
                    UpdateFrom2_1_2To2_1_3(); // 数据库版本从2.1.2升级到2.1.3
                    break; // 数据库版本从2.1.2升级到2.1.3
                case "2.1.2":
                    UpdateFrom2_1_1To2_1_2(); // 数据库版本从2.1.1升级到2.1.2
                    break; // 数据库版本从2.1.1升级到2.1.2
                case "2.1.1":
                    UpdateFrom2_1_0To2_1_1(); // 数据库版本从2.1.0升级到2.1.1
                    break; // 数据库版本从2.1.0升级到2.1.1
                default:
                    UpdateTo2_1_0(); // 数据库版本升级到2.1.0
                    break; // 数据库版本升级到2.1.0
            }
            CheckAndUpgradeDatabase(); // 递归检查并更新数据库
        }

        // 数据库版本从2.1.3升级到2.1.4
        private void UpdateFrom2_1_3To2_1_4()
        {

        }

        // 数据库版本从2.1.2升级到2.1.3
        private void UpdateFrom2_1_2To2_1_3()
        {
            using var connection = db1.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                SetCurrentVersion(connection, "2.1.3"); // 设置数据库版本为2.1.3
                if (ExistButtonDatabase()) // 如果存在按钮数据库
                { // 重命名表格中的列名
                    RenameColumn("Global");
                    RenameColumn("Common");
                    if (db2.TableExists("Desktop")) RenameColumn("Desktop");
                    if (db2.TableExists("Taskbar")) RenameColumn("Taskbar");
                }
                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
            }
        }

        /// <summary>
        /// 重命名表格中的列名
        /// </summary>
        /// <param name="tableName"></param>
        public void RenameColumn(string tableName)
        {
            var connection = db2.OpenConnection(); // 打开数据库连接

            // 重命名第一个列：Location → Path
            string renameQuery1 = $"ALTER TABLE {tableName} RENAME COLUMN ButtonName TO Title;";
            using var command1 = new SQLiteCommand(renameQuery1, connection);
            command1.ExecuteNonQuery();

            // 重命名第二个列：Type → ActionType
            string renameQuery2 = $"ALTER TABLE {tableName} RENAME COLUMN Type TO ActionType;";
            using var command2 = new SQLiteCommand(renameQuery2, connection);
            command2.ExecuteNonQuery();

            // 重命名第三个列：Usage → Description
            string renameQuery3 = $"ALTER TABLE {tableName} RENAME COLUMN Usage TO Description;";
            using var command3 = new SQLiteCommand(renameQuery3, connection);
            command3.ExecuteNonQuery();

            connection.Close(); // 关闭数据库连接
        }

        // 数据库版本从2.1.1升级到2.1.2
        private void UpdateFrom2_1_1To2_1_2()
        {
            using var connection = db1.OpenConnection(); // 打开数据库连接
            SetCurrentVersion(connection, "2.1.2"); // 设置数据库版本为2.1.3
        }

        // 数据库版本从2.1.0升级到2.1.1
        private void UpdateFrom2_1_0To2_1_1()
        {
            using var connection = db1.OpenConnection(); // 打开数据库连接
            SetCurrentVersion(connection, "2.1.1"); // 设置数据库版本为2.1.2
        }

        // 数据库版本升级到2.1.0
        private void UpdateTo2_1_0()
        {
            if (ExistSettingDatabase()) Update2_1_0SettingDatabase(); // 更新设置数据库
            if (ExistButtonDatabase()) Update2_1_0ButtonDatabase(); // 将旧表中的所有按钮迁移到对应的新表并删除旧表
        }

        /// <summary>
        /// 更新设置数据库
        /// </summary>
        private void Update2_1_0SettingDatabase()
        {
            using var connection = db1.OpenConnection(); // 打开数据库连接

            // 创建一个新表，将 Version 字段放在 AutoStart 之前
            string createNewTableQuery = @"
                CREATE TABLE IF NOT EXISTS ConventionTemp
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
                );";
            using var createNewTableCommand = new SQLiteCommand(createNewTableQuery, connection);
            createNewTableCommand.ExecuteNonQuery();

            // 将旧表的数据复制到新表
            string insertOldDataQuery = @"INSERT INTO ConventionTemp 
                (ID, AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping)
                SELECT ID, AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping
                FROM Convention;";
            using var insertOldDataCommand = new SQLiteCommand(insertOldDataQuery, connection);
            insertOldDataCommand.ExecuteNonQuery();

            // 删除旧表
            string dropOldTableQuery = "DROP TABLE Convention;";
            using var dropOldTableCommand = new SQLiteCommand(dropOldTableQuery, connection);
            dropOldTableCommand.ExecuteNonQuery();

            // 将新表重命名为旧表的名称
            string renameNewTableQuery = "ALTER TABLE ConventionTemp RENAME TO Convention;";
            using var renameNewTableCommand = new SQLiteCommand(renameNewTableQuery, connection);
            renameNewTableCommand.ExecuteNonQuery();

            // 初始化 Version 字段
            SetCurrentVersion(connection, "2.1.0"); // 设置数据库版本为2.1.1

            db1.InitializeBlacklist(); // 初始化 Blacklist 表
            db1.InitializeBlacklistApplication(); // 初始化 BlacklistApplication 表
        }

        /// <summary>
        /// 将旧表中的所有按钮迁移到对应的新表并删除旧表
        /// </summary>
        private void Update2_1_0ButtonDatabase()
        {
            try
            {
                // 创建一个新的数据库连接
                var connection = db2.OpenConnection();
                using var transaction = connection.BeginTransaction(); // 开始事务
                try
                {
                    // 检测数据库中是否存在 ButtonData 表
                    using var checkCommand = new SQLiteCommand(
                        "SELECT name FROM sqlite_master WHERE type='table' AND name='ButtonData'",
                        connection);
                    using var checkReader = checkCommand.ExecuteReader();
                    if (!checkReader.Read()) return; // 如果不存在，则直接返回

                    // 将旧表重命名为临时表
                    using var renameCommand = new SQLiteCommand("ALTER TABLE ButtonData RENAME TO Temp_ButtonData", connection);
                    renameCommand.ExecuteNonQuery();

                    // 获取旧表ButtonData中的所有按钮数据
                    var oldButtonData = new List<ButtonData>();
                    using var oldCommand = new SQLiteCommand("SELECT * FROM Temp_ButtonData", connection);
                    using var oldReader = oldCommand.ExecuteReader();
                    while (oldReader.Read())
                    {
                        oldButtonData.Add(new ButtonData
                        {
                            ButtonID = oldReader.GetString(0),
                            Title = oldReader.GetString(1),
                            Location = oldReader.GetString(2),
                            ImagePath = oldReader.GetString(3),
                            RunByMessager = oldReader.GetBoolean(4),
                            TryToOpenExitingWindow = oldReader.GetBoolean(5),
                            WindowState = oldReader.GetInt32(6),
                            Description = oldReader.GetString(7),
                            CreateTime = oldReader.GetDateTime(8),
                            LatestEditTime = oldReader.GetDateTime(9),
                            ActionType = "OpenFile"
                        });
                    }

                    // 将每个按钮数据迁移到对应的新表中
                    foreach (var buttonData in oldButtonData)
                    {
                        string tableName = db2.GetTableNameFromButtonID(buttonData.ButtonID); // 从ButtonID解析表名
                        db2.CheckAndCreateTable(tableName, connection); // 检查表是否存在，不存在则创建

                        string insertQuery = $@"INSERT INTO {tableName} 
                        (ButtonID, ButtonName, Location, ImagePath, RunByMessager, TryToOpenExitingWindow, WindowState, Usage, CreateTime, LatestEditTime, Type) 
                        VALUES 
                        (@ButtonID, @ButtonName, @Location, @ImagePath, @RunByMessager, @TryToOpenExitingWindow, @WindowState, @Usage, @CreateTime, @LatestEditTime, @Type)";
                        using var insertCommand = new SQLiteCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID);
                        insertCommand.Parameters.AddWithValue("@ButtonName", buttonData.Title);
                        insertCommand.Parameters.AddWithValue("@Location", buttonData.Location);
                        insertCommand.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath);
                        insertCommand.Parameters.AddWithValue("@RunByMessager", buttonData.RunByMessager);
                        insertCommand.Parameters.AddWithValue("@TryToOpenExitingWindow", buttonData.TryToOpenExitingWindow);
                        insertCommand.Parameters.AddWithValue("@WindowState", buttonData.WindowState);
                        insertCommand.Parameters.AddWithValue("@Usage", buttonData.Description);
                        insertCommand.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime);
                        insertCommand.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime);
                        insertCommand.Parameters.AddWithValue("@Type", buttonData.ActionType);

                        insertCommand.ExecuteNonQuery(); // 执行插入语句
                    }

                    // 删除临时表
                    using var dropCommand = new SQLiteCommand("DROP TABLE Temp_ButtonData", connection);
                    dropCommand.ExecuteNonQuery();

                    transaction.Commit(); // 提交事务
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // 回滚事务
                    new ToastContentBuilder().AddText("数据库更新失败，请删除数据库后重试。").Show(); // 弹出消息提醒
                }
                finally
                {
                    // 确保连接被关闭和释放
                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            catch
            {
                new ToastContentBuilder().AddText("数据库更新失败，请删除数据库后重试。").Show(); // 弹出消息提醒
            }
        }
    }
}