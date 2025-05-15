using Microsoft.Toolkit.Uwp.Notifications;
using System.Data.SQLite;
using Quicker.Database;
using System.Windows;
using System.IO;

namespace Quicker.Managers
{
    internal class DatabaseUpdateManager
    {
        private readonly SettingDatabase db1 = new SettingDatabase(); // 设置数据库
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库

        public DatabaseUpdateManager()
        {
            CheckAndUpgradeDatabase(); // 检查并更新数据库
        }

        // 检查并更新数据库
        private void CheckAndUpgradeDatabase()
        {
            string dbVersion = GetCurrentVersion(); // 获取当前数据库版本号
            if (dbVersion != db1.currentVersion)
                UpdateDatabase(dbVersion); // 数据库版本不同，更新数据库
        }

        /// <summary>
        /// 获取当前数据库版本号
        /// </summary>
        /// <returns> 当前版本号 </returns>
        private string GetCurrentVersion()
        {
            using var connection = db1.OpenConnection(); // 打开数据库连接
            string selectVersionQuery = "SELECT Version FROM Convention ORDER BY ID DESC LIMIT 1;"; // 查询版本号
            using var command = new SQLiteCommand(selectVersionQuery, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询命令
            if (reader.Read()) // 检查是否有数据
                return reader.GetString(0); // 如果有数据，返回版本号
            return null; // 如果没有数据，则返回null
        }

        /// <summary>
        /// 设置数据库版本号
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="version"> 版本号 </param>
        private void SetCurrentVersion(string version)
        {
            var connection = db1.OpenConnection(); // 打开数据库连接
            string updateVersionQuery = @$"UPDATE Convention SET Version = '{version}';"; // 设置默认值
            using var updateVersionCommand = new SQLiteCommand(updateVersionQuery, connection); // 创建 SQLiteCommand 对象
            updateVersionCommand.ExecuteNonQuery(); // 执行更新命令
        }

        /// <summary>
        /// 检查是否存在设置数据库
        /// </summary>
        /// <returns> 是否存在设置数据库 </returns>
        private bool ExistSettingDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
            string dbFilePath = Path.Combine(dbFolder, "Setting.db"); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        /// <summary>
        /// 检查是否存在按钮数据库
        /// </summary>
        /// <returns> 是否存在按钮数据库 </returns>
        private bool ExistButtonDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
            string dbFilePath = Path.Combine(dbFolder, "Button.db"); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        /// <summary>
        /// 检查是否存在动作页面数据库
        /// </summary>
        /// <returns> 是否存在动作页面数据库 </returns>
        private bool ExistActionPageDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取应用程序根目录下的"Database"文件夹
            string dbFilePath = Path.Combine(dbFolder, "ActionPage.db"); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        /// <summary>
        /// 更新数据库
        /// </summary>
        /// <param name="dbVersion"> 当前数据库版本号 </param>
        private void UpdateDatabase(string dbVersion)
        {
            try
            {
                switch (dbVersion)
                {
                    case "2.1.4":
                        break;
                    case "2.1.3":
                        UpdateFrom2_1_3To2_1_4(); // 数据库版本从2.1.3升级到2.1.4
                        break;
                    case "2.1.2":
                        UpdateFrom2_1_2To2_1_3(); // 数据库版本从2.1.2升级到2.1.3
                        break;
                    case "2.1.1":
                        UpdateFrom2_1_1To2_1_2(); // 数据库版本从2.1.1升级到2.1.2
                        break;
                    case "2.1.0":
                        UpdateFrom2_1_0To2_1_1(); // 数据库版本从2.1.0升级到2.1.1
                        break;
                    default:
                        UpdateTo2_1_0(); // 数据库版本升级到2.1.0
                        break;
                }
                CheckAndUpgradeDatabase(); // 递归检查并更新数据库
            }
            catch
            {
                new ToastContentBuilder().AddText("数据库更新失败，请删除数据库文件后重试。").Show(); // 弹出消息提醒用户
            }
        }

        // 数据库版本从2.1.3升级到2.1.4
        private void UpdateFrom2_1_3To2_1_4()
        {
            SetCurrentVersion("2.1.4"); // 设置数据库版本为2.1.4
            if (ExistButtonDatabase()) // 如果存在按钮数据库
            {
                Update2_1_3ButtonDatabase("Global");
                Update2_1_3ButtonDatabase("Common");
                if (db2.TableExists("Desktop")) Update2_1_3ButtonDatabase("Desktop");
                if (db2.TableExists("Taskbar")) Update2_1_3ButtonDatabase("Taskbar");
            }
        }

        /// <summary>
        /// 更新按钮数据库的列名
        /// </summary>
        /// <param name="tableName"> 表名 </param>
        private void Update2_1_3ButtonDatabase(string tableName)
        {
            using var connection = db2.OpenConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                // 创建临时表
                string createTempTableQuery = $@"
                    CREATE TABLE Temp_{tableName}
                    (
                        ButtonID TEXT PRIMARY KEY,
                        Title TEXT,
                        Location TEXT,
                        ImagePath TEXT,
                        Data1 TEXT,
                        Data2 TEXT,
                        Data3 TEXT,
                        Description TEXT,
                        CreateTime DATETIME,
                        LatestEditTime DATETIME,
                        ActionType TEXT
                    );";
                using var createTempTableCommand = new SQLiteCommand(createTempTableQuery, connection);
                createTempTableCommand.ExecuteNonQuery();

                // 将数据从旧表迁移到临时表，转换数据类型
                string migrateDataQuery = $@"
                    INSERT INTO Temp_{tableName} 
                    (ButtonID, Title, Location, ImagePath, Data1, Data2, Data3, Description, CreateTime, LatestEditTime, ActionType)
                    SELECT 
                        ButtonID, 
                        Title, 
                        Location, 
                        ImagePath, 
                        CASE RunByMessager WHEN 1 THEN 'true' ELSE 'false' END AS Data1, 
                        CASE TryToOpenExitingWindow WHEN 1 THEN 'true' ELSE 'false' END AS Data2, 
                        CAST(WindowState AS TEXT) AS Data3, 
                        Description, 
                        CreateTime, 
                        LatestEditTime, 
                        ActionType 
                    FROM {tableName};";
                using var migrateDataCommand = new SQLiteCommand(migrateDataQuery, connection);
                migrateDataCommand.ExecuteNonQuery();

                // 删除旧表
                using var dropCommand = new SQLiteCommand($"DROP TABLE {tableName}", connection);
                dropCommand.ExecuteNonQuery();

                // 将临时表重命名为旧表名
                string renameTempTableQuery = $"ALTER TABLE Temp_{tableName} RENAME TO {tableName};";
                using var renameTempTableCommand = new SQLiteCommand(renameTempTableQuery, connection);
                renameTempTableCommand.ExecuteNonQuery();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
            }
        }

        // 数据库版本从2.1.2升级到2.1.3
        private void UpdateFrom2_1_2To2_1_3()
        {
            SetCurrentVersion("2.1.3"); // 设置数据库版本为2.1.3

            using var connection = db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                MigrateDatabaseFiles(); // 迁移数据库文件
                if (ExistButtonDatabase()) // 如果存在按钮数据库
                {
                    RenameColumn("Global");
                    RenameColumn("Common");
                    if (db2.TableExists("Desktop")) RenameColumn("Desktop");
                    if (db2.TableExists("Taskbar")) RenameColumn("Taskbar");
                } // 重命名表格中的列名
                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
            }
        }

        // 迁移数据库文件到Database文件夹
        private void MigrateDatabaseFiles()
        {
            // 源文件路径
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sourceButtonDbPath = Path.Combine(appDirectory, "Button.db");
            string sourceSettingDbPath = Path.Combine(appDirectory, "Setting.db");

            string dbFolder = Path.Combine(appDirectory, "Database"); // 目标文件夹路径

            // 检查文件是否存在并迁移
            if (File.Exists(sourceButtonDbPath))
                MigrateFile("Button.db", dbFolder); // 迁移按钮数据库
            if (File.Exists(sourceSettingDbPath))
                MigrateFile("Setting.db", dbFolder); // 迁移设置数据库
        }

        /// <summary>
        /// 获取应用程序所在目录的指定文件并迁移至指定位置
        /// </summary>
        /// <param name="fileName">要迁移的文件名</param>
        /// <param name="destinationPath">目标目录路径</param>
        public void MigrateFile(string fileName, string destinationPath)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // 获取应用程序所在目录的路径
            string sourceFilePath = Path.Combine(appDirectory, fileName); // 获取指定文件的完整路径
            string destinationFilePath = Path.Combine(destinationPath, fileName); // 获取目标文件的完整路径
            if (!File.Exists(sourceFilePath)) return; // 文件不存在，则不进行迁移
            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath); // 创建目标目录
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, true); // 迁移文件并覆盖目标文件
                File.Delete(sourceFilePath); // 删除源文件
            }
            catch
            {
                new ToastContentBuilder().AddText("数据库迁移失败，请关闭应用后手动将数据库文件从应用目录迁移到目录下的Database文件夹。").Show(); // 弹出消息提醒用户
            }
        }

        // 数据库版本从2.1.1升级到2.1.2
        private void UpdateFrom2_1_1To2_1_2()
        {
            SetCurrentVersion("2.1.2"); // 设置数据库版本为2.1.2
        }

        // 数据库版本从2.1.0升级到2.1.1
        private void UpdateFrom2_1_0To2_1_1()
        {
            SetCurrentVersion("2.1.1"); // 设置数据库版本为2.1.1
        }

        // 数据库版本升级到2.1.0
        private void UpdateTo2_1_0()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // 获取应用程序所在目录的路径
                string sourceButtonDbPath = Path.Combine(appDirectory, "Button.db"); // 获取旧按钮数据库路径
                string sourceSettingDbPath = Path.Combine(appDirectory, "Setting.db"); // 获取旧设置数据库路径
                if (File.Exists(sourceSettingDbPath)) Update2_1_0SettingDatabase(); // 更新设置数据库
                if (File.Exists(sourceButtonDbPath)) Update2_1_0ButtonDatabase(); // 将旧表中的所有按钮迁移到对应的新表并删除旧表
                SetCurrentVersion("2.1.0"); // 设置数据库版本为2.1.0
            }
            catch
            {
                new ToastContentBuilder().AddText("数据库更新失败，该版本的数据库无法更新，请删除数据库后重试。").Show(); // 弹出消息提醒用户
            }
        }

        /// <summary>
        /// 重命名表格中的列名
        /// </summary>
        /// <param name="tableName"> 表格名 </param>
        public void RenameColumn(string tableName)
        {
            using var connection = db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
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

                transaction.Commit(); // 提交事务
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // 回滚事务
                throw new Exception($"重命名列失败: {tableName}", ex);
            }
            finally
            {
                connection.Close(); // 关闭数据库连接
            }
        }

        /// <summary>
        /// 更新设置数据库
        /// </summary>
        private void Update2_1_0SettingDatabase()
        {
            using var connection = db1.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
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

                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
            }
        }

        /// <summary>
        /// 将旧表中的所有按钮迁移到对应的新表并删除旧表
        /// </summary>
        private void Update2_1_0ButtonDatabase()
        {
            try
            {
                // 创建一个新的数据库连接
                using var connection = db2.OpenConnection();
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
                            Data1 = oldReader.GetString(4),
                            Data2 = oldReader.GetString(5),
                            Data3 = oldReader.GetString(6),
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
                        (ButtonID, ButtonName, Location, ImagePath, Data1, Data2, Data3, Usage, CreateTime, LatestEditTime, Type) 
                        VALUES 
                        (@ButtonID, @ButtonName, @Location, @ImagePath, @Data1, @Data2, @Data3, @Usage, @CreateTime, @LatestEditTime, @Type)";
                        using var insertCommand = new SQLiteCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID);
                        insertCommand.Parameters.AddWithValue("@ButtonName", buttonData.Title);
                        insertCommand.Parameters.AddWithValue("@Location", buttonData.Location);
                        insertCommand.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath);
                        insertCommand.Parameters.AddWithValue("@Data1", buttonData.Data1);
                        insertCommand.Parameters.AddWithValue("@Data2", buttonData.Data2);
                        insertCommand.Parameters.AddWithValue("@Data3", buttonData.Data3);
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
                catch
                {
                    transaction.Rollback(); // 回滚事务
                }
                finally
                {
                    // 确保连接被关闭和释放
                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            catch { }
        }
    }
}