using StackExchange.Redis;

namespace MultiShop.Basket.Settings
{
    public class RedisService
    {
        public string _host { get; set; }

        public int _port { get; set; }

        private readonly string _password;

        private ConnectionMultiplexer _connectionMultiplexer;
        public RedisService(string host, int port, string password)
        {
            _host = host;
            _port = port;
            _password = password;
        }

        public void Connect()
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { { _host, _port } },
                Password = _password
            };
            _connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
        }

        public IDatabase GetDb(int db = 1) => _connectionMultiplexer.GetDatabase(0);
    }
}
