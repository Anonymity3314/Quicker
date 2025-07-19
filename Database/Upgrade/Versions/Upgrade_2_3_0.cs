using Quicker.Database.Core;
using System.Data.SQLite;

namespace Quicker.Database.Upgrade.Versions
{
    internal class Upgrade_2_3_0 : IDatabaseUpgradeStep
    {
        public string FromVersion => "2.2.0"; // 升级前的版本号
        public string ToVersion => "2.3.0"; // 升级后的版本号

        /// <summary>
        /// 升级数据库
        /// </summary>
        /// <param name="connection"> 数据库连接 </param>
        /// <param name="manager"> 数据库更新管理器 </param>
        public void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager)
        {
            SettingDatabase.InitializeAppearance(); // 新增数据库表
        }
    }
}