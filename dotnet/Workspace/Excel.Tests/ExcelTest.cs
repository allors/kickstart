//------------------------------------------------------------------------------------------------- 
// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright 2002-2009 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// <summary>Defines the DomainTest type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Allors
{
    using Microsoft.Extensions.Configuration;
    using NLog;
    using Moq;
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Application;

    public class ExcelTest : IDisposable
    {
        public ExcelTest(bool populate = true)
        {
            this.SetupDatabase(populate);
            this.SetupWorkspace();

            this.Program = new Program(this.WorkspaceServiceProvider);
        }

        public ServiceProvider DatabaseServiceProvider { get; set; }

        public ServiceProvider WorkspaceServiceProvider { get; set; }

        public Program Program { get; }

    
        public ISession DatabaseSession { get; private set; }

        public ITimeService TimeService => this.DatabaseSession.ServiceProvider.GetRequiredService<ITimeService>();

        public TimeSpan? TimeShift
        {
            get => this.TimeService.Shift;

            set => this.TimeService.Shift = value;
        }

        protected Person Administrator => this.GetDatabaseUser("administrator");

        public void Dispose()
        {
            this.DatabaseSession.Rollback();
            this.DatabaseSession = null;
        }

        protected void SetDatabaseUser(string identity)
        {
            var users = new Users(this.DatabaseSession);
            var user = users.GetUser(identity) ?? new AutomatedAgents(this.DatabaseSession).Guest;
            this.DatabaseSession.SetUser(user);
        }

        private Person GetDatabaseUser(string userName) => (Person)new Users(this.DatabaseSession).GetUser(userName);

        private void SetupDatabase(bool populate)
        {
            var services = new ServiceCollection();
            services.AddAllors();
            services.AddSingleton<Bogus.Faker>();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            this.DatabaseServiceProvider = services.BuildServiceProvider();

            var configuration = new Database.Adapters.Memory.Configuration
            {
                ObjectFactory = new ObjectFactory(Meta.MetaPopulation.Instance, typeof(User)),
            };

            var database = new Database.Adapters.Memory.Database(this.DatabaseServiceProvider, configuration);
            database.Init();

            this.DatabaseSession = database.CreateSession();

            if (populate)
            {
                Fixture.Setup(database);

                this.DatabaseSession.Commit();
            }

            var databaseService = this.DatabaseServiceProvider.GetRequiredService<IDatabaseService>();
            databaseService.Database = database;
        }

        private void SetupWorkspace()
        {
            var configuration = new ConfigurationBuilder().Build();
            var messageService = new Mock<IMessageService>().Object;
            var errorService = new Mock<IErrorService>().Object;

            this.WorkspaceServiceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddSingleton<IMessageService>(messageService)
                .AddSingleton<IErrorService>(errorService)
                .BuildServiceProvider();

            var database = new LocalDatabase(this.DatabaseServiceProvider.GetRequiredService<IDatabaseService>(),
                this.DatabaseServiceProvider.GetRequiredService<ITreeService>(),
                this.DatabaseServiceProvider.GetRequiredService<IFetchService>(),
                this.DatabaseServiceProvider.GetRequiredService<IExtentService>(),
                this.DatabaseServiceProvider.GetRequiredService<ILogger<LocalDatabase>>());

            var objectFactory = new Workspace.ObjectFactory(Workspace.Meta.MetaPopulation.Instance, typeof(Workspace.Domain.User));
            var workspace = new Workspace.Workspace(objectFactory);
            this.Client = new Client(database, workspace);
        }
    }
}