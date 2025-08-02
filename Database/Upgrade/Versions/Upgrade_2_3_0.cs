using Quicker.Database.Core;
using System.Data.SQLite;
using Quicker.Helpers;
using System.IO;

namespace Quicker.Database.Upgrade.Versions
{
    internal class Upgrade_2_3_0 : IDatabaseUpgradeStep
    {
        public string FromVersion => "2.2.0"; // 升级前的版本号
        public string ToVersion => "2.3.0"; // 升级后的版本号

        /// <summary>
        /// 升级数据库
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        public void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            SettingDatabase.InitializeAppearance(); // 新增数据库表
            AddTrayIconColumnsIfNotExist(connection); // 新增托盘图标字段
            RenameDefaultTables(connection, manager); // 重命名默认表
            UpdateButtonDataImagePath(connection); // 更新ButtonData表ImagePath字段
        }

        /// <summary>
        /// 更新 ButtonData 表中 ImagePath 字段的路径
        /// </summary>
        /// <param name="connection">数据库连接</param>
        private void UpdateButtonDataImagePath(SQLiteConnection connection)
        {
            // 获取所有按钮数据表名
            var tableNames = new List<string>();
            using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string tableName = reader.GetString(0);
                    // 只处理按钮数据表
                    if (!string.IsNullOrEmpty(tableName) && !tableName.StartsWith("sqlite_"))
                    {
                        tableNames.Add(tableName);
                    }
                }
            }

            // 使用AppPathHelper获取路径
            string oldPath = Path.Combine(AppPathHelper.AppDataRoot, "LocalIcons") + Path.DirectorySeparatorChar;
            string newPath = AppPathHelper.LocalIconsFolder + Path.DirectorySeparatorChar;
            foreach (var table in tableNames)
            {
                string sql = $"UPDATE [{table}] SET ImagePath = REPLACE(ImagePath, @oldPath, @newPath) WHERE ImagePath LIKE @likePath";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@oldPath", oldPath);
                    cmd.Parameters.AddWithValue("@newPath", newPath);
                    cmd.Parameters.AddWithValue("@likePath", oldPath + "%");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 为 Convention 表添加托盘图标字段（如果不存在）
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        private void AddTrayIconColumnsIfNotExist(SQLiteConnection connection)
        {
            // 直接插入 TrayIconPathRunning 字段
            using (var cmd = new SQLiteCommand("ALTER TABLE Convention ADD COLUMN TrayIconPathRunning TEXT DEFAULT 'pack://application:,,,/Resources/Images/Quicker_Enabled.png'", connection))
            {
                cmd.ExecuteNonQuery();
            }
            // 直接插入 TrayIconPathPaused 字段
            using (var cmd = new SQLiteCommand("ALTER TABLE Convention ADD COLUMN TrayIconPathPaused TEXT DEFAULT 'pack://application:,,,/Resources/Images/Quicker_Disabled.ico'", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 重命名表名
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        public void RenameDefaultTables(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            var renameMap = new Dictionary<string, string>
            {
                { "Global", "_global" },
                { "Common", "common" },
                { "Desktop", "desktop" },
                { "Taskbar", "taskbar" }
            }; // 重命名映射表
            foreach (var kv in renameMap)
            {
                if (manager._db2.TableExists(kv.Key) && !manager._db2.TableExists(kv.Value)) // 旧表存在且新表不存在
                {
                    RenameTable(connection, kv.Key, kv.Value); // 重命名表
                }
            }
        }

        /// <summary>
        /// 重命名表
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="oldName"> 旧表名 </param>
        /// <param name="newName"> 新表名 </param>
        public void RenameTable(SQLiteConnection connection, string oldName, string newName)
        {
            string sql = $"ALTER TABLE [{oldName}] RENAME TO [{newName}]"; // 重命名表
            using var command = new SQLiteCommand(sql, connection); // 创建命令
            command.ExecuteNonQuery(); // 执行命令
        }
    }
}