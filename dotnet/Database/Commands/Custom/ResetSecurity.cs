// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Custom.cs" company="Allors bvba">
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
using Allors.Database.Services;
using NLog;

namespace Commands
{
    using Allors;

    using McMaster.Extensions.CommandLineUtils;

    using NLog;

    [Command(Description = "Reset security")]
    public class ResetSecurity
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            var database = this.Parent.Database;

            using (var transaction = database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                database.Services.Get<IPermissions>().Sync(transaction);
                
                new Security(transaction).Apply();

                transaction.Commit();

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }
    }
}