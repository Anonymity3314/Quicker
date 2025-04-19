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
        private readonly string dbPath2 = "Data Source=Button.db;Pooling=true;Max Pool Size=100;";

        // 初始化数据库
        public void Initialize()
        {
            if (File.Exists("Button.db")) return;
            SQLiteConnection.CreateFile("Button.db");

            using var connection = new SQLiteConnection(dbPath2);
            connection.Open();

            string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS [ButtonData]
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
                LatestEditTime DATETIME
            );";
            using var command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 通过ButtonID获取动作信息
        /// </summary>
        /// <param name="buttonID">要获取的动作信息</param>
        /// <returns>动作信息</returns>
        public ButtonData GetButtonDataByID(string buttonID)
        {
            using var connection = new SQLiteConnection(dbPath2);
            connection.Open();
            using var command = new SQLiteCommand("SELECT * FROM ButtonData WHERE ButtonID = @ButtonID", connection);
            command.Parameters.AddWithValue("@ButtonID", buttonID);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new ButtonData
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
                };
            }
            return null;
        }

        // 获取全部Button信息
        public List<ButtonData> GetAllButtonData()
        {
            var contents = new List<ButtonData>();
            using var connection = new SQLiteConnection(dbPath2);
            connection.Open();
            using var command = new SQLiteCommand("SELECT * FROM ButtonData", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                contents.Add(new ButtonData
                {
                    ButtonID = reader.IsDBNull(0) ? null : reader.GetString(0),
                    ButtonName = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Location = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ImagePath = reader.IsDBNull(3) ? null : reader.GetString(3),
                    RunByMessager = reader.IsDBNull(4) ? false : reader.GetBoolean(4),
                    TryToOpenExitingWindow = reader.IsDBNull(5) ? false : reader.GetBoolean(5),
                    WindowState = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    Usage = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CreateTime = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8),
                    LatestEditTime = reader.IsDBNull(9) ? DateTime.MinValue : reader.GetDateTime(9),
                });
            }
            return contents;
        }

        /// <summary>
        /// 添加动作
        /// </summary>
        /// <param name="buttonData">要添加的动作ID</param>
        public void AddAction(ButtonData buttonData)
        {
            using var connection = new SQLiteConnection(dbPath2);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = new SQLiteCommand(
            "INSERT INTO ButtonData " +
                "(ButtonID, ButtonName, Location, ImagePath, RunByMessager, TryToOpenExitingWindow, WindowState, Usage, CreateTime, LatestEditTime) " +
                "VALUES " +
                "(@ButtonID, @ButtonName, @Location, @ImagePath, @RunByMessager, @TryToOpenExitingWindow, @WindowState, @Usage, @CreateTime, @LatestEditTime)",
            connection);
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID);
            command.Parameters.AddWithValue("@ButtonName", buttonData.ButtonName);
            command.Parameters.AddWithValue("@Location", buttonData.Location);
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath);
            command.Parameters.AddWithValue("@RunByMessager", buttonData.RunByMessager);
            command.Parameters.AddWithValue("@TryToOpenExitingWindow", buttonData.TryToOpenExitingWindow);
            command.Parameters.AddWithValue("@WindowState", buttonData.WindowState);
            command.Parameters.AddWithValue("@Usage", buttonData.Usage);
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime);
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime);
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        /// <summary>
        /// 更新动作
        /// </summary>
        /// <param name="buttonData">要更新的动作ID</param>
        public void UpdateAction(ButtonData buttonData)
        {
            using var connection = new SQLiteConnection(dbPath2);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = new SQLiteCommand(
            "UPDATE ButtonData SET " +
                "ButtonName = @ButtonName, " +
                "Location = @Location, " +
                "ImagePath = @ImagePath, " +
                "RunByMessager = @RunByMessager, " +
                "TryToOpenExitingWindow = @TryToOpenExitingWindow, " +
                "WindowState = @WindowState, " +
                "Usage = @Usage, " +
                "CreateTime = @CreateTime, " +
                "LatestEditTime = @LatestEditTime " +
            "WHERE ButtonID = @ButtonID", connection);
            command.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID);
            command.Parameters.AddWithValue("@ButtonName", buttonData.ButtonName);
            command.Parameters.AddWithValue("@Location", buttonData.Location);
            command.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath);
            command.Parameters.AddWithValue("@RunByMessager", buttonData.RunByMessager);
            command.Parameters.AddWithValue("@TryToOpenExitingWindow", buttonData.TryToOpenExitingWindow);
            command.Parameters.AddWithValue("@WindowState", buttonData.WindowState);
            command.Parameters.AddWithValue("@Usage", buttonData.Usage);
            command.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime);
            command.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime);
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        /// <summary>
        /// 删除动作
        /// </summary>
        /// <param name="buttonID">要删除的动作ID</param>
        public void DeleteAction(string buttonID)
        {
            using var connection = new SQLiteConnection(dbPath2);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = new SQLiteCommand("DELETE FROM ButtonData WHERE ButtonID = @ButtonID", connection);
            command.Parameters.AddWithValue("@ButtonID", buttonID);
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        /// <summary>
        /// 根据不同情况更改 Button 数据库
        /// </summary>
        /// <param name="buttonID1"></param>
        /// <param name="buttonID2"></param>
        public void ExchangeButtonID(string buttonID1, string buttonID2)
        {
            using var connection = new SQLiteConnection(dbPath2); // 连接数据库
            connection.Open(); // 打开连接

            var data1 = GetButtonDataByID(buttonID1); // 获取 ButtonID1 的数据
            var data2 = GetButtonDataByID(buttonID2); // 获取 ButtonID2 的数据
            using var transaction = connection.BeginTransaction(); // 开始事务
            if (data2 != null) // 直接交换 ButtonID
            {
                string tempButtonID = "temp_";

                using var cmd1 = new SQLiteCommand("UPDATE ButtonData SET ButtonID = @TempButtonID WHERE ButtonID = @ButtonID1", connection); // 将 ButtonID1 的编号改为临时编号
                cmd1.Parameters.AddWithValue("@TempButtonID", tempButtonID); // 临时编号
                cmd1.Parameters.AddWithValue("@ButtonID1", buttonID1); // ButtonID1
                cmd1.ExecuteNonQuery();

                using var cmd2 = new SQLiteCommand("UPDATE ButtonData SET ButtonID = @ButtonID1 WHERE ButtonID = @ButtonID2", connection); // 将 ButtonID2 的编号改为 ButtonID1
                cmd2.Parameters.AddWithValue("@ButtonID1", buttonID1); // ButtonID1
                cmd2.Parameters.AddWithValue("@ButtonID2", buttonID2); // ButtonID2
                cmd2.ExecuteNonQuery();

                using var cmd3 = new SQLiteCommand("UPDATE ButtonData SET ButtonID = @ButtonID2 WHERE ButtonID = @TempButtonID", connection); // 将临时编号改为 ButtonID2
                cmd3.Parameters.AddWithValue("@ButtonID2", buttonID2); // ButtonID2
                cmd3.Parameters.AddWithValue("@TempButtonID", tempButtonID); // 临时编号
                cmd3.ExecuteNonQuery();
            }
            else // 将 ButtonID1 的编号改为 ButtonID2
            {
                UpdateButtonID(connection, buttonID1, buttonID2);
            }
            transaction.Commit();
        }

        /// <summary>
        /// 更新 ButtonID
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="oldButtonID">要更改的 ButtonID</param>
        /// <param name="newButtonID">目标 ButtonID</param>
        private void UpdateButtonID(SQLiteConnection connection, string oldButtonID, string newButtonID)
        {
            using var command = new SQLiteCommand("UPDATE ButtonData SET ButtonID = @NewButtonID WHERE ButtonID = @OldButtonID", connection);
            command.Parameters.AddWithValue("@NewButtonID", newButtonID);
            command.Parameters.AddWithValue("@OldButtonID", oldButtonID);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 根据输入的字符串和数字 A1、A2，交换符合条件的 ButtonID 的 A 部分
        /// </summary>
        /// <param name="inputString">Button的字符串索引</param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        public void SwapButtonAValues(string inputString, int a1, int a2)
        {
            List<ButtonData> allButtons = GetAllButtonData(); // 获取所有 Button 数据
            var buttonIDMap = allButtons
                .Select(b => new { ButtonID = b.ButtonID, Data = b })
                .Where(b => Regex.IsMatch(b.ButtonID, @"^(\w+)(\d{3})$") &&
                            Regex.Match(b.ButtonID, @"^(\w+)(\d{3})$").Groups[1].Value == inputString &&
                            (int.Parse(Regex.Match(b.ButtonID, @"^(\w+)(\d{3})$").Groups[2].Value[0].ToString()) == a1 ||
                             int.Parse(Regex.Match(b.ButtonID, @"^(\w+)(\d{3})$").Groups[2].Value[0].ToString()) == a2))
                .ToDictionary(b => b.ButtonID, b => b.Data); // 筛选出符合条件的 ButtonID

            if (buttonIDMap.Count == 0) return; // 没有符合条件的 ButtonID，直接返回

            using (var connection = new SQLiteConnection(dbPath2))
            {
                connection.Open(); // 打开数据库连接
                using (var transaction = connection.BeginTransaction())
                {
                    string tempPrefix = $"temp_{Guid.NewGuid():N}_"; // 生成临时标识符前缀
                    foreach (var pair in buttonIDMap.ToList()) // 更新 A1 部分的 ButtonID 为临时标识符
                    {
                        string buttonID = pair.Key; // 原 ButtonID
                        Match match = Regex.Match(buttonID, @"^(\w+)(\d{3})$"); // 匹配 ButtonID
                        if (match.Success && int.Parse(match.Groups[2].Value[0].ToString()) == a1)
                        {
                            string newButtonID = $"{tempPrefix}{match.Groups[2].Value}"; // 新 ButtonID
                            UpdateButtonID(connection, buttonID, newButtonID); // 更新 ButtonID
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
                            UpdateButtonID(connection, buttonID, newButtonID); // 更新 ButtonID
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
                            UpdateButtonID(connection, buttonID, newButtonID); // 更新 ButtonID
                        }
                    }

                    transaction.Commit(); // 提交事务
                }
            }
        }
    }

    // ButtonData 类
    public class ButtonData
    {
        public string ButtonID { get; set; }
        public string ButtonName { get; set; }
        public string Location { get; set; }
        public string ImagePath { get; set; }
        public bool RunByMessager { get; set; }
        public bool TryToOpenExitingWindow { get; set; }
        public int WindowState { get; set; }
        public string Usage { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime LatestEditTime { get; set; }
    }
}