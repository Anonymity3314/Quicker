using System.Data.SQLite;

namespace Quicker.Database.Upgrade
{
    public interface IDatabaseUpgradeStep
    {
        string FromVersion { get; }
        string ToVersion { get; }
        void Upgrade(SQLiteConnection connection, DatabaseUpdateManager manager);
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