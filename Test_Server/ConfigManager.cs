using Microsoft.Extensions.Configuration;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Server
{
    public static class ConfigManager
    {
        public static IConfiguration AppSetting { get; }
        static ConfigManager()
        {
            AppSetting = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("settings.json")
                    .Build();
        }
    }
}
