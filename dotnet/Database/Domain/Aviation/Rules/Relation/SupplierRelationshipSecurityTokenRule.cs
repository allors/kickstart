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
    public class SupplierRelationshipSecurityTokenRule : Rule
    {
        public SupplierRelationshipSecurityTokenRule(MetaPopulation m) : base(m, new Guid("12a991b4-b960-42a7-8c61-8c9ce82b62c7")) =>
            this.Patterns = new Pattern[]
            {
                m.SupplierRelationship.RolePattern(v => v.InternalOrganisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SupplierRelationship>())
            {
                @this.DeriveSupplierRelationshipSecurityToken(validation);
            }
        }
    }

    public static class SupplierRelationshipSecurityTokenRuleExtensions
    {
        public static void DeriveSupplierRelationshipSecurityToken(this SupplierRelationship @this, IValidation validation)
        {
            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
                @this.InternalOrganisation?.LocalAdministratorSecurityToken,
                @this.InternalOrganisation?.LocalEmployeeSecurityToken,
            };

            if (@this.ExistInternalOrganisation)
            {
                @this.InternalOrganisation.DerivationTrigger = Guid.NewGuid();
            }
        }
    }
}
