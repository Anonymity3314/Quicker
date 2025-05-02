using System.Data.SQLite;
using System.IO;

namespace Quicker.Database
{
    internal class ActionPageDatabase
    {
        private readonly string dbPath3 = "Data Source=ActionPage.db;Pooling=true;Max Pool Size=100;Journal Mode=Wal;";
        private readonly ButtonDatabase db2 = new ButtonDatabase();

        // 初始化数据库
        public void Initialize()
        {
            if (File.Exists("ActionPage.db")) // 如果数据库存在
            {
                return; // 数据库已存在，不再初始化
            }

            SQLiteConnection.CreateFile("ActionPage.db"); // 创建数据库文件
            CreateButtonTable("Global");
            CreateButtonTable("Common");
            if(db2.TableExists("Desktop"))
                CreateButtonTable("Desktop");
            if(db2.TableExists("Taskbar"))
                CreateButtonTable("Taskbar");
        }

        /// <summary>
        /// 创建Button表格
        /// </summary>
        /// <param name="tableName"> 要创建的表格名称 </param>
        public void CreateButtonTable(string tableName)
        {
            var connection = OpenConnection(); // 打开数据库连接
            string createTableQuery = @"CREATE TABLE IF NOT EXISTS [" + tableName + @"]
            (
                ActionPageId TEXT PRIMARY KEY,
                ActionPageName TEXT,
                ActionPageIconPath TEXT
            );";
            using var command = new SQLiteCommand(createTableQuery, connection); // 创建命令对象
            command.ExecuteNonQuery(); // 执行创建表格语句
        }

        /// <summary>
        /// 检查表是否存在，不存在则创建
        /// </summary>
        /// <param name="tableName"> 要检查的表名 </param>
        /// <param name="connection"> 数据库连接 </param>
        private void CheckAndCreateTable(string tableName, SQLiteConnection connection)
        {
            if (TableExists(tableName)) return; // 表存在，直接返回
            CreateButtonTable(tableName); // 创建表
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="tableName">要检查的表名</param>
        /// <returns>表是否存在</returns>
        public bool TableExists(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name = @TableName;", connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        // 打开数据库连接
        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(dbPath3); // 创建 SQLiteConnection 对象
            connection.Open(); // 打开数据库连接
            return connection; // 返回打开的连接
        }
    }
}

public class ActionPageDatabase
{
    public string ActionPageId { get; set; }
    public string ActionPageName { get; set; }
    public string ActionPageIconPath { get; set; }
}