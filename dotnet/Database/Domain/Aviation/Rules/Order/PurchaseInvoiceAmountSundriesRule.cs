// <copyright file="PurchaseOrderAmountSundriesRule.cs" company="Allors bvba">
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

    public class PurchaseOrderInvoiceSundriesRule : Rule
    {
        public PurchaseOrderInvoiceSundriesRule(MetaPopulation m) : base(m, new Guid("94805a79-ae90-4ca5-b4d3-19519a09bf70")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.DerivationTrigger),
                m.PurchaseInvoice.RolePattern(v => v.ValidInvoiceItems),
                m.PurchaseInvoiceItem.RolePattern(v => v.Part, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.Quantity, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.AssignedUnitPrice, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.NonUnifiedPart.RolePattern(v => v.IsSundries, v => v.PurchaseInvoiceItemsWherePart.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.DerivePurchaseOrderAmountSundries(validation);
            }
        }
    }

    public static class PurchaseOrderAmountSundriesRuleExtensions
    {
        public static void DerivePurchaseOrderAmountSundries(this PurchaseInvoice @this, IValidation validation)
        {
            foreach (PurchaseInvoiceItem invoiceItem in @this.ValidInvoiceItems)
            {
                if (invoiceItem.ExistPart && invoiceItem.Part.GetType().Name.Equals(typeof(NonUnifiedPart).Name))
                {
                    @this.AmountSundries += invoiceItem.TotalExVat;
                }
            }
        }
    }
}
