// <copyright file="Custom.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Services;

namespace Commands
{
    using Allors;
    using Allors.Database;
    using Allors.Database.Derivations;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;

    [Command(Description = "Execute custom code")]
    public class Custom
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        private ITransaction transaction;

        public int OnExecute(CommandLineApplication app)
        {
            this.transaction = this.Parent.Database.CreateTransaction();

            this.Logger.Info("Begin");

            transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

            var m = this.Parent.M;

            // Custom code


            transaction.Derive();
            transaction.Commit();

            this.Logger.Info("End");

            return ExitCode.Success;
        }
    }
}