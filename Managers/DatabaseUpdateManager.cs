using Quicker.Windows.Menus;
using System.Data.SQLite;
using Quicker.Database;
using Quicker.Windows;
using System.IO;

namespace Quicker.Managers
{
    public class DatabaseUpdateManager : IDisposable
    {
        private readonly string _newBasePath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\"; // 新图片文件夹路径
        private const string DatabaseFolder = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\Database"; // 数据库文件夹路径
        private readonly ButtonDatabase _db2 = new(); // 按钮数据库对象
        private bool _disposed = false; // 标记是否已释放资源

        // 检查并更新数据库
        public void CheckAndUpgradeDatabase()
        {
            string dbVersion = GetCurrentVersion(); // 获取当前数据库版本号
            if (dbVersion != SettingDatabase.currentVersion)
            {
                LoadingWindow loadingWindow = new(); // 创建加载窗口
                loadingWindow.Show(); // 显示加载窗口
                try
                {
                    UpdateDatabase(dbVersion); // 数据库版本不同，更新数据库
                }
                finally
                {
                    loadingWindow.Close(); // 关闭加载窗口
                }
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
                        return; // 数据库版本相同，无需更新
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
                using var toast = new ToastManager(); // 创建 Toast 管理器
                toast.ShowToast("数据库更新失败，请删除数据库文件后重试。", "Error"); // 显示 Toast 通知
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
            return reader.Read() ? reader.GetString(0) : null; // 如果有数据，返回版本号；如果没有数据，则返回null
        }

        /// <summary>
        /// 设置数据库版本号
        /// </summary>
        /// <param name="version"> 版本号 </param>
        private void SetCurrentVersion(string version)
        {
            using var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
            string updateVersionQuery = @$"UPDATE Convention SET Version = '{version}';"; // 设置默认值
            using var updateVersionCommand = new SQLiteCommand(updateVersionQuery, connection); // 创建 SQLiteCommand 对象
            updateVersionCommand.ExecuteNonQuery(); // 执行更新命令
        }

        /// <summary>
        /// 检查数据库文件是否存在
        /// </summary>
        private bool DatabaseExists(string dbFileName)
        {
            string dbFilePath = Path.Combine(DatabaseFolder, dbFileName); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        /// <summary>
        /// 检查是否存在设置数据库
        /// </summary>
        /// <returns> 是否存在设置数据库 </returns>
        private bool ExistSettingDatabase() => DatabaseExists("Setting.db");

        /// <summary>
        /// 检查是否存在按钮数据库
        /// </summary>
        /// <returns> 是否存在按钮数据库 </returns>
        private bool ExistButtonDatabase() => DatabaseExists("Button.db");

        /// <summary>
        /// 检查是否存在动作页面数据库
        /// </summary>
        /// <returns> 是否存在动作页面数据库 </returns>
        private bool ExistActionPageDatabase() => DatabaseExists("ActionPage.db");

        // 数据库版本从2.1.3升级到2.2.0
        private void UpdateFrom2_1_3To2_2_0()
        {
            SetCurrentVersion("2.2.0"); // 设置数据库版本为2.2.0
            if (ExistButtonDatabase()) // 如果存在按钮数据库
            {
                Update2_1_3ButtonDatabase("Global"); // 更新2.1.3版本按钮数据库的Global表
                Update2_1_3ButtonDatabase("Common"); // 更新2.1.3版本按钮数据库的Common表
                if (_db2.TableExists("Desktop")) Update2_1_3ButtonDatabase("Desktop"); // 如果2.1.3版本按钮数据库存在Desktop表，更新Desktop表
                if (_db2.TableExists("Taskbar")) Update2_1_3ButtonDatabase("Taskbar"); // 如果2.1.3版本按钮数据库存在Desktop表，更新Taskbar表
            }
            UpdateAllImagePathsToNewFolder(); // 更新所有按钮的 ImagePath 字段
            Update2_1_3SettingDatabase(); // 更新设置数据库
        }

        // 更新按钮数据库中所有按钮的 ImagePath 字段
        public void UpdateAllImagePathsToNewFolder()
        {
            using var connection = _db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                var tableNames = _db2.GetAllTableNames(); // 获取所有表名
                foreach (string tableName in tableNames) // 遍历所有表名
                {
                    UpdateTableImagePaths(connection, tableName); // 更新表的图片路径
                }
                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
                throw; // 重新抛出异常
            }
        }

        /// <summary>
        /// 更新表的图片路径
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="tableName"> 表名 </param>
        private void UpdateTableImagePaths(SQLiteConnection connection, string tableName)
        {
            const string selectQuery = "SELECT ButtonID, ImagePath FROM [{0}];"; // 查询按钮ID和图片路径
            const string updateQuery = "UPDATE [{0}] SET ImagePath = @NewImagePath WHERE ButtonID = @ButtonID;"; // 更新图片路径
            using var selectCommand = new SQLiteCommand(string.Format(selectQuery, tableName), connection); // 创建 SQLiteCommand 对象
            using var selectReader = selectCommand.ExecuteReader(); // 执行查询命令
            
            var entriesToUpdate = new List<(int ButtonID, string ImagePath)>(); // 更新列表
            while (selectReader.Read())
            {
                int buttonID = selectReader.GetInt32(0); // 获取按钮ID
                string imagePath = selectReader.GetString(1); // 获取图片路径
                string fileName = Path.GetFileName(imagePath); // 获取文件名
                
                if (!string.IsNullOrEmpty(fileName))
                {
                    string newImagePath = Path.Combine(_newBasePath, fileName); // 新图片路径
                    entriesToUpdate.Add((buttonID, newImagePath)); // 添加到更新列表
                }
            }

            foreach (var (buttonID, newImagePath) in entriesToUpdate)
            {
                using var updateCommand = new SQLiteCommand(string.Format(updateQuery, tableName), connection); // 创建 SQLiteCommand 对象
                updateCommand.Parameters.AddWithValue("@NewImagePath", newImagePath); // 添加新图片路径
                updateCommand.Parameters.AddWithValue("@ButtonID", buttonID); // 添加按钮ID
                updateCommand.ExecuteNonQuery(); // 执行更新命令
            }
        }

        /// <summary>
        /// 更新按钮数据库的列名
        /// </summary>
        /// <param name="tableName"> 表名 </param>
        private void Update2_1_3ButtonDatabase(string tableName)
        {
            using var connection = _db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                ChangeDataStyle(connection, tableName); // 转换数据类型
                AddNewColumn(tableName); // 添加新列
                ChangeButtonIDType(connection); // 转换 ButtonID 类型
                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
                throw; // 重新抛出异常
            }
        }

        /// <summary>
        /// 转换数据类型
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="tableName"> 表名 </param>
        private void ChangeDataStyle(SQLiteConnection connection, string tableName)
        {
            const string createTempTableQuery = @"
                CREATE TABLE Temp_{0}
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
                );"; // 创建临时表

            const string migrateDataQuery = @"
                INSERT INTO Temp_{0} 
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
                FROM {0};"; // 迁移数据

            using var createTempTableCommand = new SQLiteCommand(string.Format(createTempTableQuery, tableName), connection);
            createTempTableCommand.ExecuteNonQuery(); // 执行创建临时表命令

            using var migrateDataCommand = new SQLiteCommand(string.Format(migrateDataQuery, tableName), connection);
            migrateDataCommand.ExecuteNonQuery(); // 执行迁移数据命令

            using var dropCommand = new SQLiteCommand($"DROP TABLE {tableName}", connection);
            dropCommand.ExecuteNonQuery(); // 执行删除表命令

            using var renameCommand = new SQLiteCommand($"ALTER TABLE Temp_{tableName} RENAME TO {tableName};", connection);
            renameCommand.ExecuteNonQuery(); // 执行重命名表命令
        }

        /// <summary>
        /// 添加新列：动作使用次数
        /// </summary>
        /// <param name="tableName"> 表名 </param>
        private void AddNewColumn(string tableName)
        {
            using var connection = _db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                const string addUsedTimesQuery = @"
                    ALTER TABLE {0}
                    ADD COLUMN UsedTimes INTEGER DEFAULT 0;"; // 添加新列：动作使用次数

                using var command = new SQLiteCommand(string.Format(addUsedTimesQuery, tableName), connection);
                command.ExecuteNonQuery(); // 执行添加新列命令
                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
                throw; // 重新抛出异常
            }
        }

        /// <summary>
        /// 转换 ButtonID 类型
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="tableName"> 表名（已弃用，保留参数兼容）</param>
        private void ChangeButtonIDType(SQLiteConnection connection)
        {
            var oldTableNames = _db2.GetAllTableNames()
                .Where(n => !n.StartsWith("sqlite_")) // 过滤系统表
                .ToList(); // 获取所有表名

            foreach (var oldTableName in oldTableNames)
            {
                // 创建临时表
                var tempTableName = $"Temp_{oldTableName}"; // 临时表名
                _db2.CreateButtonTable(tempTableName); // 创建临时表

                // 迁移数据
                var oldButtonDataList = GetOldDataFromTable(oldTableName); // 获取旧数据
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
                        _db2.UpdateAction(newButtonData, tempTableName); // 更新动作
                    }
                }

                _db2.DeleteButtonTable(oldTableName); // 删除旧表
                string renameQuery = $"ALTER TABLE {tempTableName} RENAME TO {oldTableName};"; // 重命名表
                new SQLiteCommand(renameQuery, connection).ExecuteNonQuery(); // 执行重命名表命令
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
            using var connection = _db2.OpenConnection(); // 打开数据库连接
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

            using var connection = _db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                MigrateDatabaseFiles(); // 迁移数据库文件
                if (ExistButtonDatabase()) // 如果存在按钮数据库
                {
                    var tableName = _db2.GetAllTableNames(); // 获取所有表名
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

        /// <summary>
        /// 迁移数据库文件到Database文件夹
        /// </summary>
        private void MigrateDatabaseFiles()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dbFolder = Path.Combine(appDirectory, "Database");
            var filesToMigrate = new[]
            {
                ("Button.db", dbFolder),
                ("Setting.db", dbFolder)
            }; // 迁移数据库文件到Database文件夹

            foreach (var (fileName, destinationPath) in filesToMigrate)
            {
                string sourcePath = Path.Combine(appDirectory, fileName); // 源路径
                if (File.Exists(sourcePath))
                {
                    MigrateFile(fileName, destinationPath); // 迁移文件
                }
            }
        }

        /// <summary>
        /// 迁移文件到指定位置
        /// </summary>
        public void MigrateFile(string fileName, string destinationPath)
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // 应用程序目录
            string sourceFilePath = Path.Combine(appDirectory, fileName); // 源文件路径
            string destinationFilePath = Path.Combine(destinationPath, fileName); // 目标文件路径

            if (!File.Exists(sourceFilePath)) return; // 如果源文件不存在，则返回

            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath); // 创建目标目录
                }

                File.Copy(sourceFilePath, destinationFilePath, true); // 复制文件
                File.Delete(sourceFilePath); // 删除源文件
            }
            catch
            {
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast("数据库迁移失败，请关闭应用后手动将数据库文件从应用目录迁移到目录下的Database文件夹。", "Error");
            }
        }

        /// <summary>
        /// 重命名表格中的列名
        /// </summary>
        public void RenameColumn(string tableName)
        {
            using var connection = _db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                var columnRenames = new[]
                {
                    ("ButtonName", "Title"),
                    ("Type", "ActionType"),
                    ("Usage", "Description")
                }; // 重命名表格中的列名

                foreach (var (oldName, newName) in columnRenames)
                {
                    string renameQuery = $"ALTER TABLE {tableName} RENAME COLUMN {oldName} TO {newName};"; // 重命名表格中的列名
                    using var command = new SQLiteCommand(renameQuery, connection); // 创建 SQLiteCommand 对象
                    command.ExecuteNonQuery(); // 执行更新命令
                }

                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
                using var toast = new ToastManager(); // 消息提醒管理器
                toast.ShowToast($"重命名表格{tableName}中的列名失败,请删除数据库", "Error"); // 弹出消息提醒
            }
        }

        /// <summary>
        /// 更新设置数据库到2.1.0版本
        /// </summary>
        private void Update2_1_0SettingDatabase()
        {
            using var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                const string createNewTableQuery = @"
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

                const string insertOldDataQuery = @"
                    INSERT INTO ConventionTemp 
                    (ID, AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping)
                    SELECT ID, AutoStart, ShowNotification, ShowAddImage, TotalUsageTime, HideTooltip, LongPressThreshold, MouseMovePixels, LoopPageFlipping
                    FROM Convention;"; // 插入旧数据

                using (var command = new SQLiteCommand(createNewTableQuery, connection))
                {
                    command.ExecuteNonQuery(); // 执行更新命令
                }

                using (var command = new SQLiteCommand(insertOldDataQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand("DROP TABLE Convention;", connection))
                {
                    command.ExecuteNonQuery(); // 执行更新命令
                }

                using (var command = new SQLiteCommand("ALTER TABLE ConventionTemp RENAME TO Convention;", connection))
                {
                    command.ExecuteNonQuery(); // 执行更新命令
                }

                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
                throw; // 重新抛出异常
            }
        }

        // 将旧表中的所有按钮迁移到对应的新表并删除旧表
        private void Update2_1_0ButtonDatabase()
        {
            using var connection = _db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                // 检查是否存在 ButtonData 表
                using var checkCommand = new SQLiteCommand(
                    "SELECT name FROM sqlite_master WHERE type='table' AND name='ButtonData'",
                    connection); // 创建 SQLiteCommand 对象
                using var checkReader = checkCommand.ExecuteReader(); // 执行查询命令
                if (!checkReader.Read()) return; // 如果表不存在，则返回

                // 将旧表重命名为临时表
                using (var renameCommand = new SQLiteCommand("ALTER TABLE ButtonData RENAME TO Temp_ButtonData", connection))
                {
                    renameCommand.ExecuteNonQuery(); // 执行更新命令
                }

                // 获取旧表ButtonData中的所有按钮数据
                var oldButtonData = GetOldButtonData(connection); // 获取旧按钮数据

                // 将每个按钮数据迁移到对应的新表中
                foreach (var buttonData in oldButtonData)
                {
                    string tableName = buttonData.ButtonID.Substring(0, buttonData.ButtonID.Length - 3); // 获取表名
                    _db2.CreateButtonTable(tableName); // 创建新表

                    InsertButtonData(connection, tableName, buttonData); // 插入按钮数据
                }

                // 删除临时表
                using (var dropCommand = new SQLiteCommand("DROP TABLE Temp_ButtonData", connection))
                {
                    dropCommand.ExecuteNonQuery(); // 执行更新命令
                }

                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
                throw; // 重新抛出异常
            }
        }

        /// <summary>
        /// 获取旧按钮数据
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <returns>旧按钮数据列表</returns>
        private List<ButtonDataBefore2_2_0> GetOldButtonData(SQLiteConnection connection)
        {
            var oldButtonData = new List<ButtonDataBefore2_2_0>(); // 旧按钮数据列表
            using var command = new SQLiteCommand("SELECT * FROM Temp_ButtonData", connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询命令
            while (reader.Read())
            {
                oldButtonData.Add(new ButtonDataBefore2_2_0
                {
                    ButtonID = reader.GetString(0),
                    Title = reader.GetString(1),
                    Location = reader.GetString(2),
                    ImagePath = reader.GetString(3),
                    Data1 = reader.GetString(4),
                    Data2 = reader.GetString(5),
                    Data3 = reader.GetString(6),
                    Description = reader.GetString(7),
                    CreateTime = reader.GetDateTime(8),
                    LatestEditTime = reader.GetDateTime(9),
                    ActionType = "OpenFile"
                }); // 添加按钮数据到列表
            }
            return oldButtonData; // 返回旧按钮数据列表
        }

        /// <summary>
        /// 插入按钮数据
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableName">表名</param>
        /// <param name="buttonData">按钮数据</param>
        private void InsertButtonData(SQLiteConnection connection, string tableName, ButtonDataBefore2_2_0 buttonData)
        {
            const string insertQuery = @"
                INSERT INTO {0} 
                (ButtonID, ButtonName, Location, ImagePath, Data1, Data2, Data3, Usage, CreateTime, LatestEditTime, Type) 
                VALUES 
                (@ButtonID, @ButtonName, @Location, @ImagePath, @Data1, @Data2, @Data3, @Usage, @CreateTime, @LatestEditTime, @Type)"; // 插入按钮数据

            using var command = new SQLiteCommand(string.Format(insertQuery, tableName), connection); // 创建 SQLiteCommand 对象
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID); // 添加按钮ID
            command.Parameters.AddWithValue("@ButtonName", buttonData.Title); // 添加按钮名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 添加位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 添加图片路径
            command.Parameters.AddWithValue("@Data1", buttonData.Data1); // 添加数据1
            command.Parameters.AddWithValue("@Data2", buttonData.Data2); // 添加数据2
            command.Parameters.AddWithValue("@Data3", buttonData.Data3); // 添加数据3
            command.Parameters.AddWithValue("@Usage", buttonData.Description); // 添加使用说明
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 添加创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 添加最近修改时间
            command.Parameters.AddWithValue("@Type", buttonData.ActionType); // 添加动作类型
            command.ExecuteNonQuery(); // 执行更新命令
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
            if (!_disposed) _disposed = true; // 防止重复释放
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