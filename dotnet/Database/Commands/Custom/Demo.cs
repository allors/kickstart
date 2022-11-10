// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Demo.cs" company="Allors bvba">
//   Copyright 2002-2017 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Allors.Database.Domain;
using NLog;

namespace Commands
{
    using System.IO;
    using Allors;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.Extensions.Configuration;
    using NLog;

    [Command(Description = "Setup Demo Population")]
    public class Demo
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            this.Parent.Database.Init();

            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                var config = new Config { DataPath = this.Parent.DataPath };
                new Setup(this.Parent.Database, config).Apply();

                transaction.Derive();
                transaction.Commit();

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

                new Allors.Database.Domain.Upgrade(transaction, this.Parent.DataPath).Execute();

                transaction.Derive();
                transaction.Commit();

                new TestPopulation(transaction).ForDemo();

                transaction.Derive();
                transaction.Commit();
            }

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}