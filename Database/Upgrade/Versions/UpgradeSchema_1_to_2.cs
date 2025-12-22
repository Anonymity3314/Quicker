using Quicker.Database.Core;
using System.Data.SQLite;
using Quicker.Helpers;
using System.IO;

namespace Quicker.Database.Upgrade.Versions
{
    /// <summary>
    /// 数据库结构从版本 1 升级到版本 2 的迁移步骤。
    /// 对应原先 2.2.0 -> 2.3.0 的结构变更。
    /// </summary>
    internal class UpgradeSchema_1_to_2 : IDatabaseUpgradeStep
    {
        public int FromSchemaVersion => 1; // 升级前的结构版本号
        public int ToSchemaVersion => 2;   // 升级后的结构版本号

        /// <summary>
        /// 升级数据库
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        public void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            SettingDatabase.InitializeAppearance(); // 新增数据库表
            AddTrayIconColumnsIfNotExist(connection); // 新增托盘图标字段
            AddUseMenuAnimationColumnIfNotExist(connection); // 新增菜单动画字段
            AddIsDarkThemeColumnIfNotExist(connection); //添加 IsDarkTheme 字段
            //MigrateAppearanceData(connection); //将旧单行 Appearance 数据迁移到新的 Light 主题行
            RemoveVersionColumnFromConvention(connection); // 删除 Convention 表中的旧版本号字段
            RenameDefaultTables(connection, manager); // 重命名默认表
            UpdateButtonDataImagePath(connection); // 更新ButtonData表ImagePath字段
        }

        /// <summary>
        /// 删除 Convention 表中的旧版本号字段（Version）
        /// 通过重建表的方式安全移除该列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        private void RemoveVersionColumnFromConvention(SQLiteConnection connection)
        {
            using var transaction = connection.BeginTransaction();

            // 创建新的 Convention 表（不包含 Version 字段，但包含所有现有配置字段）
            const string createNewTableSql = @"
            CREATE TABLE IF NOT EXISTS Convention_new
            (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                AutoStart BOOLEAN,
                ShowNotification BOOLEAN,
                ShowAddImage BOOLEAN,
                TotalUsageTime REAL,
                HideTooltip BOOLEAN,
                LongPressThreshold INTEGER,
                MouseMovePixels INTEGER,
                LoopPageFlipping BOOLEAN,
                RememberLastPage BOOLEAN,
                LastPage INTEGER,
                EnableMemoryOptimization BOOLEAN,
                TrayIconPathRunning TEXT,
                TrayIconPathPaused TEXT,
                UseMenuAnimation BOOLEAN
            );";
            using (var createCmd = new SQLiteCommand(createNewTableSql, connection, transaction))
            {
                createCmd.ExecuteNonQuery();
            }

            // 将旧表数据迁移到新表（跳过 Version 列）
            const string copyDataSql = @"
            INSERT INTO Convention_new
            (
                ID,
                AutoStart,
                ShowNotification,
                ShowAddImage,
                TotalUsageTime,
                HideTooltip,
                LongPressThreshold,
                MouseMovePixels,
                LoopPageFlipping,
                RememberLastPage,
                LastPage,
                EnableMemoryOptimization,
                TrayIconPathRunning,
                TrayIconPathPaused,
                UseMenuAnimation
            )
            SELECT
                ID,
                AutoStart,
                ShowNotification,
                ShowAddImage,
                TotalUsageTime,
                HideTooltip,
                LongPressThreshold,
                MouseMovePixels,
                LoopPageFlipping,
                RememberLastPage,
                LastPage,
                EnableMemoryOptimization,
                TrayIconPathRunning,
                TrayIconPathPaused,
                UseMenuAnimation
            FROM Convention;";
            using (var copyCmd = new SQLiteCommand(copyDataSql, connection, transaction))
            {
                copyCmd.ExecuteNonQuery();
            }

            // 删除旧表并重命名新表
            using (var dropCmd = new SQLiteCommand("DROP TABLE Convention;", connection, transaction))
            {
                dropCmd.ExecuteNonQuery();
            }

            using (var renameCmd = new SQLiteCommand("ALTER TABLE Convention_new RENAME TO Convention;", connection, transaction))
            {
                renameCmd.ExecuteNonQuery();
            }

            transaction.Commit();
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
        /// 为 Convention 表添加托盘图标字段
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
        /// 为 Convention 表添加菜单动画字段
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        private void AddUseMenuAnimationColumnIfNotExist(SQLiteConnection connection)
        {
            using (var cmd = new SQLiteCommand("ALTER TABLE Convention ADD COLUMN UseMenuAnimation BOOLEAN DEFAULT 1", connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 为 Convention 表添加 IsDarkTheme 字段
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        private void AddIsDarkThemeColumnIfNotExist(SQLiteConnection connection)
        {
            const string sql = "ALTER TABLE Convention ADD COLUMN IsDarkTheme BOOLEAN DEFAULT 0";
            using (var cmd = new SQLiteCommand(sql, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 将旧的单行 Appearance 表数据迁移到新的 Appearance 表 ThemeName='Light' 的记录中。
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        private void MigrateAppearanceData(SQLiteConnection connection)
        {
            const string renameSql = "ALTER TABLE Appearance RENAME TO Appearance_Old";
            using (var cmd = new SQLiteCommand(renameSql, connection))
            {
                cmd.ExecuteNonQuery();
            }
            SettingDatabase.InitializeAppearance(); // 新增数据库表
            const string finalUpdateSql = @"
            UPDATE Appearance SET
                ButtonSize = (SELECT ButtonSize FROM Appearance_Old WHERE ID = 1),
                ButtonGap = (SELECT ButtonGap FROM Appearance_Old WHERE ID = 1),
                BorderWidth = (SELECT BorderWidth FROM Appearance_Old WHERE ID = 1),
                ButtonCornerRadius = (SELECT ButtonCornerRadius FROM Appearance_Old WHERE ID = 1),
                BackgroundColor = (SELECT BackgroundColor FROM Appearance_Old WHERE ID = 1),
                BorderColor = (SELECT BorderColor FROM Appearance_Old WHERE ID = 1),
                ToolbarColor = (SELECT ToolbarColor FROM Appearance_Old WHERE ID = 1),
                ToolbarIconColor = (SELECT ToolbarIconColor FROM Appearance_Old WHERE ID = 1),
                ActionButtonColor = (SELECT ActionButtonColor FROM Appearance_Old WHERE ID = 1),
                ActionButtonMouseOverColor = (SELECT ActionButtonMouseOverColor FROM Appearance_Old WHERE ID = 1),
                BlankButtonColor = (SELECT BlankButtonColor FROM Appearance_Old WHERE ID = 1),
                BlankButtonMouseOverColor = (SELECT BlankButtonMouseOverColor FROM Appearance_Old WHERE ID = 1),
                ButtonTextColor = (SELECT ButtonTextColor FROM Appearance_Old WHERE ID = 1),
                Font1 = (SELECT Font1 FROM Appearance_Old WHERE ID = 1),
                Font2 = (SELECT Font2 FROM Appearance_Old WHERE ID = 1),
                FontSize = (SELECT FontSize FROM Appearance_Old WHERE ID = 1),
                FontWeight = (SELECT FontWeight FROM Appearance_Old WHERE ID = 1),
                BackgroundImagePath = (SELECT BackgroundImagePath FROM Appearance_Old WHERE ID = 1),
                BackgroundImageOpacity = (SELECT BackgroundImageOpacity FROM Appearance_Old WHERE ID = 1),
                Blur = (SELECT Blur FROM Appearance_Old WHERE ID = 1),
                Win11CornerRadius = (SELECT Win11CornerRadius FROM Appearance_Old WHERE ID = 1),
                AutoHideTitleBar = (SELECT AutoHideTitleBar FROM Appearance_Old WHERE ID = 1),
                ShowActionButtonMouseOver = (SELECT ShowActionButtonMouseOver FROM Appearance_Old WHERE ID = 1),
                HideActionNameAfterIcon = (SELECT HideActionNameAfterIcon FROM Appearance_Old WHERE ID = 1),
                ShowActionIconShadow = (SELECT ShowActionIconShadow FROM Appearance_Old WHERE ID = 1),
                EnablePreview = (SELECT EnablePreview FROM Appearance_Old WHERE ID = 1)
            WHERE 
                ThemeName = 'Light' 
                AND EXISTS (SELECT 1 FROM Appearance_Old WHERE ID = 1);"; // 仅当旧表存在数据时才执行更新

            using (var cmd = new SQLiteCommand(finalUpdateSql, connection))
            {
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex) when (ex.Message.Contains("no such column") || ex.Message.Contains("no such table"))
                {
                    // 忽略错误，可能是因为旧表结构不匹配或根本不存在。
                }
            }

            // 4. 清理：删除旧表 Appearance_Old
            const string dropSql = "DROP TABLE IF EXISTS Appearance_Old";
            using (var cmd = new SQLiteCommand(dropSql, connection))
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