// <copyright file="CustomSerialisedItemPurchasePriceDerivation.cs" company="Allors bvba">
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

    public class AviationSerialisedItemPurchasePriceRule : Rule
    {
        public AviationSerialisedItemPurchasePriceRule(MetaPopulation m) : base(m, new Guid("807438b7-2de5-4cc3-9284-076fd6eaf1cc")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.PurchaseInvoice),
                m.SerialisedItem.RolePattern(v => v.AssignedBookValue),
                m.SerialisedItem.RolePattern(v => v.AssignedPurchasePrice),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveAviationSerialisedItemPurchasePrice(validation);
            }
        }
    }

    public static class AviationSerialisedItemPurchasePriceRuleExtensions
    {
        public static void DeriveAviationSerialisedItemPurchasePrice(this SerialisedItem @this, IValidation validation)
        {
            var purchasePrice = @this.PurchaseInvoiceItemsWhereSerialisedItem
                .Where(v => v.ExistInvoiceWhereValidInvoiceItem
                            && ((PurchaseInvoice)v.InvoiceWhereValidInvoiceItem).BilledFrom is Organisation organisation
                            && !organisation.IsInternalOrganisation
                            && (v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Strategy.Transaction).PartItem)
                                    || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Strategy.Transaction).ProductItem)
                                    || v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Strategy.Transaction).GseUnmotorized)))?
                .OrderBy(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate)
                .LastOrDefault()?
                .UnitPrice ?? 0M;

            var purchasePriceInPreferredCurrency = @this.ExistPurchaseInvoice
                ? Rounder.RoundDecimal(Currencies.ConvertCurrency(purchasePrice, @this.PurchaseInvoice.InvoiceDate, @this.PurchaseInvoice.DerivedCurrency, @this.PurchaseInvoice.BilledTo.PreferredCurrency), 2)
                : 0;

            @this.PurchasePrice = @this.AssignedPurchasePrice ?? purchasePriceInPreferredCurrency;

            @this.DerivedBookValue = @this.AssignedBookValue ?? @this.PurchasePrice;
        }
    }
}
