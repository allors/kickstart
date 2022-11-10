using System.IO;
using Allors.Database.Adapters.Memory;
using Allors.Database.Configuration;

namespace Tests.E2E
{
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Domain;
    using NUnit.Framework;
    using Database = Allors.Database.Adapters.Memory.Database;
    using Person = Allors.Database.Domain.Person;

    [SetUpFixture]
    public class SetUpFixture
    {

        [OneTimeSetUp]
        public void Init()
        {
            Config.PopulationFileInfo.Refresh();

            if (!Config.PopulationFileInfo.Exists)
            {
                var database = new Database(
                    new TestDatabaseServices(Config.Engine, null),
                    new Configuration
                    {
                        ObjectFactory = new ObjectFactory(Config.MetaPopulation, typeof(Person)),
                    });

                database.Init();

                var config = new Allors.Database.Domain.Config();
                new Setup(database, config).Apply();

                using var transaction = database.CreateTransaction();
                new IntranetPopulation(transaction, null, Config.MetaPopulation).Execute();
                transaction.Commit();

                using (var stream = File.Create(Config.PopulationFileInfo.FullName))
                {
                    using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { CheckCharacters = false }))
                    {
                        database.Save(writer);
                    }
                }
            }
        }
    }
}
