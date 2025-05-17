using System.Data.SQLite;
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
        /// 获取数据库中所有表名
        /// </summary>
        /// <returns> 表名列表 </returns>
        public List<string> GetAllTableNames()
        {
            var tableNames = new List<string>();
            using var connection = OpenConnection();
            using var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tableNames.Add(reader.GetString(0));
            }
            return tableNames;
        }

        /// <summary>
        /// 添加新动作到对应表中
        /// </summary>
        /// <param name="buttonData">要添加的动作数据</param>
        public void AddAction(ButtonData buttonData)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            string tableName = buttonData.ButtonID.Substring(0, buttonData.ButtonID.Length - 3); // 从ButtonID解析表名
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
            command.Parameters.AddWithValue("@Data1", buttonData.Data1); // 动作数据1
            command.Parameters.AddWithValue("@Data2", buttonData.Data2); // 动作数据2
            command.Parameters.AddWithValue("@Data3", buttonData.Data3); // 动作数据3
            command.Parameters.AddWithValue("@Description", buttonData.Description); // 对动作的描述
            command.Parameters.AddWithValue("@CreateTime", DateTime.Now); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", DateTime.Now); // 最近修改时间
            command.Parameters.AddWithValue("@ActionType", buttonData.ActionType); // 动作类型
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
            string tableName = buttonID.Substring(0, buttonID.Length - 3); // 从ButtonID解析表名
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
                    Data1 = reader.IsDBNull(4) ? null : reader.GetString(4), // 动作数据1
                    Data2 = reader.IsDBNull(5) ? null : reader.GetString(5), // 动作数据2
                    Data3 = reader.IsDBNull(6) ? null : reader.GetString(6), // 动作数据3
                    Description = reader.GetString(7), // 对动作的描述
                    CreateTime = reader.GetDateTime(8), // 创建时间
                    LatestEditTime = reader.GetDateTime(9), // 最近修改时间
                    ActionType = reader.IsDBNull(10) ? null : reader.GetString(10) // 动作类型
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
            string tableName = buttonData.ButtonID.Substring(0, buttonData.ButtonID.Length - 3); // 从ButtonID解析表名
            string query = $@"UPDATE {tableName} SET 
                Title = @Title, 
                Location = @Location, 
                ImagePath = @ImagePath, 
                Data1 = @Data1, 
                Data2 = @Data2, 
                Data3 = @Data3, 
                Description = @Description, 
                CreateTime = @CreateTime, 
                LatestEditTime = @LatestEditTime, 
                ActionType = @ActionType 
            WHERE ButtonID = @ButtonID"; // 更新指定表中的数据
            using var command = new SQLiteCommand(query, connection); // 创建命令对象
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID); // 动作ID
            command.Parameters.AddWithValue("@Title", buttonData.Title); // 动作名称
            command.Parameters.AddWithValue("@Location", buttonData.Location); // 位置
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath); // 图片路径
            command.Parameters.AddWithValue("@Data1", buttonData.Data1); // 动作数据1
            command.Parameters.AddWithValue("@Data2", buttonData.Data2); // 动作数据2
            command.Parameters.AddWithValue("@Data3", buttonData.Data3); // 动作数据3
            command.Parameters.AddWithValue("@Description", buttonData.Description); // 对动作的描述
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 最近修改时间
            command.Parameters.AddWithValue("@ActionType", buttonData.ActionType); // 动作类型
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
                    ActionType = reader.GetString(10) // 动作类型
                });
            }
            return buttonDataList; // 返回ButtonData列表
        }

        /// <summary>
        /// 删除按钮数据表
        /// </summary>
        /// <param name="tableName"> 要删除的表名 </param>
        public void DeleteButtonTable(string tableName)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var command = new SQLiteCommand($"DROP TABLE IF EXISTS {tableName}", connection); // 创建命令对象
            command.ExecuteNonQuery(); // 执行删除表格语句
        }

        /// <summary>
        /// 删除一整页的按钮
        /// </summary>
        /// <param name="prefix"> Button前缀 </param>
        /// <param name="pageIndex"> 页码 </param>
        public void DeletePageOfButtons(string prefix, int pageIndex)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务

            List<ButtonData> allButtons = GetButtonDataByPrefix(prefix); // 获取所有按钮数据
            var currentButtons = allButtons
                .Select(b => new { ButtonID = b.ButtonID, Data = b })
                .Where(b =>
                {
                    string idWithoutPrefix = b.ButtonID.Replace(prefix, ""); // 去掉前缀
                    return int.Parse(idWithoutPrefix[idWithoutPrefix.Length - 3].ToString()) == pageIndex; // 当前页的按钮
                })
                .ToDictionary(b => b.ButtonID, b => b.Data); // 当前页的按钮

            // 删除当前页的按钮
            foreach (var buttonData in currentButtons.Values)
            {
                DeleteAction(buttonData.ButtonID); // 删除动作
            }

            // 筛选并更新后续页的按钮编号
            var subsequentButtons = allButtons
                .Select(b => new { ButtonID = b.ButtonID, Data = b })
                .Where(b =>
                {
                    string idWithoutPrefix = b.ButtonID.Replace(prefix, ""); // 去掉前缀
                    return int.Parse(idWithoutPrefix[idWithoutPrefix.Length - 3].ToString()) > pageIndex; // 后续页的按钮
                })
                .ToList(); // 后续页的按钮

            foreach (var item in subsequentButtons)
            {
                string buttonID = item.ButtonID; // 原 ButtonID
                string newButtonID = UpdateButtonPageNumber(connection, buttonID, pageIndex); // 更新按钮的页码编号
                UpdateButtonID(connection, prefix, buttonID, newButtonID); // 更新 ButtonID
            }

            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 更新按钮的页码编号
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="buttonID">原 ButtonID </param>
        /// <param name="pageIndex">原页码</param>
        /// <returns>新的 ButtonID</returns>
        private string UpdateButtonPageNumber(SQLiteConnection connection, string buttonID, int pageIndex)
        {
            string prefix = buttonID.Substring(0, buttonID.Length - 3); // 获取前缀部分
            string idWithoutPrefix = buttonID.Substring(prefix.Length); // 直接获取去掉前缀的部分
            string pagePart = idWithoutPrefix.Substring(0, 3); // 获取前三位数字部分

            // 将三位数字部分拆分为页码、行号和列号
            int page = int.Parse(pagePart[0].ToString());
            string bcPart = idWithoutPrefix.Substring(1); // 目标 ID 的 B 和 C 部分
            if (page > pageIndex) page--; // 如果页码大于原页码，则页码减一

            // 重新组合新的三位数字部分
            string newPagePart = $"{page}{bcPart}";
            string newButtonID = $"{prefix}{newPagePart}";

            return newButtonID;
        }

        /// <summary>
        /// 删除动作
        /// </summary>
        /// <param name="buttonID">要删除的动作ID</param>
        public void DeleteAction(string buttonID)
        {
            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务
            string tableName = buttonID.Substring(0, buttonID.Length - 3); // 获取表名
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

            string tempButtonID = $"temp_{Guid.NewGuid().ToString("N")}"; // 生成临时 ButtonID
            string tableName1 = buttonID1.Substring(0, buttonID1.Length - 3); // 从ButtonID解析表名
            string tableName2 = buttonID2.Substring(0, buttonID2.Length - 3); // 从ButtonID解析表名
            if (data2 != null) // 直接交换 ButtonID
            {
                UpdateButtonID(connection, tableName1, buttonID1, tempButtonID); // 将 ButtonID1 的编号改为临时编号
                UpdateButtonID(connection, tableName2, buttonID2, buttonID1); // 将 ButtonID2 的编号改为 ButtonID1
                UpdateButtonID(connection, tableName1, tempButtonID, buttonID2); // 将临时编号改为 ButtonID2
                transaction.Commit(); // 提交事务
                if (tableName1 != tableName2) // 表名不同，迁移到对应表
                {
                    MoveButtonDataToNewTable(buttonID2, data1, tableName1, tableName2); // 迁移数据到新表
                    MoveButtonDataToNewTable(buttonID1, data2, tableName2, tableName1); // 迁移数据到旧表
                }
            }
            else // 将 ButtonID1 的编号改为 ButtonID2
            {
                UpdateButtonID(connection, tableName1, buttonID1, buttonID2); // 更新 ButtonID1 的编号
                transaction.Commit(); // 提交事务
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
            command.Parameters.AddWithValue("@NewButtonID", newButtonID); // 绑定参数
            command.Parameters.AddWithValue("@OldButtonID", oldButtonID); // 绑定参数
            command.ExecuteNonQuery(); // 执行更新语句
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
            command.Parameters.AddWithValue("@Data1", buttonData.Data1); // 动作数据1
            command.Parameters.AddWithValue("@Data2", buttonData.Data2); // 动作数据2
            command.Parameters.AddWithValue("@Data3", buttonData.Data3); // 动作数据3
            command.Parameters.AddWithValue("@Description", buttonData.Description); // 对动作的描述
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime); // 创建时间
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime); // 最近修改时间
            command.Parameters.AddWithValue("@ActionType", buttonData.ActionType); // 动作类型
            command.ExecuteNonQuery(); // 执行插入语句
            transaction.Commit(); // 提交事务

            // 从源表删除数据
            using var deleteCommand = new SQLiteCommand($@"DELETE FROM {sourceTable} WHERE ButtonID = @ButtonID", connection); // 创建命令
            deleteCommand.Parameters.AddWithValue("@ButtonID", buttonID); // 绑定参数
            deleteCommand.ExecuteNonQuery(); // 执行删除语句
        }

        /// <summary>
        /// 根据输入的字符串和数字 A，获取符合条件的 ButtonData
        /// </summary>
        /// <param name="prefix"> Button前缀 </param>
        /// <param name="a"> 数字 A </param>
        /// <returns> ButtonData列表 </returns>
        private List<ButtonData> MatchButtons(string prefix, int a)
        {
            List<ButtonData> buttonDatas = GetButtonDataByPrefix(prefix);
            var matchedButtons = buttonDatas
                .Where(b =>
                {
                    string idWithoutPrefix = b.ButtonID.Replace(prefix, "");
                    return int.Parse(idWithoutPrefix[idWithoutPrefix.Length - 3].ToString()) == a;
                })
                .ToList(); // 返回符合条件的 ButtonData 列表
            return matchedButtons;
        }

        /// <summary>
        /// 根据输入的字符串和数字 A1、A2，交换符合条件的 ButtonID 的 A 部分
        /// </summary>
        /// <param name="prefix">Button的字符串索引</param>
        /// <param name="a1"> A1 部分 </param>
        /// <param name="a2"> A2 部分 </param>
        public void SwapButtonAValues(string prefix, int a1, int a2)
        {
            var a1ButtonDatas = MatchButtons(prefix, a1); // 获取 A1 部分的 ButtonData
            var a2ButtonDatas = MatchButtons(prefix, a2); // 获取 A2 部分的 ButtonData
            if (a1ButtonDatas.Count == 0 && a2ButtonDatas.Count == 0) return; // 两个部分没有 ButtonData，直接返回

            using var connection = OpenConnection(); // 打开数据库连接
            using var transaction = connection.BeginTransaction(); // 开始事务

            string tempPrefix = $"temp_{Guid.NewGuid():N}_"; // 生成临时标识符前缀
            foreach (var buttonData in a1ButtonDatas.ToList()) // 更新 A1 部分的 ButtonID 为临时标识符
            {
                string buttonID = buttonData.ButtonID; // 原 ButtonID
                string idWithoutPrefix = buttonID.Replace(prefix, ""); // 获取ID部分，去掉前缀
                string newButtonID = $"{tempPrefix}{idWithoutPrefix}"; // 新 ButtonID
                UpdateButtonID(connection, prefix, buttonID, newButtonID); // 更新 ButtonID

                var index = a1ButtonDatas.IndexOf(buttonData); // 获取索引
                if (index != -1) a1ButtonDatas[index].ButtonID = newButtonID; // 更新字典中的 ButtonID
            }

            foreach (var buttonData in a2ButtonDatas.ToList()) // 更新 A2 部分的 ButtonID 为目标 ID
            {
                string buttonID = buttonData.ButtonID; // 原 ButtonID
                string idWithoutPrefix = buttonID.Replace(prefix, ""); // 获取ID部分，去掉前缀
                string bcPart = idWithoutPrefix.Substring(1); // 目标 ID 的 B 和 C 部分
                string newButtonID = $"{prefix}{a1}{bcPart}"; // 新 ButtonID
                UpdateButtonID(connection, prefix, buttonID, newButtonID); // 更新 ButtonID
            }

            foreach (var buttonData in a1ButtonDatas.ToList()) // 更新临时标识符的 ButtonID 为目标 ID
            {
                string buttonID = buttonData.ButtonID; // 原 ButtonID
                string idWithoutPrefix = buttonID.Replace(tempPrefix, ""); // 获取ID部分，去掉临时前缀
                string bcPart = idWithoutPrefix.Substring(1); // 目标 ID 的 B 和 C 部分
                string newButtonID = $"{prefix}{a2}{bcPart}"; // 新 ButtonID
                UpdateButtonID(connection, prefix, buttonID, newButtonID); // 更新 ButtonID
            }
            transaction.Commit(); // 提交事务
        }

        /// <summary>
        /// 获取总的页面数
        /// </summary>
        /// <param name="prefix"> 目标样式名称 </param>
        /// <returns> 总页面数 </returns>
        public int GetTotalAntionPageIndex(string prefix)
        {
            int TotalAntionPageIndex = 1; // 重置页面索引
            var buttonData = GetButtonDataByPrefix(prefix); // 从数据库中获取按钮数据
            foreach (var data in buttonData)
            {
                string buttonID = data.ButtonID; // 获取按钮ID
                string idWithoutPrefix = buttonID.Replace(prefix, ""); // 获取ID部分，去掉前缀
                int aPart = int.Parse(idWithoutPrefix[idWithoutPrefix.Length - 3].ToString()); // 获取A部分
                if (aPart > TotalAntionPageIndex) // 如果数字大于当前最大索引
                    TotalAntionPageIndex = aPart + 1; // 更新全局页面索引
            }
            return TotalAntionPageIndex;
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
            command.Parameters.AddWithValue("@TableName", tableName); // 绑定参数
            using var reader = command.ExecuteReader(); // 执行查询语句
            return reader.Read(); // 返回是否存在
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
        public string Data1 { get; set; } // 动作数据1
        public string Data2 { get; set; } // 动作数据2
        public string Data3 { get; set; } // 动作数据3
        public string Description { get; set; } // 对动作的描述
        public DateTime CreateTime { get; set; } // 创建时间
        public DateTime LatestEditTime { get; set; } // 最近修改时间
        public string ActionType { get; set; } // 动作类型
    }
}