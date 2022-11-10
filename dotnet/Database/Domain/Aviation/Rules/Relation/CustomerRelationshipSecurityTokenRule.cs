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
    public class CustomerRelationshipSecurityTokenRule : Rule
    {
        public CustomerRelationshipSecurityTokenRule(MetaPopulation m) : base(m, new Guid("f23a522b-a597-4097-a557-c0f43f58f127")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerRelationship.RolePattern(v => v.InternalOrganisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CustomerRelationship>())
            {
                @this.DeriveCustomerRelationshipSecurityToken(validation);
            }
        }
    }

    public static class CustomerRelationshipSecurityTokenRuleExtensions
    {
        public static void DeriveCustomerRelationshipSecurityToken(this CustomerRelationship @this, IValidation validation)
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
