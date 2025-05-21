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
                    CreatAndInitTable(tableName, "", ""); // 创建数据表并初始化
                }
            }
        }

        /// <summary>
        /// 创建场景数据表并初始化
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        public void CreatAndInitTable(string tableName, string sceneIconPath, string sceneTag)
        {
            CreateSceneTable(tableName); // 创建场景数据表
            string actionPageProcess = "", actionPageName = "";
            switch (tableName)
            {
                case "Global":
                    sceneIconPath = "none"; // 设置全局场景图标路径
                    sceneTag = "_global"; // 设置全局场景标签
                    actionPageProcess = "Default"; // 设置动作页所属应用程序名称
                    actionPageName = "默认全局动作页"; // 设置动作页名称
                    break;
                case "Common":
                    sceneIconPath = "none"; // 设置常用场景图标路径
                    sceneTag = "common"; // 设置常用场景标签
                    actionPageProcess = "Default"; // 设置动作页所属应用程序名称
                    actionPageName = "默认"; // 设置动作页名称
                    break;
                case "Desktop":
                    sceneIconPath = "none"; // 设置桌面场景图标路径
                    sceneTag = "desktop"; // 设置桌面场景标签
                    actionPageProcess = "Windows桌面"; // 设置动作页所属应用程序名称
                    actionPageName = "桌面 #"; // 设置动作页名称
                    break;
                case "Taskbar":
                    sceneIconPath = "none"; // 设置任务栏场景图标路径
                    sceneTag = "taskbar"; // 设置任务栏场景标签
                    actionPageProcess = "Windows任务栏"; // 设置动作页所属应用程序名称
                    actionPageName = "任务栏 #"; // 设置动作页名称
                    break;
                default:
                    break;
            }
            int actionPageCount = db2.GetTotalAntionPageIndex(tableName); // 获取动作页数量
            UpdateSceneTable(tableName, tableName, sceneIconPath, actionPageCount, sceneTag); // 初始化场景数据表
            for (int i = 0; i < actionPageCount; i++) 
            {
                CreatActionPageTable(tableName);
                switch(tableName)
                {
                    case "Global":
                    case "Common":
                        break;
                    default:
                        actionPageName = actionPageName + i.ToString(); // 设置动作页名称
                        break;
                }
                UpdateActionPageTable(tableName, tableName+i.ToString(), actionPageProcess, actionPageName, 0); // 初始化动作页数据表
            }
        }

        /// <summary>
        /// 创建场景数据表
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        public void CreateSceneTable(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            string createTableQuery = $@"CREATE TABLE IF NOT EXISTS [{tableName + "Scene"}]
            (
                SceneType TEXT PRIMARY KEY,
                SceneIconPath TEXT,
                SceneCount INTEGER,
                SceneTag TEXT
            );"; // 创建场景数据表的SQL语句
            using var command = new SQLiteCommand(createTableQuery, connection); // 创建SQLiteCommand对象
            command.ExecuteNonQuery(); // 执行创建表的SQL语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 更新场景数据表
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        /// <param name="sceneType"> 场景名称 </param>
        /// <param name="sceneIconPath"> 场景图标路径 </param>
        /// <param name="sceneCount"> 场景数量 </param>
        public void UpdateSceneTable(string tableName, string sceneType, string sceneIconPath, int sceneCount, string sceneTag)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            string query = $@"INSERT OR REPLACE INTO {tableName + "Scene"}
            (SceneType, SceneIconPath, SceneCount, SceneTag)
            VALUES
            (@SceneType, @SceneIconPath, @SceneCount, @SceneTag)"; // 更新场景数据表的SQL语句
            using var command = new SQLiteCommand(query, connection, transaction); // 创建SQLiteCommand对象
            command.Parameters.AddWithValue("@SceneType", sceneType); // 场景类型
            command.Parameters.AddWithValue("@SceneIconPath", sceneIconPath); // 场景图标路径
            command.Parameters.AddWithValue("@SceneCount", sceneCount); // 场景数量
            command.Parameters.AddWithValue("@SceneTag", sceneTag); // 场景标签
            command.ExecuteNonQuery(); // 执行更新表的SQL语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 创建动作页数据表
        /// </summary>
        /// <param name="tableName"> 动作页名称 </param>
        public void CreatActionPageTable(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            string createTableQuery = $@"CREATE TABLE IF NOT EXISTS [{tableName + "ActionPage"}]
            (
                DefaultActionPageName TEXT PRIMARY KEY,
                ActionProcess TEXT,
                ActionPageName TEXT,
                LastEditTime DATETIME,
                ActionPageSize INTEGER
            );"; // 创建场景数据表的SQL语句
            using var command = new SQLiteCommand(createTableQuery, connection); // 创建SQLiteCommand对象
            command.ExecuteNonQuery(); // 执行创建表的SQL语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 更新动作页数据
        /// </summary>
        /// <param name="tableName"> 动作页名称 </param>
        /// <param name="actionProcess"> 动作页所属应用程序名称 </param>
        /// <param name="actionPageName"> 动作页名称 </param>
        /// <param name="actionPageSize"> 动作页大小 </param>
        public void UpdateActionPageTable(string tableName, string defaultActionPageName, string actionProcess, string actionPageName, int actionPageSize)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            string query = $@"INSERT OR REPLACE INTO {tableName + "ActionPage"}
            (DefaultActionPageName, ActionProcess, ActionPageName, LastEditTime, ActionPageSize)
            VALUES
            (@DefaultActionPageName, @ActionProcess, @ActionPageName, @LastEditTime, @ActionPageSize)"; // 更新场景数据表的SQL语句
            using var command = new SQLiteCommand(query, connection, transaction); // 创建SQLiteCommand对象
            command.Parameters.AddWithValue("@DefaultActionPageName", defaultActionPageName); // 动作页内置默认名称
            command.Parameters.AddWithValue("@ActionProcess", actionProcess); // 场景类型
            command.Parameters.AddWithValue("@ActionPageName", actionPageName); // 场景图标路径
            command.Parameters.AddWithValue("@LastEditTime", DateTime.Now); // 场景数量
            command.Parameters.AddWithValue("@ActionPageSize", actionPageSize); // 场景标签
            command.ExecuteNonQuery(); // 执行更新表的SQL语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 删除场景数据表
        /// </summary>
        /// <param name="tableName"> 场景数据表名称 </param>
        public void DeleteSceneTable(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand($"DROP TABLE IF EXISTS {tableName + "Scene"}", connection); // 创建命令对象
            command.ExecuteNonQuery(); // 执行删除表格语句
        }

        /// <summary>
        /// 删除动作页数据
        /// </summary>
        /// <param name="tableName"> 动作页数据表名称 </param>
        /// <param name="actionPageIndex"> 动作页索引 </param>
        public void DeleteActionPage(string tableName, int actionPageIndex)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            string query = $@"DELETE FROM {tableName + "ActionPage"} WHERE DefaultActionPageName = @DefaultActionPageName"; // 删除动作页数据表的SQL语句
            using var command1 = new SQLiteCommand(query, connection, transaction); // 创建SQLiteCommand对象
            command1.Parameters.AddWithValue("@DefaultActionPageName", tableName + actionPageIndex.ToString()); // 添加参数
            command1.ExecuteNonQuery(); // 执行删除表的SQL语句

            string selectQuery = $@"SELECT DefaultActionPageName FROM {tableName + "ActionPage"} 
                    ORDER BY CAST(SUBSTR(DefaultActionPageName, LENGTH(@TableName) + 1) AS INTEGER)"; // 获取所有剩余动作页的 SQL语句
            using var selectCommand = new SQLiteCommand(selectQuery, connection, transaction); // 创建SQLiteCommand对象
            selectCommand.Parameters.AddWithValue("@TableName", tableName); // 设置参数
            using var reader = selectCommand.ExecuteReader(); // 执行查询SQL语句
            List<string> defaultNames = new List<string>(); // 所有剩余动作页的 DefaultActionPageName
            while (reader.Read())
            {
                defaultNames.Add(reader.GetString(0)); // 获取所有剩余动作页的 DefaultActionPageName
            }

            for (int i = 0; i < defaultNames.Count; i++) // 重新编号并更新每个动作页的 DefaultActionPageName
            {
                string oldDefaultActionPageName = defaultNames[i]; // 获取旧名称
                string newDefaultActionPageName = $"{tableName}{i}"; // 设置新名称
                if (oldDefaultActionPageName != newDefaultActionPageName)
                {
                    string updateQuery = $@"UPDATE {tableName + "ActionPage"}
                            SET DefaultActionPageName = @NewDefaultActionPageName
                            WHERE DefaultActionPageName = @OldDefaultActionPageName"; // 更新动作页数据表的SQL语句
                    using var updateCommand = new SQLiteCommand(updateQuery, connection, transaction); // 创建SQLiteCommand对象
                    updateCommand.Parameters.AddWithValue("@NewDefaultActionPageName", newDefaultActionPageName); // 设置新名称
                    updateCommand.Parameters.AddWithValue("@OldDefaultActionPageName", oldDefaultActionPageName); // 设置旧名称
                    updateCommand.ExecuteNonQuery(); // 执行更新表的SQL语句
                }
            }
            query = $@"UPDATE {tableName + "Scene"} SET SceneCount = SceneCount - 1 WHERE SceneType = @SceneType"; // 更新场景数据表的SQL语句
            using var command2 = new SQLiteCommand(query, connection, transaction); // 创建SQLiteCommand对象
            command2.Parameters.AddWithValue("@SceneType", tableName); // 设置场景类型
            command2.ExecuteNonQuery(); // 执行更新表的SQL语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 删除动作页数据表
        /// </summary>
        /// <param name="tableName"> 动作页数据表名称 </param>
        public void DeleteActionPageTable(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand($"DROP TABLE IF EXISTS {tableName + "ActionPage"}", connection); // 创建命令对象
            command.ExecuteNonQuery(); // 执行删除表格语句
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
            string selectQuery = $"SELECT * FROM {tableName + "Scene"};"; // 获取场景数据表的SQL语句
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
                }); // 添加场景数据
            }
            return conditions; // 返回场景数据表
        }

        /// <summary>
        /// 获取动作页数据
        /// </summary>
        /// <param name="tableName"> 动作页数据表名称 </param>
        /// <param name="actionPageIndex"> 动作页索引 </param>
        /// <returns> 动作页数据 </returns>
        public ActionPageData GetActionPageData(string tableName, int actionPageIndex)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string selectQuery = $"SELECT * FROM {tableName + "ActionPage"} WHERE DefaultActionPageName = @DefaultActionPageName"; // 获取动作页数据表的SQL语句
            using var command = new SQLiteCommand(selectQuery, connection); // 创建SQLiteCommand对象
            command.Parameters.AddWithValue("@DefaultActionPageName", tableName + actionPageIndex.ToString()); // 添加参数
            using var reader = command.ExecuteReader(); // 执行查询SQL语句
            if (reader.Read())
            {
                return new ActionPageData
                {
                    DefaultActionPageName = reader.GetString(0), // 内部默认的动作页名称，例如“Global0”
                    ActionProcess = reader.GetString(1), // 动作页所属程序名称
                    ActionPageName = reader.GetString(2), // 动作页名称
                    LastEditTime = reader.GetDateTime(3), // 最后编辑时间
                    ActionPageSize = reader.GetInt32(4), // 动作页大小
                }; // 返回动作页数据
            }
            return null; // 返回空
        }

        /// <summary>
        /// 交换两个动作页的数据
        /// </summary>
        /// <param name="tableName"> 动作页数据表名称 </param>
        /// <param name="index1"> 动作页索引1 </param>
        /// <param name="index2"> 动作页索引2 </param>
        public void SwapActionPage(string tableName, int index1, int index2)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开启事务
            try
            {
                // 获取两个动作页的当前 DefaultActionPageName
                string actionPage1Name = $"{tableName}{index1}";
                string actionPage2Name = $"{tableName}{index2}";
                string tempName = $"{tableName}_temp_swap"; // 生成临时名称

                // 更新第一个动作页为临时名称
                string updateQuery = $@"UPDATE {tableName + "ActionPage"}
                                SET DefaultActionPageName = @TempName
                                WHERE DefaultActionPageName = @ActionPage1Name"; // 更新第一个动作页的SQL语句
                using var updateCommand1 = new SQLiteCommand(updateQuery, connection, transaction); // 创建SQLiteCommand对象
                updateCommand1.Parameters.AddWithValue("@TempName", tempName); // 设置临时名称
                updateCommand1.Parameters.AddWithValue("@ActionPage1Name", actionPage1Name); // 设置第一个动作页名称
                updateCommand1.ExecuteNonQuery(); // 执行更新表的SQL语句

                // 更新第二个动作页为第一个动作页的原始名称
                updateQuery = $@"UPDATE {tableName + "ActionPage"}
                        SET DefaultActionPageName = @ActionPage1Name
                        WHERE DefaultActionPageName = @ActionPage2Name"; // 更新第二个动作页的SQL语句
                using var updateCommand2 = new SQLiteCommand(updateQuery, connection, transaction); // 创建SQLiteCommand对象
                updateCommand2.Parameters.AddWithValue("@ActionPage1Name", actionPage1Name); // 设置第一个动作页名称
                updateCommand2.Parameters.AddWithValue("@ActionPage2Name", actionPage2Name); // 设置第二个动作页名称
                updateCommand2.ExecuteNonQuery(); // 执行更新表的SQL语句

                // 更新第一个动作页为第二个动作页的原始名称
                updateQuery = $@"UPDATE {tableName + "ActionPage"}
                        SET DefaultActionPageName = @ActionPage2Name
                        WHERE DefaultActionPageName = @TempName"; // 更新第一个动作页的SQL语句
                using var updateCommand3 = new SQLiteCommand(updateQuery, connection, transaction); // 创建SQLiteCommand对象
                updateCommand3.Parameters.AddWithValue("@ActionPage2Name", actionPage2Name); // 设置第二个动作页名称
                updateCommand3.Parameters.AddWithValue("@TempName", tempName); // 设置临时名称
                updateCommand3.ExecuteNonQuery(); // 执行更新表的SQL语句
                transaction.Commit(); // 提交事务
            }
            catch
            {
                transaction.Rollback(); // 回滚事务
            }
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
        public string DefaultActionPageName { get; set; } // 内部默认的动作页名称，例如“Global0”
        public string ActionProcess { get; set; } // 动作页所属程序名称
        public string ActionPageName { get; set; } // 动作页名称
        public DateTime LastEditTime { get; set; } // 最后编辑时间
        public int ActionPageSize { get; set; } // 动作页大小
    }
}