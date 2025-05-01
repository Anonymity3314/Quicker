using System.Data.SQLite;
using System.IO;

namespace Quicker.Database
{
    internal class ActionPageDatabase
    {
        private readonly string dbPath3 = "Data Source=ActionPage.db;Pooling=true;Max Pool Size=100;Journal Mode=Wal;";

        // 初始化数据库
        public void Initialize()
        {
            if (File.Exists("ActionPage.db")) // 如果数据库存在
            {
                return; // 数据库已存在，不再初始化
            }

            SQLiteConnection.CreateFile("ActionPage.db"); // 创建数据库文件
        }
    }
}

public class ActionPageDatabase
{
    public string ActionPageId { get; set; }
    public string ActionPageName { get; set; }
    public string ActionPageIconPath { get; set; }
}