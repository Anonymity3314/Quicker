using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using System.IO;

namespace Quicker.Database
{
    public class ButtonDatabase
    {
        private readonly string dbPath2 = "Data Source=Button.db;Pooling=true;Max Pool Size=100;Journal Mode=Wal;";

        // 初始化数据库
        public void Initialize()
        {
            if (File.Exists("Button.db")) // 如果数据库存在
            {
                CheckAndUpdateDatabase(); // 检查并更新数据库
                return; // 数据库已存在，不再初始化
            }

            SQLiteConnection.CreateFile("Button.db"); // 创建数据库文件
            CreateButtonTable("Global"); // 创建全局表格
            CreateButtonTable("Common"); // 创建通用表格
        }

        // 检查并更新数据库
        private void CheckAndUpdateDatabase()
        {

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
                ButtonName TEXT,
                Location TEXT,
                ImagePath TEXT,
                RunByMessager BOOL,
                TryToOpenExitingWindow BOOL,
                WindowState INT,
                Usage TEXT,
                CreateTime DATETIME,
                LatestEditTime DATETIME,
                Type TEXT
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
            (ButtonID, ButtonName, Location, ImagePath, RunByMessager, TryToOpenExitingWindow, WindowState, Usage, CreateTime, LatestEditTime, Type) 
            VALUES 
            (@ButtonID, @ButtonName, @Location, @ImagePath, @RunByMessager, @TryToOpenExitingWindow, @WindowState, @Usage, @CreateTime, @LatestEditTime, @Type)"; // 创建SQL语句
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID); // 动作ID
            command.Parameters.AddWithValue("@ButtonName", buttonData.ButtonName); // 动作名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 图片路径
            command.Parameters.AddWithValue("@RunByMessager", buttonData.RunByMessager); // 是否用管理员身份运行
            command.Parameters.AddWithValue("@TryToOpenExitingWindow", buttonData.TryToOpenExitingWindow); // 是否尝试打开已有窗口
            command.Parameters.AddWithValue("@WindowState", buttonData.WindowState); // 窗口状态
            command.Parameters.AddWithValue("@Usage", buttonData.Usage); // 用途
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 最近修改时间
            command.Parameters.AddWithValue("@Type", buttonData.Type); // 类型
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
                    ButtonName = reader.GetString(1), // 动作名称
                    Location = reader.GetString(2), // 位置
                    ImagePath = reader.GetString(3), // 图片路径
                    RunByMessager = reader.GetBoolean(4), // 是否用管理员身份运行
                    TryToOpenExitingWindow = reader.GetBoolean(5), // 是否尝试打开已有窗口
                    WindowState = reader.GetInt32(6), // 窗口状态
                    Usage = reader.GetString(7), // 用途
                    CreateTime = reader.GetDateTime(8), // 创建时间
                    LatestEditTime = reader.GetDateTime(9), // 最近修改时间
                    Type = reader.IsDBNull(10) ? null : reader.GetString(10) // 类型
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
                ButtonName = @ButtonName, 
                Location = @Location, 
                ImagePath = @ImagePath, 
                RunByMessager = @RunByMessager, 
                TryToOpenExitingWindow = @TryToOpenExitingWindow, 
                WindowState = @WindowState, 
                Usage = @Usage, 
                CreateTime = @CreateTime, 
                LatestEditTime = @LatestEditTime 
            WHERE ButtonID = @ButtonID"; // 更新指定表中的数据
            using var command = new SQLiteCommand(query, connection); // 创建命令对象
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID); // 动作ID
            command.Parameters.AddWithValue("@ButtonName", buttonData.ButtonName); // 动作名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 图片路径
            command.Parameters.AddWithValue("@RunByMessager", buttonData.RunByMessager); // 是否用管理员身份运行
            command.Parameters.AddWithValue("@TryToOpenExitingWindow", buttonData.TryToOpenExitingWindow); // 是否尝试打开已有窗口
            command.Parameters.AddWithValue("@WindowState", buttonData.WindowState); // 窗口状态
            command.Parameters.AddWithValue("@Usage", buttonData.Usage); // 用途
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
                    ButtonName = reader.GetString(1),
                    Location = reader.GetString(2),
                    ImagePath = reader.GetString(3),
                    RunByMessager = reader.GetBoolean(4),
                    TryToOpenExitingWindow = reader.GetBoolean(5),
                    WindowState = reader.GetInt32(6),
                    Usage = reader.GetString(7),
                    CreateTime = reader.GetDateTime(8),
                    LatestEditTime = reader.GetDateTime(9),
                    Type = reader.GetString(10)
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
            (ButtonID, ButtonName, Location, ImagePath, RunByMessager, TryToOpenExitingWindow, WindowState, Usage, CreateTime, LatestEditTime, Type) 
            VALUES 
            (@ButtonID, @ButtonName, @Location, @ImagePath, @RunByMessager, @TryToOpenExitingWindow, @WindowState, @Usage, @CreateTime, @LatestEditTime, @Type)"; // 创建SQL语句
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@ButtonID", buttonID); // 动作ID
            command.Parameters.AddWithValue("@ButtonName", buttonData.ButtonName); // 动作名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 图片路径
            command.Parameters.AddWithValue("@RunByMessager", buttonData.RunByMessager); // 是否用管理员身份运行
            command.Parameters.AddWithValue("@TryToOpenExitingWindow", buttonData.TryToOpenExitingWindow); // 是否尝试打开已有窗口
            command.Parameters.AddWithValue("@WindowState", buttonData.WindowState); // 窗口状态
            command.Parameters.AddWithValue("@Usage", buttonData.Usage); // 用途
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 最近修改时间
            command.Parameters.AddWithValue("@Type", buttonData.Type); // 类型
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
            using var connection = OpenConnection(); // 打开连接
            using var transaction = connection.BeginTransaction(); // 开始事务

            var buttonDataList = GetButtonDataByPrefix(inputString);// 获取所有符合条件的按钮数据
            var buttonsToUpdate = new List<(string ButtonID, int OriginalA, string BcPart)>(); // 存储需要更新的按钮信息
            foreach (var buttonData in buttonDataList)
            {
                Match match = Regex.Match(buttonData.ButtonID, @"^(\w+)(\d{3})$"); // 匹配 ButtonID 中的 A、B、C 部分
                if (!match.Success) continue; // 格式不匹配，跳过

                string aPart = match.Groups[1].Value; // A 部分
                string bcPart = match.Groups[2].Value; // B、C 部分
                if (aPart != inputString) continue; // 表名不匹配，跳过

                int currentA = int.Parse(bcPart[0].ToString()); // 获取当前 A 部分
                if (currentA == a1 || currentA == a2) // 如果 A 部分符合条件，则添加到待更新列表
                    buttonsToUpdate.Add((buttonData.ButtonID, currentA, bcPart)); // 添加到待更新列表
            }

            foreach (var (buttonID, originalA, bcPart) in buttonsToUpdate) // 为需要更新的按钮生成新的 ButtonID
            {
                int newA = originalA == a1 ? a2 : a1; // 交换 A 部分
                string newButtonID = $"{inputString}{newA}{bcPart.Substring(1)}"; // 生成新的 ButtonID
                UpdateButtonID(connection, GetTableNameFromButtonID(buttonID), buttonID, newButtonID); // 更新 ButtonID
            }
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 使用正则表达式从 ButtonID 提取表名
        /// </summary>
        /// <param name="buttonID"> ButtonID </param>
        /// <returns> 表名 </returns>
        private string GetTableNameFromButtonID(string buttonID)
        {
            Match match = Regex.Match(buttonID, @"^(\w+)(\d{3})$"); // 匹配 ButtonID 格式
            return match.Groups[1].Value; // 返回表名
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
            using var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name = @TableName;",connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        // 打开数据库连接
        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(dbPath2); // 创建 SQLiteConnection 对象
            connection.Open(); // 打开数据库连接
            return connection; // 返回打开的连接
        }
    }

    // ButtonData 类
    public class ButtonData
    {
        public string ButtonID { get; set; } // 动作ID，通常为Button的名称
        public string ButtonName { get; set; } // 动作名称
        public string Location { get; set; } // 位置
        public string ImagePath { get; set; } // 图片路径
        public bool RunByMessager { get; set; } // 是否用管理员身份运行
        public bool TryToOpenExitingWindow { get; set; } // 是否尝试打开已有窗口
        public int WindowState { get; set; } // 窗口状态
        public string Usage { get; set; } // 用途(对动作的描述)
        public DateTime CreateTime { get; set; } // 创建时间
        public DateTime LatestEditTime { get; set; } // 最近修改时间
        public string Type { get; set; } // 类型
    }
}