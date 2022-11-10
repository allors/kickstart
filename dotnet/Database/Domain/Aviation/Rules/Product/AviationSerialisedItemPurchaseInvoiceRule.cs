// <copyright file="CustomSerialisedItemPurchaseInvoiceDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Allors.Database.Domain.Derivations.Rules;

    public class AviationSerialisedItemPurchaseInvoiceRule : Rule
    {
        public AviationSerialisedItemPurchaseInvoiceRule(MetaPopulation m) : base(m, new Guid("28569756-4cca-41dc-8318-4c96e0f8e446")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.ValidInvoiceItems, v => v.PurchaseInvoiceItems.PurchaseInvoiceItem.SerialisedItem),
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState, v => v.PurchaseInvoiceItems.PurchaseInvoiceItem.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.PurchaseInvoice = @this.PurchaseInvoiceItemsWhereSerialisedItem
                    .Where(v => v.ExistInvoiceWhereValidInvoiceItem
                                && ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).BilledFrom is Organisation organisation
                                && !organisation.IsInternalOrganisation
                                && (((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(transaction).NotPaid)
                                    || ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(transaction).PartiallyPaid)
                                    || ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(transaction).Paid))
                                && (v.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).PartItem)
                                    || v.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).ProductItem)
                                    || v.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).GseUnmotorized)))?
                    .OrderBy(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate)
                    .LastOrDefault()?
                    .PurchaseInvoiceWherePurchaseInvoiceItem;
            }
        }
    }
}
