using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAPI.Security
{
    public class ConfigContex
    {
        // This class handle interaction with the config (appsettings.json)

        public readonly IConfiguration configuration;

        public ConfigContex()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            configuration = configBuilder.Build();
        }
    }
}
