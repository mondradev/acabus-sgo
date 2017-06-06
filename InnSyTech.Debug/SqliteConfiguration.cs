using InnSyTech.Standard.Database;

namespace InnSyTech.Debug
{
    internal class SQLiteConfiguration : IDbConfiguration
    {
        public string ConnectionString => "Data Source=acabus_data.dat;Version=3;Password=acabus*data*dat";

        public string LastInsertFunctionName => "last_insert_rowid";

        public int TransactionPerConnection => 1;
    }
}