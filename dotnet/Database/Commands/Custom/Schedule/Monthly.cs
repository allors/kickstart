﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Monthly.cs" company="Allors bvba">
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

    using McMaster.Extensions.CommandLineUtils;

    using NLog;

    [Command(Description = "Add file contents to the index")]
    public class Monthly
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();


        public int OnExecute(CommandLineApplication app)
        {
            this.Logger.Info("Begin");

            using var transaction = this.Parent.Database.CreateTransaction();

            transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

            // Do monthlt stuff
            // new X(transaction).Monthly(transaction);

            var validation = transaction.Derive(false);

            if (validation.HasErrors)
            {
                foreach (var error in validation.Errors)
                {
                    this.Logger.Error("Validation error: {error}", error);
                }

                transaction.Rollback();
            }
            else
            {
                transaction.Commit();
            }

            transaction.Derive();
            transaction.Commit();

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}
