using System.Data.SQLite;

namespace Quicker.Database.Upgrade.Versions
{
    internal class Upgrade_2_1_2 : IDatabaseUpgradeStep
    {
        public string FromVersion => "2.1.1";

        public string ToVersion => "2.1.2";

        /// <summary>
        /// 更新数据库
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库管理器 </param>
        public void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            // 2.1.2版本无需特殊升级操作
        }
    }
}