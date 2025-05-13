using System.Data.SQLite;
using Quicker.Database;
using System.IO;

namespace Quicker.Managers
{
    internal class UpdateManager
    {
        private readonly string dbPath2 = "Data Source=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Button.db") + ";Pooling=true;Max Pool Size=100;Journal Mode=Wal;";
        private readonly ButtonDatabase db2 = new ButtonDatabase(); // 按钮数据库

        /// <summary>
        /// 将旧表中的所有按钮迁移到对应的新表并删除旧表
        /// </summary>
        public void MigrateOldData()
        {
            try
            {
                // 检测数据库文件是否存在
                if (!File.Exists("Button.db"))
                {
                    return;
                }

                // 创建一个新的数据库连接
                var connection = new SQLiteConnection(dbPath2);
                connection.Open();

                try
                {
                    // 检测数据库中是否存在 ButtonData 表
                    using var checkCommand = new SQLiteCommand(
                        "SELECT name FROM sqlite_master WHERE type='table' AND name='ButtonData'",
                        connection);
                    using var checkReader = checkCommand.ExecuteReader();
                    if (!checkReader.Read())
                    {
                        // 如果不存在 ButtonData 表，直接返回，避免出错
                        Console.WriteLine("ButtonData 表不存在，无需迁移。");
                        return;
                    }

                    // 将旧表重命名为临时表
                    using var renameCommand = new SQLiteCommand("ALTER TABLE ButtonData RENAME TO Temp_ButtonData", connection);
                    renameCommand.ExecuteNonQuery();

                    // 获取旧表ButtonData中的所有按钮数据
                    var oldButtonData = new List<ButtonData>();
                    using var oldCommand = new SQLiteCommand("SELECT * FROM Temp_ButtonData", connection);
                    using var oldReader = oldCommand.ExecuteReader();
                    while (oldReader.Read())
                    {
                        oldButtonData.Add(new ButtonData
                        {
                            ButtonID = oldReader.GetString(0),
                            Title = oldReader.GetString(1),
                            Location = oldReader.GetString(2),
                            ImagePath = oldReader.GetString(3),
                            RunByMessager = oldReader.GetBoolean(4),
                            TryToOpenExitingWindow = oldReader.GetBoolean(5),
                            WindowState = oldReader.GetInt32(6),
                            Description = oldReader.GetString(7),
                            CreateTime = oldReader.GetDateTime(8),
                            LatestEditTime = oldReader.GetDateTime(9),
                            ActionType = "OpenFile"
                        });
                    }

                    using var transaction = connection.BeginTransaction(); // 开始事务

                    try
                    {
                        // 将每个按钮数据迁移到对应的新表中
                        foreach (var buttonData in oldButtonData)
                        {
                            string tableName = db2.GetTableNameFromButtonID(buttonData.ButtonID); // 从ButtonID解析表名
                            db2.CheckAndCreateTable(tableName, connection); // 检查表是否存在，不存在则创建

                            string insertQuery = $@"INSERT INTO {tableName} 
                        (ButtonID, ButtonName, Location, ImagePath, RunByMessager, TryToOpenExitingWindow, WindowState, Usage, CreateTime, LatestEditTime, Type) 
                        VALUES 
                        (@ButtonID, @ButtonName, @Location, @ImagePath, @RunByMessager, @TryToOpenExitingWindow, @WindowState, @Usage, @CreateTime, @LatestEditTime, @Type)";

                            using var insertCommand = new SQLiteCommand(insertQuery, connection);
                            insertCommand.Parameters.AddWithValue("@ButtonID", buttonData.ButtonID);
                            insertCommand.Parameters.AddWithValue("@ButtonName", buttonData.Title);
                            insertCommand.Parameters.AddWithValue("@Location", buttonData.Location);
                            insertCommand.Parameters.AddWithValue("@ImagePath", buttonData.ImagePath);
                            insertCommand.Parameters.AddWithValue("@RunByMessager", buttonData.RunByMessager);
                            insertCommand.Parameters.AddWithValue("@TryToOpenExitingWindow", buttonData.TryToOpenExitingWindow);
                            insertCommand.Parameters.AddWithValue("@WindowState", buttonData.WindowState);
                            insertCommand.Parameters.AddWithValue("@Usage", buttonData.Description);
                            insertCommand.Parameters.AddWithValue("@CreateTime", buttonData.CreateTime);
                            insertCommand.Parameters.AddWithValue("@LatestEditTime", buttonData.LatestEditTime);
                            insertCommand.Parameters.AddWithValue("@Type", buttonData.ActionType);

                            insertCommand.ExecuteNonQuery(); // 执行插入语句
                        }

                        transaction.Commit(); // 提交事务
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // 回滚事务
                        throw new Exception("迁移数据失败: " + ex.Message);
                    }

                    // 删除临时表
                    using var dropCommand = new SQLiteCommand("DROP TABLE Temp_ButtonData", connection);
                    dropCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("迁移失败: " + ex.Message);
                }
                finally
                {
                    // 确保连接被关闭和释放
                    if (connection.State == System.Data.ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("迁移失败: " + ex.Message);
            }
        }
    }
}