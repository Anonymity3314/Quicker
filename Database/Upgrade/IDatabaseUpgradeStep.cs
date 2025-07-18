using System.Data.SQLite;

namespace Quicker.Database.Upgrade
{
    public interface IDatabaseUpgradeStep
    {
        string FromVersion { get; }
        string ToVersion { get; }
        void Upgrade(SQLiteConnection connection);
    }
}