using Quicker.Managers;
using System.Data.SQLite;
using System.IO;

namespace Quicker.Database
{
    internal class ActionPageDatabase
    {
        // 获取应用程序根目录，并设置数据库文件路径为根目录下的"Database"文件夹
        private readonly string db3 = "Data Source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "ActionPage.db") + ";Pooling=true;Max Pool Size=100;Journal Mode=Wal;";
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库

        public ActionPageDatabase()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取数据库文件夹路径
            if (!Directory.Exists(dbFolder)) // 如果数据库文件夹不存在，则创建
                Directory.CreateDirectory(dbFolder); // 创建数据库文件夹
            string dbFilePath = Path.Combine(dbFolder, "ActionPage.db"); // 获取数据库文件路径
            if (!File.Exists(dbFilePath)) // 如果数据库文件不存在，则创建
            {
                SQLiteConnection.CreateFile(dbFilePath); // 创建数据库文件
                var buttonTables = db2.GetAllTableNames(); // 获取 ButtonDatabase 中的所有表名
                foreach (var tableName in buttonTables) // 遍历 ButtonDatabase 中的每个表并初始化 ActionPageDatabase
                {
                    CreatAndInitTable(tableName, "", ""); // 创建场景数据表并初始化
                }
            }
        }

        /// <summary>
        /// 创建场景数据表并初始化
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        public void CreatAndInitTable(string tableName, string actionPageIconPath, string actionPageTag)
        {
            CreateActionPageTable(tableName); // 创建场景数据表
            switch (tableName)
            {
                case "Global":
                    actionPageIconPath = "none"; // 设置全局场景图标路径
                    actionPageTag = "_global"; // 设置全局场景标签
                    break;
                case "Common":
                    actionPageIconPath = "none"; // 设置常用场景图标路径
                    actionPageTag = "common"; // 设置常用场景标签
                    break;
                case "Desktop":
                    actionPageIconPath = "none"; // 设置桌面场景图标路径
                    actionPageTag = "desktop"; // 设置桌面场景标签
                    break;
                case "Taskbar":
                    actionPageIconPath = "none"; // 设置任务栏场景图标路径
                    actionPageTag = "taskbar"; // 设置任务栏场景标签
                    break;
                default:
                    break;
            }
            UpdateActionPageTable(tableName, tableName, actionPageIconPath, db2.GetTotalAntionPageIndex(tableName), actionPageTag); // 初始化场景数据表
        }

        /// <summary>
        /// 创建场景数据表
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        public void CreateActionPageTable(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string createTableQuery = $@"CREATE TABLE IF NOT EXISTS [{tableName}]
            (
                ActionPageType TEXT PRIMARY KEY,
                ActionPageIconPath TEXT,
                ActionPageCount INTEGER,
                ActionPageTag TEXT
            );"; // 创建场景数据表的SQL语句
            using var command = new SQLiteCommand(createTableQuery, connection); // 创建SQLiteCommand对象
            command.ExecuteNonQuery(); // 执行创建表的SQL语句
        }

        /// <summary>
        /// 更新场景数据表
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        /// <param name="actionPageType"> 场景名称 </param>
        /// <param name="actionPageIconPath"> 场景图标路径 </param>
        /// <param name="actionPageCount"> 场景数量 </param>
        public void UpdateActionPageTable(string tableName, string actionPageType, string actionPageIconPath, int actionPageCount, string actionPageTag)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            string query = $@"INSERT OR REPLACE INTO {tableName}
            (ActionPageType, ActionPageIconPath, ActionPageCount, ActionPageTag)
            VALUES
            (@ActionPageType, @ActionPageIconPath, @ActionPageCount, @ActionPageTag)"; // 更新场景数据表的SQL语句
            using var command = new SQLiteCommand(query, connection, transaction); // 创建SQLiteCommand对象
            command.Parameters.AddWithValue("@ActionPageType", actionPageType); // 场景类型
            command.Parameters.AddWithValue("@ActionPageIconPath", actionPageIconPath); // 场景图标路径
            command.Parameters.AddWithValue("@ActionPageCount", actionPageCount); // 场景数量
            command.Parameters.AddWithValue("@ActionPageTag", actionPageTag); // 场景标签
            command.ExecuteNonQuery(); // 执行更新表的SQL语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 删除场景数据表
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        /// <param name="actionPageType"> 场景类型 </param>
        public void DeleteActionPageTable(string tableName, string actionPageType)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            string query = $@"DELETE FROM {tableName} WHERE ActionPageType = @ActionPageType"; // 删除场景数据表的SQL语句
            using var command = new SQLiteCommand(query, connection, transaction); // 创建SQLiteCommand对象
            command.Parameters.AddWithValue("@ActionPageType", actionPageType); // 添加参数
            command.ExecuteNonQuery(); // 执行删除表的SQL语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 获取场景数据表
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        /// <returns> 场景数据表 </returns>
        public List<SceneData> GetSceneData(string tableName)
        {
            var conditions = new List<SceneData>(); // 场景数据表
            using var connection = OpenConnection(); // 打开数据库连接
            string selectQuery = $"SELECT * FROM {tableName};"; // 获取场景数据表的SQL语句
            using var command = new SQLiteCommand(selectQuery, connection); // 创建SQLiteCommand对象
            using var reader = command.ExecuteReader(); // 执行查询SQL语句
            while (reader.Read())
            {
                conditions.Add(new SceneData
                {
                    SceneType = reader.GetString(0), // 场景类型
                    SceneIconPath = reader.GetString(1), // 场景图标路径
                    SceneCount = reader.GetInt32(2), // 场景数量
                    SceneTag = reader.GetString(3), // 场景标签
                });
            }
            return conditions; // 返回场景数据表
        }

        /// <summary>
        /// 判断场景数据表是否存在
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        /// <returns> 场景数据表是否存在 </returns>
        public bool TableExists(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name = @TableName;", connection); // 创建SQLiteCommand对象
            command.Parameters.AddWithValue("@TableName", tableName); // 添加参数
            using var reader = command.ExecuteReader(); // 执行查询SQL语句
            return reader.Read(); // 返回场景数据表是否存在
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <returns> 数据库连接 </returns>
        public SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(db3); // 创建数据库连接
            connection.Open(); // 打开数据库连接
            return connection; // 返回数据库连接
        }
    }

    // 场景数据
    public class SceneData
    {
        public string SceneType { get; set; } // 场景类型
        public string SceneIconPath { get; set; } // 场景图标路径
        public int SceneCount { get; set; } // 场景数量
        public string SceneTag { get; set; } // 场景标签
    }

    // 动作页信息
    public class ActionPageData
    {
        public string ActionPageName { get; set; } // 动作页名称
        public DateTime LastEditTime { get; set; } // 最后编辑时间
        public int ActionPageSize { get; set; } // 动作页大小
    }
}