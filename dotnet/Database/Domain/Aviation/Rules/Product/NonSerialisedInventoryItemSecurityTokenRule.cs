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
    public class NonSerialisedInventoryItemSecurityTokenRule : Rule
    {
        public NonSerialisedInventoryItemSecurityTokenRule(MetaPopulation m) : base(m, new Guid("d2416519-7da3-4a8f-aa67-f01f19b12bf5")) =>
            this.Patterns = new Pattern[]
            {
                m.NonSerialisedInventoryItem.RolePattern(v => v.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var m = cycle.Transaction.Database.Services.Get<MetaPopulation>();

            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                @this.DeriveNonSerialisedInventoryItemSecurityToken(validation);
            }
        }
    }

    public static class NonSerialisedInventoryItemSecurityTokenRuleExtensions
    {
        public static void DeriveNonSerialisedInventoryItemSecurityToken(this NonSerialisedInventoryItem @this, IValidation validation)
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
