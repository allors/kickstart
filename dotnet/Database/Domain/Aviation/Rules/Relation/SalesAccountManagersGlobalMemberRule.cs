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
    public class SalesAccountManagersGlobalMemberRule : Rule
    {
        public SalesAccountManagersGlobalMemberRule(MetaPopulation m) : base(m, new Guid("298fc57f-ac34-4404-a7a6-12909c58c214")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereLocalSalesAccountManager),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.DeriveSalesAccountManagersGlobalMember(validation);
            }
        }
    }

    public static class SalesAccountManagersGlobalMemberRuleExtensions
    {
        public static void DeriveSalesAccountManagersGlobalMember(this Person @this, IValidation validation)
        {
            if (@this.ExistInternalOrganisationsWhereLocalSalesAccountManager)
            {
                new UserGroups(@this.Strategy.Transaction).SalesAccountManagersGlobal.AddMember(@this);
            }
            else
            {
                new UserGroups(@this.Strategy.Transaction).SalesAccountManagersGlobal.RemoveMember(@this);
            }
        }
    }
}
