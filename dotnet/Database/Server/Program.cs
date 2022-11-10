﻿namespace Allors.Database.Server.Controllers
{
    using System;
    using System.IO;
    using Allors.Server;
    using Services;
    using Configuration;
    using Database;
    using Domain;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using NLog.Web;

    public class Program
    {
        private const string ConfigPath = "/config/aviation";

        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(builder => builder
                    .UseStartup<Startup>()
                    .ConfigureAppConfiguration((hostingContext, configurationBuilder) =>
                    {
                        var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
                        configurationBuilder.AddCrossPlatform(".", environmentName, true);
                        configurationBuilder.AddCrossPlatform(ConfigPath, environmentName);
                        configurationBuilder.AddCrossPlatform(Path.Combine(ConfigPath, hostingContext.HostingEnvironment.ApplicationName), environmentName);
                    })
                    .UseNLog());
    }
}