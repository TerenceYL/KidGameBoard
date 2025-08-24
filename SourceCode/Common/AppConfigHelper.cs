using Microsoft.Extensions.Configuration;
using System.IO;

namespace KidGameBoard.Common
{
    public static class AppConfigHelper
    {
        private static IConfigurationRoot? _config;
        static AppConfigHelper()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static string GetConnectionString(string name)
            => _config?.GetConnectionString(name) ?? "";
    }
}