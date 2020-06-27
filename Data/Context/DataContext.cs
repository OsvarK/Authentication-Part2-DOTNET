using Microsoft.Extensions.Configuration;
using AuthenticationAPI.Security;

namespace AuthenticationAPI.Data.Context
{
    public class DataContext
    {
        private readonly IConfiguration _configuration;

        public DataContext()
        {
            _configuration = new ConfigContex().configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetValue<string>("ConnectionStrings:Database");
        }
    }
}
