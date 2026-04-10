using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace KeywordScoring.Data
{
    public abstract class DbFactoryBase
    {
        private readonly IConfiguration _config;

        private readonly string _connectionStringKey;

        internal string DbConnectionString => _config.GetConnectionString(_connectionStringKey);

        public DbFactoryBase(IConfiguration config, string connectionStringKey)
        {
            _config = config;
            _connectionStringKey = connectionStringKey;
        }

        internal IDbConnection DbConnection => new SqlConnection(DbConnectionString);

    }
}
