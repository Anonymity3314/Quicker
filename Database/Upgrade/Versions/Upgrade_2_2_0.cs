using Quicker.Database.Core;
using System.Data.SQLite;
using Quicker.Models;
using System.IO;

namespace Quicker.Database.Upgrade.Versions
{
    internal class Upgrade_2_2_0 : IDatabaseUpgradeStep
    {
        public string FromVersion => "2.1.3"; // 升级前的版本号
        public string ToVersion => "2.2.0"; // 升级后的版本号

        /// <summary>
        /// 升级数据库
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        public void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            if (DatabaseExists("Button.db", manager)) // 如果存在按钮数据库
            {
                Update2_1_3ButtonDatabase("Global", manager); // 更新2.1.3版本按钮数据库的Global表
                Update2_1_3ButtonDatabase("Common", manager); // 更新2.1.3版本按钮数据库的Common表
                if (manager._db2.TableExists("Desktop")) Update2_1_3ButtonDatabase("Desktop", manager); // 如果2.1.3版本按钮数据库存在Desktop表，更新Desktop表
                if (manager._db2.TableExists("Taskbar")) Update2_1_3ButtonDatabase("Taskbar", manager); // 如果2.1.3版本按钮数据库存在Desktop表，更新Taskbar表
            }
            UpdateAllImagePathsToNewFolder(manager); // 更新所有按钮的 ImagePath 字段
            Update2_1_3SettingDatabase(); // 更新设置数据库
        }

        /// <summary>
        /// 检查数据库文件是否存在
        /// </summary>
        /// <param name="dbFileName"> 数据库文件名 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        /// <returns> 是否存在数据库文件 </returns>
        private bool DatabaseExists(string dbFileName, DatabaseUpdateManager manager)
        {
            string dbFilePath = Path.Combine(DatabaseUpdateManager.DatabaseFolder, dbFileName); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }

        /// <summary>
        /// 更新按钮数据库的列名
        /// </summary>
        /// <param name="tableName"> 表名 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        private void Update2_1_3ButtonDatabase(string tableName, DatabaseUpdateManager manager)
        {
            using var connection = manager._db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                ChangeDataStyle(connection, tableName); // 转换数据类型
                AddNewColumn(tableName, manager); // 添加新列
                ChangeButtonIDType(connection, manager); // 转换 ButtonID 类型
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
        private void AddNewColumn(string tableName, DatabaseUpdateManager manager)
        {
            using var connection = manager._db2.OpenConnection(); // 打开数据库连接
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
        private void ChangeButtonIDType(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            var oldTableNames = manager._db2.GetAllTableNames()
                .Where(n => !n.StartsWith("sqlite_")) // 过滤系统表
                .ToList(); // 获取所有表名

            foreach (var oldTableName in oldTableNames)
            {
                // 创建临时表
                var tempTableName = $"Temp_{oldTableName}"; // 临时表名
                manager._db2.CreateButtonTable(tempTableName); // 创建临时表

                // 迁移数据
                var oldButtonDataList = GetOldDataFromTable(oldTableName, manager); // 获取旧数据
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
                        manager._db2.UpdateAction(newButtonData, tempTableName); // 更新动作
                    }
                }

                manager._db2.DeleteButtonTable(oldTableName); // 删除旧表
                string renameQuery = $"ALTER TABLE {tempTableName} RENAME TO {oldTableName};"; // 重命名表
                new SQLiteCommand(renameQuery, connection).ExecuteNonQuery(); // 执行重命名表命令
            }
        }

        /// <summary>
        /// 从旧表中获取数据
        /// </summary>
        /// <param name="tableName">旧表名</param>
        /// <returns>旧的 ButtonData 列表</returns>
        private List<ButtonDataBefore2_2_0> GetOldDataFromTable(string tableName, DatabaseUpdateManager manager)
        {
            var oldButtonDataList = new List<ButtonDataBefore2_2_0>(); // 旧的 ButtonData 列表
            using var connection = manager._db2.OpenConnection(); // 打开数据库连接
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

        // 更新按钮数据库中所有按钮的 ImagePath 字段
        public void UpdateAllImagePathsToNewFolder(DatabaseUpdateManager manager)
        {
            using var connection = manager._db2.OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                var tableNames = manager._db2.GetAllTableNames(); // 获取所有表名
                foreach (string tableName in tableNames) // 遍历所有表名
                {
                    UpdateTableImagePaths(connection, tableName, manager); // 更新表的图片路径
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
        private void UpdateTableImagePaths(SQLiteConnection connection, string tableName, DatabaseUpdateManager manager)
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
                    string newImagePath = Path.Combine(manager.NewBasePath, fileName); // 新图片路径
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
    }
}