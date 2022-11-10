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
    public class NonUnifiedPartSecurityTokenRule : Rule
    {
        public NonUnifiedPartSecurityTokenRule(MetaPopulation m) : base(m, new Guid("a186dece-957b-4fa7-a174-556a6b122c15")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedPart.RolePattern(v => v.DefaultFacility),
                m.Facility.RolePattern(v => v.Owner, v => v.PartsWhereDefaultFacility.Part, m.NonUnifiedPart),
                m.Facility.RolePattern(v => v.WorkshopWarehouse, v => v.PartsWhereDefaultFacility.Part, m.NonUnifiedPart),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                @this.DeriveNonUnifiedPartSecurityToken(validation);
            }
        }
    }

    public static class NonUnifiedPartSecurityTokenRuleExtensions
    {
        public static void DeriveNonUnifiedPartSecurityToken(this NonUnifiedPart @this, IValidation validation)
        {
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();
            var transaction = @this.Strategy.Transaction;

            var internalOrganisations = transaction.Extent<Organisation>().Where(v => v.IsInternalOrganisation).ToArray();

            foreach (InternalOrganisation internalOrganisation in internalOrganisations)
            {
                internalOrganisation.RemoveSparePart(@this);
            }

            @this.DefaultFacility?.Owner?.AddSparePart(@this);

            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
            };

            foreach (var internalOrganisation in internalOrganisations)
            {
                @this.AddSecurityToken(internalOrganisation.LocalEmployeeSecurityToken);
                @this.AddSecurityToken(internalOrganisation.LocalAdministratorSecurityToken);
            }
        }
    }
}
