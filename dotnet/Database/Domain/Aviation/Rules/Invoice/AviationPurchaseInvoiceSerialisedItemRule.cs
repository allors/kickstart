// <copyright file="PurchaseInvoiceSerialisedItemDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Database.Derivations;
    using Allors.Database.Domain.Derivations.Rules;
    public class AviationPurchaseInvoiceSerialisedItemRule : Rule
    {
        public AviationPurchaseInvoiceSerialisedItemRule(MetaPopulation m) : base(m, new Guid("c2f7fb5e-78ee-467d-ab08-41f870148178")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.DeriveAviationPurchaseInvoiceSerialisedItem(validation);
            }
        }
    }

    public static class AviationPurchaseInvoiceSerialisedItemRuleExtensions
    {
        public static void DeriveAviationPurchaseInvoiceSerialisedItem(this PurchaseInvoice @this, IValidation validation)
        {
            if (@this.PurchaseInvoiceState.IsNotPaid)
            {
                foreach (PurchaseInvoiceItem invoiceItem in @this.ValidInvoiceItems)
                {
                    if (invoiceItem.ExistSerialisedItem
                        && @this.BilledTo.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Transaction()).PurchaseInvoiceConfirm)
                        && (invoiceItem.InvoiceItemType.IsPartItem || invoiceItem.InvoiceItemType.IsProductItem || invoiceItem.InvoiceItemType.IsGseUnmotorized))
                    {
                        if ((@this.BilledFrom as InternalOrganisation)?.IsInternalOrganisation == false)
                        {
                            invoiceItem.SerialisedItem.Buyer = @this.BilledTo;
                        }

                        // who comes first?
                        // Item you purchased can be on sold via sales invoice even before purchase invoice is created and confirmed!!
                        if (invoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate > new System.DateTime(2021, 07, 04)
                            && !invoiceItem.SerialisedItem.SalesInvoiceItemsWhereSerialisedItem.Any(v => (v.SalesInvoiceWhereSalesInvoiceItem.BillToCustomer as Organisation)?.IsInternalOrganisation == false
                            && v.SalesInvoiceWhereSalesInvoiceItem.SalesInvoiceType.Equals(new SalesInvoiceTypes(@this.Transaction()).SalesInvoice)
                            && !v.SalesInvoiceWhereSalesInvoiceItem.ExistSalesInvoiceWhereCreditedFromInvoice))
                        {
                            invoiceItem.SerialisedItem.OwnedBy = @this.BilledTo;
                            invoiceItem.SerialisedItem.Ownership = new Ownerships(@this.Transaction()).Own;
                        }
                    }
                }
            }
        }
    }
}