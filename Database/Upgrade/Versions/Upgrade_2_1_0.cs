using Quicker.Database.Core;
using System.Data.SQLite;
using Quicker.Managers;
using System.IO;

namespace Quicker.Database.Upgrade.Versions
{
    internal class Upgrade_2_1_0 : IDatabaseUpgradeStep
    {
        public string FromVersion => null; // 初始版本，没有前置版本

        public string ToVersion => "2.1.0"; // 目标版本

        /// <summary>
        /// 执行升级
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        public void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // 获取应用程序所在目录的路径
                string sourceButtonDbPath = Path.Combine(appDirectory, "Button.db"); // 获取旧按钮数据库路径
                string sourceSettingDbPath = Path.Combine(appDirectory, "Setting.db"); // 获取旧设置数据库路径
                if (File.Exists(sourceSettingDbPath)) Update2_1_0SettingDatabase(); // 更新设置数据库
                if (File.Exists(sourceButtonDbPath)) Update2_1_0ButtonDatabase(manager); // 将旧表中的所有按钮迁移到对应的新表并删除旧表
            }
            catch
            {
                using var toast = new ToastManager(); // 创建 ToastManager 对象
                toast.Show("数据库更新失败，该版本的数据库无法更新，请删除数据库后重试。", "Error"); // 弹出消息提醒
            }
        }

        // 更新设置数据库到2.1.0版本
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
        private void Update2_1_0ButtonDatabase(DatabaseUpdateManager manager)
        {
            using var connection = manager._db2.OpenConnection(); // 打开数据库连接
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
                    manager._db2.CreateButtonTable(tableName); // 创建新表

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
    }
}