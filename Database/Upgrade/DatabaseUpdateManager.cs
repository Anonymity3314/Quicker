using Quicker.Database.Upgrade.Versions;
using Quicker.Windows.ToolWindows;
using Quicker.Database.Upgrade;
using Quicker.Database.Core;
using System.Data.SQLite;
using System.Reflection;
using Quicker.Managers;
using Quicker.Helpers;
using System.IO;

namespace Quicker.Database.Upgrade
{
    public class DatabaseUpdateManager :IDisposable
    {
        private List<IDatabaseUpgradeStep> _upgradeSteps; // 升级步骤列表
        public readonly ButtonDatabase _db2 = new(); // 按钮数据库对象
        private bool _disposed = false; // 标记是否已释放资源

        public DatabaseUpdateManager()
        {
            _upgradeSteps = new List<IDatabaseUpgradeStep>
            {
                new Upgrade_2_3_0()
            }; // 升级步骤列表
        }

        // 检查并升级数据库
        public void CheckAndUpgradeDatabase()
        {
            string currentVersion = GetCurrentVersion(); // 获取当前数据库版本号
            if (VersionHelper.CompareVersions(currentVersion, SettingDatabase.currentVersion) == 0) return; // 使用VersionHelper进行版本比较
            LoadingWindow loadingWindow = new(); // 创建加载窗口
            loadingWindow.Show(); // 显示加载窗口
            try
            {
                while (true)
                {
                    var step = _upgradeSteps.FirstOrDefault(s => s.FromVersion == currentVersion);
                    if (step == null) break;
                    using (var conn = SettingDatabase.OpenConnection())
                    {
                        step.Upgrade(conn, this);
                    }
                    SetCurrentVersion(step.ToVersion);
                    currentVersion = step.ToVersion;
                }
            }
            catch
            {
                using var toast = new ToastManager(); // 创建 Toast 管理器
                toast.Show("数据库更新失败，请删除数据库文件后重试。", ToastType.Error); // 显示 Toast 通知
            }
            finally
            {
                loadingWindow.Close(); // 关闭加载窗口
            }
        }

        /// <summary>
        /// 获取当前数据库版本号
        /// </summary>
        /// <returns> 当前版本号 </returns>
        private string GetCurrentVersion()
        {
            using var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
            string selectVersionQuery = "SELECT Version FROM Convention ORDER BY ID DESC LIMIT 1;"; // 查询版本号
            using var command = new SQLiteCommand(selectVersionQuery, connection); // 创建 SQLiteCommand 对象
            using var reader = command.ExecuteReader(); // 执行查询命令
            return reader.Read() ? reader.GetString(0) : null; // 如果有数据，返回版本号；如果没有数据，则返回null
        }

        /// <summary>
        /// 设置数据库版本号
        /// </summary>
        /// <param name="version"> 版本号 </param>
        public void SetCurrentVersion(string version)
        {
            using var connection = SettingDatabase.OpenConnection(); // 打开数据库连接
            string updateVersionQuery = @$"UPDATE Convention SET Version = '{version}';"; // 设置默认值
            using var updateVersionCommand = new SQLiteCommand(updateVersionQuery, connection); // 创建 SQLiteCommand 对象
            updateVersionCommand.ExecuteNonQuery(); // 执行更新命令
        }

        // 释放资源
        public void Dispose()
        {
            Dispose(true); // 释放托管资源
            GC.SuppressFinalize(this); // 调用终结器（析构器）以释放非托管资源
        }

        /// <summary>
        /// 释放资源的受保护实现
        /// </summary>
        /// <param name="disposing"> 是否释放托管资源 </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) _disposed = true; // 防止重复释放
        }
    }
}