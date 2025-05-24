using System.Data.SQLite;
using Quicker.Database;
using Quicker.Windows;
using System.IO;

namespace Quicker.Managers
{
    public class DatabaseUpdateManager : IDisposable
    {
        private readonly ButtonDatabase db2 = new(); // 按钮数据库
        private bool _disposed = false; // 标记是否已释放资源

        // 检查并更新数据库
        public void CheckAndUpgradeDatabase()
        {
            string dbVersion = GetCurrentVersion(); // 获取当前数据库版本号
            if (dbVersion != SettingDatabase.currentVersion)
            {
                LoadingWindow loadingWindow = new(); // 显示加载窗口
                loadingWindow.Show(); // 显示加载窗口
                UpdateDatabase(dbVersion); // 数据库版本不同，更新数据库
                loadingWindow?.Close(); // 关闭加载窗口
            }
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
                    case "2.2.0":
                        break;
                    case "2.1.3":
                        UpdateFrom2_1_3To2_2_0(); // 数据库版本从2.1.3升级到2.2.0
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
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("数据库更新失败，请删除数据库文件后重试。", "Error"); // 弹出消息提醒
            }
        }

        /// <summary>
        /// 获取当前数据库版本号
        /// </summary>
        /// <returns> 当前版本号 </returns>
        private string GetCurrentVersion()
        {
            using var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
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
            var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
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

        // 数据库版本从2.1.3升级到2.2.0
        private void UpdateFrom2_1_3To2_2_0()
        {
            SetCurrentVersion("2.2.0"); // 设置数据库版本为2.2.0
            if (ExistButtonDatabase()) // 如果存在按钮数据库
            {
                Update2_1_3ButtonDatabase("Global"); // 更新2.1.3版本按钮数据库的Global表
                Update2_1_3ButtonDatabase("Common"); // 更新2.1.3版本按钮数据库的Common表
                if (db2.TableExists("Desktop")) Update2_1_3ButtonDatabase("Desktop"); // 如果2.1.3版本按钮数据库存在Desktop表，更新Desktop表
                if (db2.TableExists("Taskbar")) Update2_1_3ButtonDatabase("Taskbar"); // 如果2.1.3版本按钮数据库存在Desktop表，更新Taskbar表
            }
            UpdateAllImagePathsToNewFolder(); // 更新所有按钮的 ImagePath 字段
            Update2_1_3SettingDatabase(); // 更新设置数据库
        }

        // 更新按钮数据库中所有按钮的 ImagePath 字段
        /// <summary>
        /// 更新按钮数据库中所有按钮的 ImagePath 字段为新的路径
        /// </summary>
        public void UpdateAllImagePathsToNewFolder()
        {
            string newBasePath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\";

            using var connection = db2.OpenConnection();
            using var transaction = connection.BeginTransaction();

            List<string> tableNames = db2.GetAllTableNames(); // 获取所有表名
            // 遍历每个表并更新 ImagePath
            foreach (string tableName in tableNames)
            {
                string selectQuery = $@"SELECT ButtonID, ImagePath FROM [{tableName}];";
                using var selectCommand = new SQLiteCommand(selectQuery, connection);
                using var selectReader = selectCommand.ExecuteReader();

                List<(int ButtonID, string ImagePath)> entriesToUpdate = new List<(int, string)>();

                while (selectReader.Read())
                {
                    int buttonID = selectReader.GetInt32(0);
                    string imagePath = selectReader.GetString(1);

                    // 提取文件名
                    string fileName = Path.GetFileName(imagePath);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        // 拼接新的路径
                        string newImagePath = Path.Combine(newBasePath, fileName);
                        entriesToUpdate.Add((buttonID, newImagePath));
                    }
                }

                // 更新 ImagePath
                foreach (var (buttonID, newImagePath) in entriesToUpdate)
                {
                    string updateQuery = $@"UPDATE [{tableName}] SET ImagePath = @NewImagePath WHERE ButtonID = @ButtonID;";
                    using var updateCommand = new SQLiteCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@NewImagePath", newImagePath);
                    updateCommand.Parameters.AddWithValue("@ButtonID", buttonID);
                    updateCommand.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }

        /// <summary>
        /// 更新按钮数据库的列名
        /// </summary>
        /// <param name="tableName"> 表名 </param>
        private void Update2_1_3ButtonDatabase(string tableName)
        {
            using var connection = db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                ChangeDataStyle(connection, tableName); // 转换数据类型
                AddNewColumn(tableName); // 为新表添加UsedTimes列
                ChangeButtonIDType(connection); // 转换 ButtonID 类型
                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
            }
        }

        /// <summary>
        /// 转换数据类型
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="tableName"> 表名 </param>
        private void ChangeDataStyle(SQLiteConnection connection, string tableName)
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
                        CASE RunByMessager WHEN 1 THEN 'True' ELSE 'False' END AS Data1, 
                        CASE TryToOpenExitingWindow WHEN 1 THEN 'True' ELSE 'False' END AS Data2, 
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
        }

        /// <summary>
        /// 添加新列：动作使用次数
        /// </summary>
        /// <param name="tableName"> 表名 </param>
        private void AddNewColumn(string tableName)
        {
            using var addConnection = db2.OpenConnection(); // 打开数据库连接
            using var transaction = addConnection.BeginTransaction(); // 开启事务
            string addUsedTimesQuery = @$"
                ALTER TABLE {tableName}
                ADD COLUMN UsedTimes INTEGER DEFAULT 0;"; // 为Convention表添加UsedTimes列
            using var addUsedTimesCommand = new SQLiteCommand(addUsedTimesQuery, addConnection);
            addUsedTimesCommand.ExecuteNonQuery(); // 执行更新命令
            transaction.Commit(); // 提交事务
        }

        /// 转换 ButtonID 类型
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableName">表名（已弃用，保留参数兼容）</param>
        private void ChangeButtonIDType(SQLiteConnection connection)
        {
            var oldTableNames = db2.GetAllTableNames()
                .Where(n => !n.StartsWith("sqlite_")) // 过滤系统表
                .ToList();

            foreach (var oldTableName in oldTableNames)
            {
                // 创建临时表
                var tempTableName = $"Temp_{oldTableName}";
                db2.CreateButtonTable(tempTableName);

                // 迁移数据
                var oldButtonDataList = GetOldDataFromTable(oldTableName);
                foreach (var oldButtonData in oldButtonDataList)
                {
                    // 安全处理可能为空的 ButtonID
                    var buttonId = oldButtonData.ButtonID ?? "";
                    if (buttonId.StartsWith(oldTableName) &&
                        int.TryParse(buttonId.Substring(oldTableName.Length), out int newButtonID))
                    {
                        var newButtonData = new ButtonData
                        {
                            ButtonID = newButtonID,
                            Title = oldButtonData.Title,
                            Location = oldButtonData.Location,
                            ImagePath = oldButtonData.ImagePath,
                            Data1 = oldButtonData.Data1,
                            Data2 = oldButtonData.Data2,
                            Data3 = oldButtonData.Data3,
                            Description = oldButtonData.Description,
                            CreateTime = oldButtonData.CreateTime,
                            LatestEditTime = oldButtonData.LatestEditTime,
                            ActionType = oldButtonData.ActionType,
                            UsedTimes = oldButtonData.UsedTimes // 设置动作使用次数为0
                        };
                        db2.UpdateAction(newButtonData, tempTableName);
                    }
                }

                db2.DeleteButtonTable(oldTableName);
                string renameQuery = $"ALTER TABLE {tempTableName} RENAME TO {oldTableName};";
                new SQLiteCommand(renameQuery, connection).ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 从旧表中获取数据
        /// </summary>
        /// <param name="tableName">旧表名</param>
        /// <returns>旧的 ButtonData 列表</returns>
        private List<ButtonDataBefore2_2_0> GetOldDataFromTable(string tableName)
        {
            var oldButtonDataList = new List<ButtonDataBefore2_2_0>(); // 旧的 ButtonData 列表
            using var connection = db2.OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand($@"SELECT * FROM {tableName}", connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询命令
            while (reader.Read())
            {
                oldButtonDataList.Add(new ButtonDataBefore2_2_0
                {
                    ButtonID = reader.GetString(0), // 动作ID
                    Title = reader.GetString(1), // 动作名称
                    Location = reader.GetString(2), // 位置
                    ImagePath = reader.GetString(3), // 图片路径
                    Data1 = reader.IsDBNull(4) ? null : reader.GetString(4), // 动作数据1
                    Data2 = reader.IsDBNull(5) ? null : reader.GetString(5), // 动作数据2
                    Data3 = reader.IsDBNull(6) ? null : reader.GetString(6), // 动作数据3
                    Description = reader.GetString(7), // 对动作的描述
                    CreateTime = reader.GetDateTime(8), // 创建时间
                    LatestEditTime = reader.GetDateTime(9), // 最近修改时间
                    ActionType = reader.GetString(10), // 动作类型
                    UsedTimes = reader.GetInt32(11) // 使用次数
                }); // 添加 ButtonData 到列表
            }
            return oldButtonDataList; // 返回旧的 ButtonData 列表
        }

        // 更新设置数据库
        private void Update2_1_3SettingDatabase()
        {
            using var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
            string addRememberLastPageQuery = @"
            ALTER TABLE Convention 
            ADD COLUMN RememberLastPage BOOL DEFAULT FALSE;"; // 为Convention表添加RememberLastPage列
            using var addRememberLastPageCommand = new SQLiteCommand(addRememberLastPageQuery, connection);
            addRememberLastPageCommand.ExecuteNonQuery(); // 执行更新命令

            string addLastPageQuery = @"
            ALTER TABLE Convention 
            ADD COLUMN RememberLastPage INTEGER DEFAULT 11;"; // 为Convention表添加LastPage列
            using var addLastPageCommand = new SQLiteCommand(addLastPageQuery, connection);
            addLastPageCommand.ExecuteNonQuery(); // 执行更新命令

            string addEnableMemoryOptimizationQuery = @"
            ALTER TABLE Convention 
            ADD COLUMN EnableMemoryOptimization BOOL DEFAULT TRUE;"; // 为Convention表添加EnableMemoryOptimization列
            using var addEnableMemoryOptimizationCommand = new SQLiteCommand(addEnableMemoryOptimizationQuery, connection);
            addEnableMemoryOptimizationCommand.ExecuteNonQuery(); // 执行更新命令
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
                    var tableName = db2.GetAllTableNames(); // 获取所有表名
                    foreach (var name in tableName) // 遍历所有表名
                        RenameColumn(name); // 重命名表格中的列名
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
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("数据库迁移失败，请关闭应用后手动将数据库文件从应用目录迁移到目录下的Database文件夹。", "Error"); // 弹出消息提醒
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
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("数据库更新失败，该版本的数据库无法更新，请删除数据库后重试。", "Error"); // 弹出消息提醒
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
                string renameQuery1 = $"ALTER TABLE {tableName} RENAME COLUMN ButtonName TO Title;"; // 重命名第一个列
                using var command1 = new SQLiteCommand(renameQuery1, connection); // 创建 SQLiteCommand 对象
                command1.ExecuteNonQuery(); // 执行更新命令

                // 重命名第二个列：Type → ActionType
                string renameQuery2 = $"ALTER TABLE {tableName} RENAME COLUMN Type TO ActionType;"; // 重命名第二个列
                using var command2 = new SQLiteCommand(renameQuery2, connection); // 创建 SQLiteCommand 对象
                command2.ExecuteNonQuery(); // 执行更新命令

                // 重命名第三个列：Usage → Description
                string renameQuery3 = $"ALTER TABLE {tableName} RENAME COLUMN Usage TO Description;"; // 重命名第三个列
                using var command3 = new SQLiteCommand(renameQuery3, connection); // 创建 SQLiteCommand 对象
                command3.ExecuteNonQuery(); // 执行更新命令
                transaction.Commit(); // 提交事务
            }
            catch (Exception ex)
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast($"重命名表格{tableName}中的列名失败,请删除数据库", "Error"); // 弹出消息提醒
                transaction.Rollback(); // 回滚事务
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
            using var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
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
                    );"; // 创建新表
                using var createNewTableCommand = new SQLiteCommand(createNewTableQuery, connection); // 创建 SQLiteCommand 对象
                createNewTableCommand.ExecuteNonQuery(); // 执行更新命令

                string insertOldDataQuery = @"INSERT INTO ConventionTemp 
                    (ID, AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping)
                    SELECT ID, AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping
                    FROM Convention;"; // 复制旧表数据到新表
                using var insertOldDataCommand = new SQLiteCommand(insertOldDataQuery, connection); // 创建 SQLiteCommand 对象
                insertOldDataCommand.ExecuteNonQuery(); // 执行更新命令

                string dropOldTableQuery = "DROP TABLE Convention;"; // 删除旧表
                using var dropOldTableCommand = new SQLiteCommand(dropOldTableQuery, connection); // 创建 SQLiteCommand 对象
                dropOldTableCommand.ExecuteNonQuery(); // 执行更新命令

                string renameNewTableQuery = "ALTER TABLE ConventionTemp RENAME TO Convention;"; // 重命名新表为旧表的名称
                using var renameNewTableCommand = new SQLiteCommand(renameNewTableQuery, connection); // 创建 SQLiteCommand 对象
                renameNewTableCommand.ExecuteNonQuery(); // 执行更新命令
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
                    var oldButtonData = new List<ButtonDataBefore2_2_0>();
                    using var oldCommand = new SQLiteCommand("SELECT * FROM Temp_ButtonData", connection);
                    using var oldReader = oldCommand.ExecuteReader();
                    while (oldReader.Read())
                    {
                        oldButtonData.Add(new ButtonDataBefore2_2_0
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
                        string tableName = buttonData.ButtonID.Substring(0, buttonData.ButtonID.Length - 3); // 从ButtonID解析表名
                        db2.CreateButtonTable(tableName); // 检查表是否存在，不存在则创建

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
                    using var dropCommand = new SQLiteCommand("DROP TABLE Temp_ButtonData", connection); // 创建 SQLiteCommand 对象
                    dropCommand.ExecuteNonQuery(); // 执行删除语句
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

        // 释放资源
        public void Dispose()
        {
            Dispose(true); // 释放托管资源
            GC.SuppressFinalize(this); // 调用终结器（析构器）以释放非托管资源
        }

        /// <summary>
        /// 释放资源的受保护实现
        /// </summary>
        /// <param name="disposing"> 是否释放托管资源 </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) _disposed = true; // 防止多次调用
        }

        // 析构函数
        ~DatabaseUpdateManager()
        {
            Dispose(false); // 释放非托管资源
        }

        // 2.2.0 版本之前的按钮数据
        public class ButtonDataBefore2_2_0
        {
            public string ButtonID { get; set; }
            public string Title { get; set; }
            public string Location { get; set; }
            public string ImagePath { get; set; }
            public string Data1 { get; set; }
            public string Data2 { get; set; }
            public string Data3 { get; set; }
            public string Description { get; set; }
            public DateTime CreateTime { get; set; }
            public DateTime LatestEditTime { get; set; }
            public string ActionType { get; set; }
            public int UsedTimes { get; set; }
        }
    }
}