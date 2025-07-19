using Quicker.Database.Core;
using System.Data.SQLite;
using Quicker.Managers;
using System.IO;

namespace Quicker.Database.Upgrade.Versions
{
    internal class Upgrade_2_1_3 : IDatabaseUpgradeStep
    {
        public string FromVersion => "2.1.2";

        public string ToVersion => "2.1.3";

        /// <summary>
        /// 更新数据库
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库管理器 </param>
        public void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            using var transaction = connection.BeginTransaction(); // 开始事务
            try
            {
                MigrateDatabaseFiles(); // 迁移数据库文件
                if (DatabaseExists(manager)) // 如果存在按钮数据库
                {
                    var tableName = manager._db2.GetAllTableNames(); // 获取所有表名
                    foreach (var name in tableName) // 遍历所有表名
                        RenameColumn(name, manager); // 重命名表格中的列名
                } // 重命名表格中的列名
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
        /// <param name="tableName">表名</param>
        /// <param name="manager">数据库更新管理器</param>
        public void RenameColumn(string tableName, DatabaseUpdateManager manager)
        {
            using var connection = manager._db2.OpenConnection(); // 打开数据库连接
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
                toast.Show($"重命名表格{tableName}中的列名失败,请删除数据库", "Error"); // 弹出消息提醒
            }
        }

        // 迁移数据库文件到Database文件夹
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
        /// <param name="fileName">文件名</param>
        /// <param name="destinationPath">目标路径</param>
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
                toast.Show("数据库迁移失败，请关闭应用后手动将数据库文件从应用目录迁移到目录下的Database文件夹。", "Error");
            }
        }

        /// <summary>
        /// 检查数据库文件是否存在
        /// </summary>
        /// <param name="manager"> 数据库更新管理器 </param>
        /// <returns> 是否存在数据库文件 </returns>
        private bool DatabaseExists(DatabaseUpdateManager manager)
        {
            string dbFilePath = Path.Combine(DatabaseUpdateManager.DatabaseFolder, "Button.db"); // 设置数据库文件路径
            return File.Exists(dbFilePath); // 判断文件是否存在
        }
    }
}