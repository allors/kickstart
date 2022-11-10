// <copyright file="LocalEmployeesMemberRule.cs" company="Allors bvba">
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
    public class LocalEmployeesMemberRule : Rule
    {
        public LocalEmployeesMemberRule(MetaPopulation m) : base(m, new Guid("3cbf1ba6-9d00-4008-9da9-25c342ab69f1")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereBlueCollarWorker),
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereLocalAdministrator),
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereProductQuoteApprover),
                m.Person.AssociationPattern(v => v.InternalOrganisationsWherePurchaseInvoiceApprover),
                m.Person.AssociationPattern(v => v.InternalOrganisationsWherePurchaseOrderApproversLevel1),
                m.Person.AssociationPattern(v => v.InternalOrganisationsWherePurchaseOrderApproversLevel2),
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereStockManager),
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereActiveEmployee),
                m.Employment.RolePattern(v => v.FromDate, v => v.Employee),
                m.Employment.RolePattern(v => v.ThroughDate, v => v.Employee),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.DeriveLocalEmployeesMember(validation);
            }
        }
    }

    public static class LocalEmployeesMemberRuleExtensions
    {
        public static void DeriveLocalEmployeesMember(this Person @this, IValidation validation)
        {
            var internalOrganisations = @this.Strategy.Transaction.Extent<Organisation>().Where(v => v.IsInternalOrganisation).ToArray();
            foreach (InternalOrganisation internalOrganisation in internalOrganisations)
            {
                if ((internalOrganisation.BlueCollarWorkers.Contains(@this)
                    || internalOrganisation.LocalAdministrators.Contains(@this)
                    || internalOrganisation.ProductQuoteApprovers.Contains(@this)
                    || internalOrganisation.PurchaseInvoiceApprovers.Contains(@this)
                    || internalOrganisation.PurchaseOrderApproversLevel1.Contains(@this)
                    || internalOrganisation.PurchaseOrderApproversLevel2.Contains(@this)
                    || internalOrganisation.StockManagers.Contains(@this))
                    && !internalOrganisation.LocalEmployees.Contains(@this))
                {
                    internalOrganisation.AddLocalEmployee(@this);
                }
            }
        }
    }
}
