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
    public class FacilitySecurityTokenRule : Rule
    {
        public FacilitySecurityTokenRule(MetaPopulation m) : base(m, new Guid("52eac18a-72d9-45b4-8c70-b3cb277c83c7")) =>
            this.Patterns = new Pattern[]
            {
                m.Facility.RolePattern(v => v.Owner),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacilitySecurityToken(validation);
            }
        }
    }

    public static class FacilitySecurityTokenRuleExtensions
    {
        public static void DeriveFacilitySecurityToken(this Facility @this, IValidation validation)
        {
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();
            var transaction = @this.Strategy.Transaction;

            @this.SecurityTokens = new[]
            {
                new SecurityTokens(transaction).DefaultSecurityToken,
            };

            var internalOrganisations = transaction.Extent<Organisation>().Where(v => v.IsInternalOrganisation).ToArray();

            foreach (var internalOrganisation in internalOrganisations)
            {
                @this.AddSecurityToken(internalOrganisation.LocalEmployeeSecurityToken);
                @this.AddSecurityToken(internalOrganisation.LocalAdministratorSecurityToken);
            }
        }
    }
}
