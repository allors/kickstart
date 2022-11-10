namespace Tests.E2E
{
    using System.Globalization;
    using System.IO;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Allors.Database.Domain.Tests;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Person = Allors.Database.Domain.Person;

    public static class Config
    {
        public const string EnvironmentName = "Development";
        public const string ConfigPath = "/config/aviation";

        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        static Config()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCrossPlatform(".");
            configurationBuilder.AddCrossPlatform(".", EnvironmentName, true);
            configurationBuilder.AddCrossPlatform(ConfigPath, EnvironmentName);

            Configuration = configurationBuilder.Build();

            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");
            MetaPopulation = MetaBuilder.Build();
            var rules = Rules.Create(MetaPopulation);
            Engine = new Engine(rules);

            var domainPrint = typeof(Person).Assembly.Fingerprint();
            var testPrint = typeof(Test).Assembly.Fingerprint();
            var testPopulationPrint = typeof(Marker).Assembly.Fingerprint();
            PopulationFileInfo = new FileInfo($"population.{domainPrint}.{testPrint}.{testPopulationPrint}.xml");
        }

        public static IConfigurationRoot Configuration { get; }

        public static MetaPopulation MetaPopulation { get; }

        public static Engine Engine { get; }

        public static FileInfo PopulationFileInfo { get; }
    }
}
