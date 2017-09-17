using InnSyTech.Standard.Database;
using InnSyTech.Standard.Database.Sqlite;

namespace Opera.Acabus.DatabaseController
{
    public class SQLiteDialect : DbDialectBase
    {
        public SQLiteDialect(string connectionString) : base(connectionString)
        {
        }

        public override IDbConverter DateTimeConverter => new SQLiteDateTimeConverter();

        public override string LastInsertFunctionName => "last_insert_rowid";

        public override int TransactionPerConnection => 1;
    }
}