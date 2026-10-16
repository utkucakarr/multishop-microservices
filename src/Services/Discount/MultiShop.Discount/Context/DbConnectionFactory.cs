using System.Data;
using Microsoft.Data.SqlClient;

namespace MultiShop.Discount.Context
{
    /// <summary>Dapper sorguları için açık SQL Server bağlantısı üretir; bağlantıyı çağıran kapatır (<c>using</c>).</summary>
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    public class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}
