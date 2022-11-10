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

    public class AviationUserGroupOutMembersRule : Rule
    {
        public AviationUserGroupOutMembersRule(MetaPopulation m) : base(m, new Guid("8eeb7f29-56da-4483-a9b2-7943099f48d0")) =>
            this.Patterns = new Pattern[]
            {
                m.UserGroup.RolePattern(v => v.OutMembers),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UserGroup>())
            {
                foreach(User user in @this.OutMembers)
                {
                    @this.RemoveMember(user);
                }

                @this.RemoveOutMembers();

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
