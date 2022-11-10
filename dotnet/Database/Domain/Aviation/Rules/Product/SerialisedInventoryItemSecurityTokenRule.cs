// <copyright file="OrganisationCalculationRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class SerialisedInventoryItemSecurityTokenRule : Rule
    {
        public SerialisedInventoryItemSecurityTokenRule(MetaPopulation m) : base(m, new Guid("68620ea6-2c61-4785-b267-819bee983a5d")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedInventoryItem.RolePattern(v => v.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var m = cycle.Transaction.Database.Services.Get<MetaPopulation>();

            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                @this.DeriveSerialisedInventoryItemSecurityToken(validation);
            }
        }
    }

    public static class SerialisedInventoryItemSecurityTokenRuleExtensions
    {
        public static void DeriveSerialisedInventoryItemSecurityToken(this SerialisedInventoryItem @this, IValidation validation)
        {
            var transaction = @this.Strategy.Transaction;
            var m = transaction.Database.Services.Get<MetaPopulation>();

            var internalOrganisations = transaction.Extent<Organisation>().Where(v => v.IsInternalOrganisation).ToArray();
            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
                @this.Facility?.Owner?.LocalEmployeeSecurityToken,
            };

            foreach (var internalOrganisation in internalOrganisations)
            {
                @this.AddSecurityToken(internalOrganisation.LocalEmployeeSecurityToken);
                @this.AddSecurityToken(internalOrganisation.LocalAdministratorSecurityToken);
            }
        }
    }
}
