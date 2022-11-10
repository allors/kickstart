// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class AviationUserGroupInMembersRule : Rule
    {
        public AviationUserGroupInMembersRule(MetaPopulation m) : base(m, new Guid("804d7cb1-e294-4754-8952-bb734fa978f6")) =>
            this.Patterns = new Pattern[]
            {
                m.UserGroup.RolePattern(v => v.InMembers),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UserGroup>())
            {
                foreach(User user in @this.InMembers)
                {
                    @this.AddMember(user);
                }

                @this.RemoveInMembers();

                if (@this.ExistInternalOrganisationWhereBlueCollarWorkerUserGroup)
                {
                    @this.InternalOrganisationWhereBlueCollarWorkerUserGroup.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistInternalOrganisationWhereLocalAdministratorUserGroup)
                {
                    @this.InternalOrganisationWhereLocalAdministratorUserGroup.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistInternalOrganisationWhereLocalEmployeeUserGroup)
                {
                    @this.InternalOrganisationWhereLocalEmployeeUserGroup.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistInternalOrganisationWhereProductQuoteApproverUserGroup)
                {
                    @this.InternalOrganisationWhereProductQuoteApproverUserGroup.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistInternalOrganisationWherePurchaseInvoiceApproverUserGroup)
                {
                    @this.InternalOrganisationWherePurchaseInvoiceApproverUserGroup.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistInternalOrganisationWherePurchaseOrderApproverLevel1UserGroup)
                {
                    @this.InternalOrganisationWherePurchaseOrderApproverLevel1UserGroup.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistInternalOrganisationWherePurchaseOrderApproverLevel2UserGroup)
                {
                    @this.InternalOrganisationWherePurchaseOrderApproverLevel2UserGroup.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistInternalOrganisationWhereStockManagerUserGroup)
                {
                    @this.InternalOrganisationWhereStockManagerUserGroup.DerivationTrigger = Guid.NewGuid();
                }
            }
        }
    }
}
