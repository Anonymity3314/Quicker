using Quicker.Database.Upgrade;
using Quicker.Database.Core;

namespace Quicker.Database.Upgrade
{
    internal class DatabaseUpdateManager :IDisposable
    {
        private readonly string _newBasePath = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\LocalIcons\"; // 新图片文件夹路径
        private const string DatabaseFolder = @"C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\Database"; // 数据库文件夹路径
        private readonly ButtonDatabase _db2 = new(); // 按钮数据库对象
        private bool _disposed = false; // 标记是否已释放资源

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

        // 析构函数
        ~DatabaseUpdateManager()
        {
            Dispose(false); // 释放非托管资源
        }

        // 2.2.0 版本之前的按钮数据
        public class ButtonDataBefore2_2_0
        {
            public string ButtonID { get; set; }
            public string Title { get; set; }
            public string Location { get; set; }
            public string ImagePath { get; set; }
            public string Data1 { get; set; }
            public string Data2 { get; set; }
            public string Data3 { get; set; }
            public string Description { get; set; }
            public DateTime CreateTime { get; set; }
            public DateTime LatestEditTime { get; set; }
            public string ActionType { get; set; }
            public int UsedTimes { get; set; }
        }
    }
}