// <copyright file="Upgrade.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Services;

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;

    [Command(Description = "Add file contents to the index")]
    public class Upgrade
    {
        private readonly HashSet<Guid> excludedObjectTypes = new HashSet<Guid>
        {
            //new Guid("7fd1760c-ee1f-4d04-8a93-dfebc82757c1"), // Amortization
        }; 

        private readonly HashSet<Guid> excludedRelationTypes = new HashSet<Guid>
        {
            new Guid("258a33cc-06ea-45a0-9b15-1b6d58385910"), // TimeEntry.SalesTerm
        };

        private readonly HashSet<Guid> movedRelationTypes = new HashSet<Guid>
        {
        };

        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        [Option("-f", Description = "File to load")]
        public string FileName { get; set; }

        public int OnExecute(CommandLineApplication app)
        {
            var fileName = this.FileName ?? this.Parent.Configuration["populationFile"];
            var fileInfo = new FileInfo(fileName);

            this.Logger.Info("Begin");

            var notLoadedObjectTypeIds = new HashSet<Guid>();
            var notLoadedRelationTypeIds = new HashSet<Guid>();

            var notLoadedObjects = new HashSet<long>();

            var deletedDelegatedAccess = new HashSet<long>();

            using (var reader = XmlReader.Create(fileInfo.FullName))
            {
                this.Parent.Database.ObjectNotLoaded += (sender, args) =>
                {
                    if (!this.excludedObjectTypes.Contains(args.ObjectTypeId))
                    {
                        notLoadedObjectTypeIds.Add(args.ObjectTypeId);
                    }
                    else
                    {
                        var id = args.ObjectId;
                        notLoadedObjects.Add(id);
                    }
                };

                this.Parent.Database.RelationNotLoaded += (sender, args) =>
                {
                    if (args.RelationTypeId == new Guid("4277EB04-A800-4EA9-B19F-A2268D903D5F"))
                    {
                        deletedDelegatedAccess.Add(args.AssociationId);
                        return;
                    }

                    if (!this.excludedRelationTypes.Contains(args.RelationTypeId))
                    {
                        if (!notLoadedObjects.Contains(args.AssociationId))
                        {
                            notLoadedRelationTypeIds.Add(args.RelationTypeId);
                        }
                    }
                };

                this.Logger.Info("Loading {file}", fileInfo.FullName);
                this.Parent.Database.Load(reader);
            }

            if (notLoadedObjectTypeIds.Count > 0)
            {
                var notLoaded = notLoadedObjectTypeIds
                    .Aggregate("Could not load following ObjectTypeIds: ", (current, objectTypeId) => current + "- " + objectTypeId);

                this.Logger.Error(notLoaded);
                return 1;
            }

            if (notLoadedRelationTypeIds.Count > 0)
            {
                var notLoaded = notLoadedRelationTypeIds
                    .Aggregate("Could not load following RelationTypeIds: ", (current, relationTypeId) => current + "- " + relationTypeId);

                this.Logger.Error(notLoaded);
                return 1;
            }

            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                var removed = transaction.Instantiate(deletedDelegatedAccess);
                if (removed.Any(v => v.Strategy.Class.Id != new Guid("3b43da7f-5252-4824-85fe-c85d6864838a")))
                {
                    this.Logger.Error("Error removing delegated access");
                    return 1;
                }
            }

            // Upgrade
            //using (var transaction = this.Parent.Database.CreateTransaction())
            //{
            //    foreach (var kvp in upgradePartyContactMechanism)
            //    {
            //        var partyContactMechanism = (PartyContactMechanism)transaction.Instantiate(kvp.Key);
            //        var party = (Party)transaction.Instantiate(kvp.Value);
            //        partyContactMechanism.Party = party;
            //    }

            //    transaction.Commit();
            //}

            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                new Allors.Database.Domain.Upgrade(transaction, this.Parent.DataPath).Execute();
                transaction.Commit();

                this.Parent.Database.Services.Get<IPermissions>().Sync(transaction);

                new Security(transaction).Apply();

                transaction.Commit();
            }

            this.Logger.Info("End");
            return ExitCode.Success;
        }
    }
}
