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
    public class SubContractorRelationshipSecurityTokenRule : Rule
    {
        public SubContractorRelationshipSecurityTokenRule(MetaPopulation m) : base(m, new Guid("a51460d4-4f8e-4f5d-835c-7839bb75017f")) =>
            this.Patterns = new Pattern[]
            {
                m.SubContractorRelationship.RolePattern(v => v.Contractor),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SubContractorRelationship>())
            {
                @this.DeriveSubContractorRelationshipSecurityToken(validation);
            }
        }
    }

    public static class SubContractorRelationshipSecurityTokenRuleExtensions
    {
        public static void DeriveSubContractorRelationshipSecurityToken(this SubContractorRelationship @this, IValidation validation)
        {
            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
                @this.Contractor?.LocalAdministratorSecurityToken,
                @this.Contractor?.LocalEmployeeSecurityToken,
            };

            if (@this.ExistContractor)
            {
                @this.Contractor.DerivationTrigger = Guid.NewGuid();
            }
        }
    }
}
