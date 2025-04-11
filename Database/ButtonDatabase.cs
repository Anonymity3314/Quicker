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
        public void InitializeDatabase()
        {
            if (!File.Exists("Button.db"))
            {
                SQLiteConnection.CreateFile("Button.db");
            }

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

        // 通过ButtonID获取Button信息
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
                });
            }
            return contents;
        }

        // 添加动作
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

        // 更新动作
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

        // 删除动作
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

        // 生成ButtonID
        public string GenerateButtonID(string filePath, int canvasIndex, int row, int col)
        {
            // 计算文件路径的哈希值
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(filePath));
            string hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            // 生成ButtonID: 哈希值_页面索引_行_列
            return $"{hashString}_{canvasIndex:D3}_{row:D1}_{col:D1}";
        }

        // 根据不同情况更改 Button 数据库
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
                using var cmd = new SQLiteCommand("UPDATE ButtonData SET ButtonID = @NewButtonID WHERE ButtonID = @OldButtonID", connection); // 将 ButtonID1 的编号改为 ButtonID2
                cmd.Parameters.AddWithValue("@NewButtonID", buttonID2); // ButtonID2
                cmd.Parameters.AddWithValue("@OldButtonID", buttonID1); // ButtonID1
                cmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }
    }

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