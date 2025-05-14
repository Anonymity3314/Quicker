using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Text;
using System.IO;

namespace Quicker.Database
{
    public class ButtonDatabase
    {
        // 获取应用程序根目录，并设置数据库文件路径为根目录下的"Database"文件夹
        private readonly string db2 = "Data Source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Button.db") + ";Pooling=true;Max Pool Size=100;Journal Mode=Wal;";

        public ButtonDatabase()
        {
            Initialize(); // 初始化数据库
        }

        // 初始化数据库
        private void Initialize()
        {
            string dbFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database"); // 获取数据库文件夹路径
            string dbFilePath = Path.Combine(dbFolder, "Button.db"); // 设置数据库文件路径
            if (!Directory.Exists(dbFolder)) Directory.CreateDirectory(dbFolder); // 如果"Database"文件夹不存在，则创建它
            if (!File.Exists(dbFilePath))
            {
                SQLiteConnection.CreateFile(dbFilePath); // 创建数据库文件
                CreateButtonTable("Global"); // 创建全局表格
                CreateButtonTable("Common"); // 创建通用表格
            }
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
            using var command = new SQLiteCommand(createTableQuery, connection); // 创建命令对象
            command.ExecuteNonQuery(); // 执行创建表格语句
        }

        /// <summary>
        /// 添加新动作到对应表中
        /// </summary>
        /// <param name="buttonData">要添加的动作数据</param>
        public void AddAction(ButtonData buttonData)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            string tableName = GetTableNameFromButtonID(buttonData.ButtonID); // 从ButtonID解析表名
            CheckAndCreateTable(tableName, connection); // 检查表是否存在，不存在则创建
            string query = $@"INSERT INTO {tableName} 
            (ButtonID, Title, Location, ImagePath, Data1, Data2, Data3, Description, CreateTime, LatestEditTime, ActionType) 
            VALUES 
            (@ButtonID, @Title, @Location, @ImagePath, @Data1, @Data2, @Data3, @Description, @CreateTime, @LatestEditTime, @ActionType)"; // 创建SQL语句
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID); // 动作ID
            command.Parameters.AddWithValue("@Title", buttonData.Title); // 动作名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 图片路径
            command.Parameters.AddWithValue("@Data1", buttonData.Data1); // 是否用管理员身份运行
            command.Parameters.AddWithValue("@Data2", buttonData.Data2); // 是否尝试打开已有窗口
            command.Parameters.AddWithValue("@Data3", buttonData.Data3); // 窗口状态
            command.Parameters.AddWithValue("@Description", buttonData.Description); // 用途
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 最近修改时间
            command.Parameters.AddWithValue("@ActionType", buttonData.ActionType); // 类型
            command.ExecuteNonQuery(); // 执行插入语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 通过ButtonID从对应表中获取数据
        /// </summary>
        /// <param name="buttonID"> 要获取数据的ButtonID </param>
        /// <returns> ButtonData对象，如果找不到则返回null </returns>
        public ButtonData GetButtonDataByID(string buttonID)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            string tableName = GetTableNameFromButtonID(buttonID); // 从ButtonID解析表名
            using var command = new SQLiteCommand($"SELECT * FROM {tableName} WHERE ButtonID = @ButtonID", connection); // 创建命令对象
            command.Parameters.AddWithValue("@ButtonID", buttonID); // 动作ID
            using var reader = command.ExecuteReader(); // 执行查询语句
            if (reader.Read())
            {
                return new ButtonData
                {
                    ButtonID = reader.GetString(0), // 动作ID
                    Title = reader.GetString(1), // 动作名称
                    Location = reader.GetString(2), // 位置
                    ImagePath = reader.GetString(3), // 图片路径
                    Data1 = reader.GetString(4), // 是否用管理员身份运行
                    Data2 = reader.GetString(5), // 是否尝试打开已有窗口
                    Data3 = reader.GetString(6), // 窗口状态
                    Description = reader.GetString(7), // 用途
                    CreateTime = reader.GetDateTime(8), // 创建时间
                    LatestEditTime = reader.GetDateTime(9), // 最近修改时间
                    ActionType = reader.IsDBNull(10) ? null : reader.GetString(10) // 类型
                };
            }
            return null; // 没有找到数据
        }

        /// <summary>
        /// 更新对应表中的动作数据
        /// </summary>
        /// <param name="buttonData"> 要更新的动作数据 </param>
        public void UpdateAction(ButtonData buttonData)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            string tableName = GetTableNameFromButtonID(buttonData.ButtonID); // 从ButtonID解析表名
            string query = $@"UPDATE {tableName} SET 
                Title = @Title, 
                Location = @Location, 
                ImagePath = @ImagePath, 
                Data1 = @Data1, 
                Data2 = @Data2, 
                Data3 = @Data3, 
                Description = @Description, 
                CreateTime = @CreateTime, 
                LatestEditTime = @LatestEditTime 
            WHERE ButtonID = @ButtonID"; // 更新指定表中的数据
            using var command = new SQLiteCommand(query, connection); // 创建命令对象
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID); // 动作ID
            command.Parameters.AddWithValue("@Title", buttonData.Title); // 动作名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 图片路径
            command.Parameters.AddWithValue("@Data1", buttonData.Data1); // 是否用管理员身份运行
            command.Parameters.AddWithValue("@Data2", buttonData.Data2); // 是否尝试打开已有窗口
            command.Parameters.AddWithValue("@Data3", buttonData.Data3); // 窗口状态
            command.Parameters.AddWithValue("@Description", buttonData.Description); // 用途
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 最近修改时间
            command.ExecuteNonQuery(); // 执行更新语句
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 通过Button前缀读取对应表格中所有ButtonData
        /// </summary>
        /// <param name="prefix"> Button前缀 </param>
        /// <returns> ButtonData列表 </returns>
        public List<ButtonData> GetButtonDataByPrefix(string prefix)
        {
            var buttonDataList = new List<ButtonData>();
            using var connection = OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand($"SELECT * FROM {prefix}", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                buttonDataList.Add(new ButtonData
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
                    ActionType = reader.GetString(10)
                });
            }
            return buttonDataList; // 返回ButtonData列表
        }

        /// <summary>
        /// 删除动作
        /// </summary>
        /// <param name="buttonID">要删除的动作ID</param>
        public void DeleteAction(string buttonID)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            string tableName = GetTableNameFromButtonID(buttonID); // 获取表名
            using var command = new SQLiteCommand($@"DELETE FROM {tableName} WHERE ButtonID = @ButtonID", connection); // 创建命令
            command.Parameters.AddWithValue("@ButtonID", buttonID); // 绑定参数
            command.ExecuteNonQuery(); // 执行命令
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 根据不同情况更改 Button 数据库
        /// </summary>
        /// <param name="buttonID1"> ButtonID1 </param>
        /// <param name="buttonID2"> ButtonID2 </param>
        public void ExchangeButtonID(string buttonID1, string buttonID2)
        {
            using var connection = OpenConnection(); // 打开连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            var data1 = GetButtonDataByID(buttonID1); // 获取 ButtonID1 的数据
            var data2 = GetButtonDataByID(buttonID2); // 获取 ButtonID2 的数据

            string tempButtonID = "temp_"; // 临时ButtonID
            string tableName1 = GetTableNameFromButtonID(buttonID1); // 从ButtonID解析表名
            string tableName2 = GetTableNameFromButtonID(buttonID2); // 从ButtonID解析表名
            if (data2 != null) // 直接交换 ButtonID
            {
                UpdateButtonID(connection, tableName1, buttonID1, tempButtonID); // 将 ButtonID1 的编号改为临时编号
                UpdateButtonID(connection, tableName2, buttonID2, buttonID1); // 将 ButtonID2 的编号改为 ButtonID1
                UpdateButtonID(connection, tableName1, tempButtonID, buttonID2); // 将临时编号改为 ButtonID2
                transaction.Commit();
                if (tableName1 != tableName2) // 表名不同，迁移到对应表
                {
                    MoveButtonDataToNewTable(buttonID2, data1, tableName1, tableName2); // 迁移数据到新表
                    MoveButtonDataToNewTable(buttonID1, data2, tableName2, tableName1); // 迁移数据到旧表
                }
            }
            else // 将 ButtonID1 的编号改为 ButtonID2
            {
                UpdateButtonID(connection, tableName1, buttonID1, buttonID2); // 更新 ButtonID1 的编号
                transaction.Commit();
                if (tableName1 != tableName2) // 表名不同，迁移到对应表
                    MoveButtonDataToNewTable(buttonID2, data1, tableName1, tableName2); // 迁移数据到新表
            }
        }

        /// <summary>
        /// 更新 ButtonID
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="tableName"> 数据库表名 </param>
        /// <param name="oldButtonID"> 要更改的 ButtonID </param>
        /// <param name="newButtonID"> 目标 ButtonID </param>
        private void UpdateButtonID(SQLiteConnection connection, string tableName, string oldButtonID, string newButtonID)
        {
            using var command = new SQLiteCommand($@"UPDATE {tableName} SET ButtonID = @NewButtonID WHERE ButtonID = @OldButtonID", connection);
            command.Parameters.AddWithValue("@NewButtonID", newButtonID);
            command.Parameters.AddWithValue("@OldButtonID", oldButtonID);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 将按钮数据迁移到新表
        /// </summary>
        /// <param name="buttonID">按钮ID</param>
        /// <param name="sourceTable">源表名</param>
        /// <param name="targetTable">目标表名</param>
        private void MoveButtonDataToNewTable(string buttonID, ButtonData buttonData, string sourceTable, string targetTable)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务

            // 插入数据到目标表
            string query = $@"INSERT INTO {targetTable} 
            (ButtonID, Title, Location, ImagePath, Data1, Data2, Data3, Description, CreateTime, LatestEditTime, ActionType) 
            VALUES 
            (@ButtonID, @Title, @Location, @ImagePath, @Data1, @Data2, @Data3, @Description, @CreateTime, @LatestEditTime, @ActionType)"; // 创建SQL语句
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ButtonID", buttonID); // 动作ID
            command.Parameters.AddWithValue("@Title", buttonData.Title); // 动作名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 图片路径
            command.Parameters.AddWithValue("@Data1", buttonData.Data1); // 是否用管理员身份运行
            command.Parameters.AddWithValue("@Data2", buttonData.Data2); // 是否尝试打开已有窗口
            command.Parameters.AddWithValue("@Data3", buttonData.Data3); // 窗口状态
            command.Parameters.AddWithValue("@Description", buttonData.Description); // 用途
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 最近修改时间
            command.Parameters.AddWithValue("@ActionType", buttonData.ActionType); // 类型
            command.ExecuteNonQuery(); // 执行插入语句
            transaction.Commit(); // 提交事务

            // 从源表删除数据
            using var deleteCommand = new SQLiteCommand($@"DELETE FROM {sourceTable} WHERE ButtonID = @ButtonID", connection);
            deleteCommand.Parameters.AddWithValue("@ButtonID", buttonID);
            deleteCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// 根据输入的字符串和数字 A1、A2，交换符合条件的 ButtonID 的 A 部分
        /// </summary>
        /// <param name="inputString">Button的字符串索引</param>
        /// <param name="a1"> A1 部分 </param>
        /// <param name="a2"> A2 部分 </param>
        public void SwapButtonAValues(string inputString, int a1, int a2)
        {
            List<ButtonData> allButtons = GetButtonDataByPrefix(inputString); // 获取所有 Button 数据
            var buttonIDMap = allButtons
                .Select(b => new { ButtonID = b.ButtonID, Data = b })
                .Where(b => Regex.IsMatch(b.ButtonID, @"^(\w+)(\d{3})$") &&
                            Regex.Match(b.ButtonID, @"^(\w+)(\d{3})$").Groups[1].Value == inputString &&
                            (int.Parse(Regex.Match(b.ButtonID, @"^(\w+)(\d{3})$").Groups[2].Value[0].ToString()) == a1 ||
                             int.Parse(Regex.Match(b.ButtonID, @"^(\w+)(\d{3})$").Groups[2].Value[0].ToString()) == a2))
                .ToDictionary(b => b.ButtonID, b => b.Data); // 筛选出符合条件的 ButtonID

            if (buttonIDMap.Count == 0) return; // 没有符合条件的 ButtonID，直接返回

            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务

            string tempPrefix = $"temp_{Guid.NewGuid():N}_"; // 生成临时标识符前缀
            foreach (var pair in buttonIDMap.ToList()) // 更新 A1 部分的 ButtonID 为临时标识符
            {
                string buttonID = pair.Key; // 原 ButtonID
                Match match = Regex.Match(buttonID, @"^(\w+)(\d{3})$"); // 匹配 ButtonID
                if (match.Success && int.Parse(match.Groups[2].Value[0].ToString()) == a1)
                {
                    string newButtonID = $"{tempPrefix}{match.Groups[2].Value}"; // 新 ButtonID
                    UpdateButtonID(connection, inputString, buttonID, newButtonID); // 更新 ButtonID
                    buttonIDMap.Remove(buttonID); // 从字典中删除原数据
                    buttonIDMap[newButtonID] = pair.Value; // 添加新数据
                }
            }

            foreach (var pair in buttonIDMap.ToList())// 更新 A2 部分的 ButtonID 为目标 ID
            {
                string buttonID = pair.Key; // 原 ButtonID
                Match match = Regex.Match(buttonID, @"^(\w+)(\d{3})$"); // 匹配 ButtonID
                if (match.Success && int.Parse(match.Groups[2].Value[0].ToString()) == a2)
                {
                    string bcPart = match.Groups[2].Value.Substring(1); // 目标 ID 的 B 和 C 部分
                    string newButtonID = $"{inputString}{a1}{bcPart}"; // 新 ButtonID
                    UpdateButtonID(connection, inputString, buttonID, newButtonID); // 更新 ButtonID
                    buttonIDMap.Remove(buttonID); // 从字典中删除原数据
                    buttonIDMap[newButtonID] = pair.Value; // 添加新数据
                }
            }

            foreach (var pair in buttonIDMap.ToList())// 更新临时标识符的 ButtonID 为目标 ID
            {
                string buttonID = pair.Key; // 原 ButtonID
                if (buttonID.StartsWith(tempPrefix))
                {
                    string bcPart = buttonID.Substring(tempPrefix.Length + 1); // 目标 ID 的 B 和 C 部分
                    string newButtonID = $"{inputString}{a2}{bcPart}"; // 新 ButtonID
                    UpdateButtonID(connection, inputString, buttonID, newButtonID); // 更新 ButtonID
                }
            }
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 使用正则表达式从 ButtonID 提取表名
        /// </summary>
        /// <param name="buttonID"> ButtonID </param>
        /// <returns> 表名 </returns>
        public string GetTableNameFromButtonID(string buttonID)
        {
            Match match = Regex.Match(buttonID, @"^(\w+)(\d{3})$"); // 匹配 ButtonID 格式
            return match.Groups[1].Value; // 返回表名
        }

        /// <summary>
        /// 检查表是否存在，不存在则创建
        /// </summary>
        /// <param name="tableName"> 要检查的表名 </param>
        /// <param name="connection"> 数据库连接 </param>
        public void CheckAndCreateTable(string tableName, SQLiteConnection connection)
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
            using var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name = @TableName;",connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <returns> 数据库连接 </returns>
        public SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(db2); // 打开数据库连接
            connection.Open(); // 打开数据库连接
            return connection; // 返回数据库连接
        }
    }

    // ButtonData 类
    public class ButtonData
    {
        public string ButtonID { get; set; } // 动作ID，通常为Button的名称
        public string Title { get; set; } // 动作名称
        public string Location { get; set; } // 位置
        public string ImagePath { get; set; } // 图片路径
        public string Data1 { get; set; } // 替代 RunByMessager
        public string Data2 { get; set; } // 替代 TryToOpenExitingWindow
        public string Data3 { get; set; } // 替代 WindowState
        public string Description { get; set; } // 对动作的描述
        public DateTime CreateTime { get; set; } // 创建时间
        public DateTime LatestEditTime { get; set; } // 最近修改时间
        public string ActionType { get; set; } // 类型
    }
}